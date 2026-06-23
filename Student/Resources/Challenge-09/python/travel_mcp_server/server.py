"""Basic Travel MCP server using the Amadeus Python SDK.

Two MCP tools expose a minimal subset of travel functionality backed by the
official Amadeus client:

- `[FLIGHT] search_flight_offers` – basic flight shopping
- `[HOTEL] search_hotel_offers` – simple hotel lookups by city/dates
"""

from __future__ import annotations

import asyncio
import os
import sys
from dataclasses import dataclass
from datetime import datetime
from functools import lru_cache
from typing import Any, Dict, List, Iterable, Set

from amadeus import Client, ResponseError, Location
from dotenv import load_dotenv
from mcp.server.fastmcp import FastMCP

load_dotenv()


@dataclass(frozen=True)
class ServerSettings:
    debug_enabled: bool

    @classmethod
    def from_env(cls) -> "ServerSettings":
        debug_flag = os.getenv("TRAVEL_MCP_DEBUG", "1").lower() not in {"0", "false", "no"}
        return cls(debug_enabled=debug_flag)


@dataclass(frozen=True)
class AmadeusConfig:
    client_id: str
    client_secret: str
    hostname: str


mcp = FastMCP("travel")
SETTINGS = ServerSettings.from_env()
ANSI_ORANGE = "\033[38;5;208m"
ANSI_RESET = "\033[0m"


def debug(message: str) -> None:
    """Emit debug output without polluting the MCP stdio channel."""

    if SETTINGS.debug_enabled:
        print(f"{ANSI_ORANGE}[DEBUG][TravelMCP] {message}{ANSI_RESET}", file=sys.stderr, flush=True)


def coerce_positive_int(value: Any, *, fallback: int | None, label: str) -> int | None:
    """Convert a loosely typed input into a positive integer."""

    if value in (None, ""):
        return fallback

    try:
        parsed = int(value)
    except (TypeError, ValueError):
        debug(f"[{label}] Unable to parse integer from value={value!r}; using {fallback}")
        return fallback

    if parsed < 1:
        debug(f"[{label}] Parsed value {parsed} is < 1; using {fallback}")
        return fallback

    return parsed


def normalize_travel_class(value: str | None) -> str | None:
    """Upper-case the travel class and validate it against Amadeus options."""

    if not value:
        return None

    cleaned = value.strip().upper()
    if cleaned not in {"ECONOMY", "PREMIUM_ECONOMY", "BUSINESS", "FIRST"}:
        debug(f"[FLIGHT] Unknown travel class '{value}'; omitting from request")
        return None

    return cleaned


@lru_cache(maxsize=1)
def get_amadeus_config() -> AmadeusConfig:
    """Load Amadeus credentials from environment variables once."""

    client_id = os.getenv("AMADEUS_CLIENT_ID") or os.getenv("AMADEUS_API_KEY")
    client_secret = os.getenv("AMADEUS_CLIENT_SECRET") or os.getenv("AMADEUS_API_SECRET")
    environment = (os.getenv("AMADEUS_ENVIRONMENT") or "test").lower()

    if not client_id or not client_secret:
        raise RuntimeError(
            "Set AMADEUS_CLIENT_ID and AMADEUS_CLIENT_SECRET (or the legacy API key names)"
        )

    hostname = "production" if environment in {"prod", "production"} else "test"
    return AmadeusConfig(client_id=client_id, client_secret=client_secret, hostname=hostname)


@lru_cache(maxsize=1)
def get_amadeus_client() -> Client:
    """Create (or reuse) the Amadeus client using cached configuration."""

    cfg = get_amadeus_config()
    return Client(client_id=cfg.client_id, client_secret=cfg.client_secret, hostname=cfg.hostname)


def describe_response_error(exc: ResponseError) -> tuple[str, Dict[str, Any], Any]:
    """Return the textual detail, response body, and status code from an SDK error."""

    body = getattr(exc, "body", {}) or {}
    response_obj = getattr(exc, "response", None)
    if not body and response_obj is not None:
        body = getattr(response_obj, "result", None) or getattr(response_obj, "data", {})
    errors = body.get("errors") if isinstance(body, dict) else None  # type: ignore[assignment]
    detail = str(exc)
    if isinstance(errors, list) and errors:
        detail = errors[0].get("detail", detail)
    status_code = getattr(exc, "status_code", getattr(response_obj, "status_code", "unknown"))
    return detail, body, status_code


def format_timestamp(value: str | None) -> str:
    """Convert ISO timestamps to `YYYY-MM-DD HH:MM` for readability."""

    if not value:
        return "?"
    cleaned = value.replace("Z", "+00:00")
    try:
        parsed = datetime.fromisoformat(cleaned)
        return parsed.strftime("%Y-%m-%d %H:%M")
    except ValueError:
        return value


def describe_airlines(codes: Iterable[str], name_lookup: Dict[str, str]) -> str:
    """Return a human-friendly representation of airline codes using known names."""

    unique_codes = sorted({code for code in codes if code})
    if not unique_codes:
        return "Airline: n/a"

    pieces = []
    for code in unique_codes[:3]:
        name = name_lookup.get(code)
        pieces.append(f"{name} ({code})" if name else code)

    remainder = len(unique_codes) - len(pieces)
    if remainder > 0:
        pieces.append(f"+{remainder} more")

    return " ".join(pieces)


async def resolve_airline_names(codes: Set[str], client: Client) -> Dict[str, str]:
    """Fetch airline names for a small set of carrier codes."""

    normalized = sorted({code for code in codes if code})
    if not normalized:
        return {}

    # Limit lookups to prevent over-fetching; 10 covers our 3-offer summary easily.
    batch = normalized[:10]
    try:
        records = await call_amadeus(
            client.reference_data.airlines.get,
            airlineCodes=",".join(batch),
        )
    except RuntimeError:
        return {}

    names: Dict[str, str] = {}
    for record in records:
        code = record.get("iataCode")
        name = record.get("businessName") or record.get("commonName") or record.get("officialName")
        if code and name:
            names[code.upper()] = name
    return names


async def call_amadeus(func: Any, **params: Any) -> List[Dict[str, Any]]:
    """Run blocking SDK calls on a worker and return the raw data list."""

    params = {k: v for k, v in params.items() if v not in (None, "")}
    debug(f"Calling {getattr(func, '__qualname__', str(func))} with params={params}")
    try:
        response = await asyncio.to_thread(func, **params)
    except ResponseError as exc:  # pragma: no cover - depends on network
        detail, body, status_code = describe_response_error(exc)
        debug(
            "Amadeus ResponseError status=%s body=%s attrs=%s"
            % (status_code, body, exc.__dict__)
        )
        raise RuntimeError(f"Amadeus error: {detail}")
    except Exception as exc:  # pragma: no cover - unexpected network failures
        raise RuntimeError(f"Amadeus request failed: {exc}")

    data = getattr(response, "data", None)
    if data is None:
        debug("No data field on response; returning empty list")
        return []
    records = data if isinstance(data, list) else [data]
    debug(f"Received {len(records)} record(s) from {getattr(func, '__qualname__', str(func))}")
    return records


def format_flights(offers: List[Dict[str, Any]], airline_names: Dict[str, str] | None = None) -> str:
    """Turn a handful of flight offers into human-friendly lines."""

    if not offers:
        return "No flight offers returned. Try adjusting your dates or airports."

    lines = []
    for offer in offers[:3]:
        price = offer.get("price", {})
        itineraries = offer.get("itineraries", [])
        route_bits = []
        time_bits = []
        carriers: Set[str] = set()

        for itinerary in itineraries:
            segments = itinerary.get("segments", [])
            if not segments:
                continue
            first, last = segments[0], segments[-1]
            path_codes = []
            origin_code = first.get("departure", {}).get("iataCode")
            if origin_code:
                path_codes.append(origin_code)
            for segment in segments:
                arrival_code = segment.get("arrival", {}).get("iataCode")
                if arrival_code:
                    path_codes.append(arrival_code)
            if len(path_codes) >= 2:
                route_bits.append(" → ".join(path_codes))
            else:
                route_bits.append(
                    f"{first.get('departure', {}).get('iataCode', '?')} → {last.get('arrival', {}).get('iataCode', '?')}"
                )
            time_bits.append(
                f"{format_timestamp(first['departure'].get('at'))} → {format_timestamp(last['arrival'].get('at'))}"
            )
            for segment in segments:
                carrier = segment.get("carrierCode") or segment.get("operating", {}).get("carrierCode")
                if carrier:
                    carriers.add(str(carrier).upper())

        routes = "; ".join(
            f"{route} ({time})" for route, time in zip(route_bits, time_bits)
        ) or "route unavailable"
        airlines_text = describe_airlines(carriers, airline_names or {})
        cabin = (
            offer.get("travelerPricings", [{}])[0]
            .get("fareDetailsBySegment", [{}])[0]
            .get("cabin", "n/a")
        )
        lines.append(
            f"{routes} | {airlines_text} | {price.get('total', '?')} {price.get('currency', '')} | Cabin: {cabin}"
        )
    return "\n".join(lines)


def format_hotels(results: List[Dict[str, Any]]) -> str:
    """Summarise hotel offers for quick comparison."""

    if not results:
        return "No hotel offers found. Try a different city code or date range."

    lines = []
    for result in results[:5]:
        hotel = result.get("hotel", {})
        offer = result.get("offers", [{}])[0]
        lines.append(
            f"{hotel.get('name', 'Hotel')} (ID {hotel.get('hotelId', '?')}) | "
            f"{offer.get('price', {}).get('total', '?')} {offer.get('price', {}).get('currency', '')} "
            f"| Check-in {offer.get('checkInDate')}"
        )
    return "\n".join(lines)


async def resolve_location_code(raw_value: str, client: Client, *, prefer_city: bool = False) -> str:
    """Accept either an IATA code or a city/airport name and return an IATA code."""

    if not raw_value:
        raise RuntimeError("Missing location input")

    cleaned = raw_value.strip().upper()
    if len(cleaned) == 3 and cleaned.isalpha():
        debug(f"Resolved '{raw_value}' as direct IATA code '{cleaned}'")
        return cleaned

    results = await call_amadeus(
        client.reference_data.locations.get,
        keyword=raw_value.strip(),
        subType=Location.ANY,
    )

    if prefer_city:
        for location in results:
            if location.get("subType") == "CITY" and location.get("iataCode"):
                city_code = location["iataCode"].upper()
                debug(f"Resolved '{raw_value}' to city code '{city_code}'")
                return city_code

    for location in results:
        code = location.get("iataCode")
        if code:
            resolved = code.upper()
            debug(f"Resolved '{raw_value}' to code '{resolved}'")
            return resolved

    raise RuntimeError(
        f"Could not resolve '{raw_value}' to an IATA code. Provide the 3-letter code explicitly."
    )


@mcp.tool(description="[FLIGHT] Search simple flight offers")
async def search_flight_offers(
    origin: str,
    destination: str,
    departure_date: str,
    return_date: str | None = None,
    adults: int = 1,
    travel_class: str | None = None,
    extras: Any | None = None,
) -> str:
    """Fetch basic flight offers between two airports using Amadeus shopping."""

    client = get_amadeus_client()
    adult_count = coerce_positive_int(adults, fallback=1, label="FLIGHT:adults") or 1
    cabin = normalize_travel_class(travel_class)
    if extras not in (None, "", {}):
        debug(f"[FLIGHT] Ignoring unsupported arguments: {extras}")
    debug(
        f"[FLIGHT] origin={origin} destination={destination} departure={departure_date} "
        f"return={return_date} adults={adult_count} class={cabin or travel_class}"
    )
    try:
        origin_code = await resolve_location_code(origin, client)
        destination_code = await resolve_location_code(destination, client)
        offers = await call_amadeus(
            client.shopping.flight_offers_search.get,
            originLocationCode=origin_code,
            destinationLocationCode=destination_code,
            departureDate=departure_date,
            returnDate=return_date,
            adults=adult_count,
            travelClass=cabin,
            max=5,
        )
        airline_codes: Set[str] = set()
        for offer in offers:
            for itinerary in offer.get("itineraries", []):
                for segment in itinerary.get("segments", []):
                    carrier = segment.get("carrierCode") or segment.get("operating", {}).get("carrierCode")
                    if carrier:
                        airline_codes.add(str(carrier).upper())
        airline_names = await resolve_airline_names(airline_codes, client)
    except RuntimeError as exc:
        debug(f"[FLIGHT] Error: {exc}")
        return f"[FLIGHT] Unable to retrieve offers: {exc}"

    summary = format_flights(offers, airline_names)
    debug(f"[FLIGHT] Returning {len(offers)} offer(s)")
    return summary


@mcp.tool(description="[HOTEL] Search hotel offers by city code")
async def search_hotel_offers(
    city_code: str,
    check_in_date: str,
    check_out_date: str,
    adults: int = 1,
    room_quantity: int | None = None,
    extras: Any | None = None,
) -> str:
    """Use the hotel offers search endpoint with minimal required inputs."""

    client = get_amadeus_client()
    adult_count = coerce_positive_int(adults, fallback=1, label="HOTEL:adults") or 1
    rooms = coerce_positive_int(room_quantity, fallback=None, label="HOTEL:rooms")
    if extras not in (None, "", {}):
        debug(f"[HOTEL] Ignoring unsupported arguments: {extras}")
    if rooms:
        debug(f"[HOTEL] room_quantity override: {rooms}")
    debug(
        f"[HOTEL] city={city_code} check_in={check_in_date} check_out={check_out_date} "
        f"adults={adult_count} room_quantity={rooms}"
    )

    try:
        normalized_city = await resolve_location_code(city_code, client, prefer_city=True)
        lookup_params: Dict[str, Any] = {"cityCode": normalized_city}
        city_hotels = await call_amadeus(
            client.reference_data.locations.hotels.by_city.get,
            **lookup_params,
        )

        hotel_ids: List[str] = []
        for candidate in city_hotels:
            code = candidate.get("hotelId") or candidate.get("hotel", {}).get("hotelId")
            if code:
                hotel_ids.append(str(code))
            if len(hotel_ids) >= 20:
                break

        if not hotel_ids:
            debug(f"[HOTEL] No hotels found in city {normalized_city}")
            return "[HOTEL] Unable to find hotels for that city. Try a different location."

        params: Dict[str, Any] = {
            "hotelIds": ",".join(hotel_ids),
            "checkInDate": check_in_date,
            "checkOutDate": check_out_date,
            "adults": adult_count,
        }
        if rooms:
            params["roomQuantity"] = rooms

        hotels = await call_amadeus(
            client.shopping.hotel_offers_search.get,
            **params,
        )
    except RuntimeError as exc:
        debug(f"[HOTEL] Error: {exc}")
        return f"[HOTEL] Unable to retrieve offers: {exc}"

    summary = format_hotels(hotels)
    debug(f"[HOTEL] Returning {len(hotels)} result(s)")
    return summary


def main() -> None:
    """Run the MCP server over stdio."""

    get_amadeus_client()  # fail fast if credentials are missing
    mcp.run(transport="stdio")


if __name__ == "__main__":  # pragma: no cover - manual execution path
    main()
