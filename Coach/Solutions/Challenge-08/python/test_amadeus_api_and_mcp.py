"""Quick regression test for the travel MCP server.

The script performs identical flight and hotel searches:

1. Calls the Amadeus API directly via the official SDK.
2. Calls the matching MCP tools exposed in ``travel_mcp_server/server.py``.

Use it to confirm your credentials and the server wiring before launching the
full multi-agent planner.
"""

# Not needed for Python 3.11+, it's the default
from __future__ import annotations

import asyncio
import os
import sys
from pathlib import Path
from typing import Any, Dict, List

from amadeus import Client, ResponseError
from dotenv import load_dotenv
from mcp import ClientSession, StdioServerParameters
from mcp.client.stdio import stdio_client

load_dotenv()

ORIGIN = "OTP"
DESTINATION = "SEA"
DEPARTURE_DATE = "2026-06-11"
ADULTS = 2
TRAVEL_CLASS = "BUSINESS"

HOTEL_CITY = DESTINATION
HOTEL_CHECK_IN_DATE = DEPARTURE_DATE
HOTEL_CHECK_OUT_DATE = "2026-06-20"  # few nights after arrival


def get_amadeus_client() -> Client:
    """Create the Amadeus SDK client using standard environment variables."""

    client_id = os.getenv("AMADEUS_CLIENT_ID") or os.getenv("AMADEUS_API_KEY")
    client_secret = os.getenv("AMADEUS_CLIENT_SECRET") or os.getenv("AMADEUS_API_SECRET")
    environment = (os.getenv("AMADEUS_ENVIRONMENT") or "test").lower()

    if not client_id or not client_secret:
        raise RuntimeError("Set AMADEUS_CLIENT_ID and AMADEUS_CLIENT_SECRET before running this test.")

    hostname = "production" if environment in {"prod", "production"} else "test"
    return Client(client_id=client_id, client_secret=client_secret, hostname=hostname)


def summarize_flight_offers(offers: List[Dict[str, Any]]) -> str:
    """Convert the first few flight offers into printable lines."""

    if not offers:
        return "No flight offers returned. Check your inputs or try different dates."

    lines = []
    for offer in offers[:3]:
        price = offer.get("price", {})
        itineraries = offer.get("itineraries", [])
        segments = itineraries[0].get("segments", []) if itineraries else []
        route = "route unavailable"
        if segments:
            first, last = segments[0], segments[-1]
            route = f"{first['departure']['iataCode']} → {last['arrival']['iataCode']}"
        cabin = offer.get("travelerPricings", [{}])[0].get("fareDetailsBySegment", [{}])[0].get("cabin", "n/a")
        lines.append(
            f"{route} | {price.get('total', '?')} {price.get('currency', '')} | Cabin: {cabin}"
        )
    return "\n".join(lines)


def summarize_hotel_offers(results: List[Dict[str, Any]]) -> str:
    """Summarize hotel offers for easier inspection."""

    if not results:
        return "No hotel offers returned. Try adjusting the city or dates."

    lines = []
    for result in results[:5]:
        hotel = result.get("hotel", {})
        offer = result.get("offers", [{}])[0]
        price = offer.get("price", {})
        lines.append(
            f"{hotel.get('name', 'Hotel')} (ID {hotel.get('hotelId', '?')}) | "
            f"{price.get('total', '?')} {price.get('currency', '')} | "
            f"Check-in {offer.get('checkInDate', HOTEL_CHECK_IN_DATE)}"
        )
    return "\n".join(lines)


def response_error_detail(exc: ResponseError) -> str:
    body = getattr(exc, "body", {}) or {}
    errors = body.get("errors") if isinstance(body, dict) else None  # type: ignore[assignment]
    detail = str(exc)
    if isinstance(errors, list) and errors:
        detail = errors[0].get("detail", detail)
    return detail


def run_direct_flight_call() -> str:
    """Hit Amadeus directly for flights and return a formatted summary."""

    client = get_amadeus_client()
    try:
        response = client.shopping.flight_offers_search.get(
            originLocationCode=ORIGIN,
            destinationLocationCode=DESTINATION,
            departureDate=DEPARTURE_DATE,
            adults=ADULTS,
            travelClass=TRAVEL_CLASS,
            max=5,
        )
    except ResponseError as exc:  # pragma: no cover - depends on live service
        raise RuntimeError(f"Direct flight call failed: {response_error_detail(exc)}")

    data = getattr(response, "data", None)
    offers: List[Dict[str, Any]] = data if isinstance(data, list) else []
    return summarize_flight_offers(offers)


def run_direct_hotel_call() -> str:
    """Run a hotel lookup through the Amadeus SDK."""

    client = get_amadeus_client()
    try:
        city_response = client.reference_data.locations.hotels.by_city.get(cityCode=HOTEL_CITY)
        city_hotels = getattr(city_response, "data", None) or []
        hotel_ids: List[str] = []
        for candidate in city_hotels:
            code = candidate.get("hotelId") or candidate.get("hotel", {}).get("hotelId")
            if code:
                hotel_ids.append(str(code))
            if len(hotel_ids) >= 20:
                break

        if not hotel_ids:
            return "No hotels returned by the city lookup."

        offers_response = client.shopping.hotel_offers_search.get(
            hotelIds=",".join(hotel_ids),
            checkInDate=HOTEL_CHECK_IN_DATE,
            checkOutDate=HOTEL_CHECK_OUT_DATE,
            adults=ADULTS,
        )
    except ResponseError as exc:
        raise RuntimeError(f"Direct hotel call failed: {response_error_detail(exc)}")

    data = getattr(offers_response, "data", None)
    hotels: List[Dict[str, Any]] = data if isinstance(data, list) else []
    return summarize_hotel_offers(hotels)


async def call_mcp_tool(tool_name: str, arguments: Dict[str, Any]) -> str:
    """Launch the MCP server, invoke a tool, and return its text output."""

    server_path = Path(__file__).parent / "travel_mcp_server" / "server.py"
    if not server_path.exists():
        raise FileNotFoundError(f"Cannot locate MCP server script at {server_path}")

    server_params = StdioServerParameters(command=sys.executable, args=[str(server_path)])

    async with stdio_client(server_params) as (read_stream, write_stream):
        async with ClientSession(read_stream, write_stream) as session:
            await session.initialize()
            result = await session.call_tool(
                name=tool_name,
                arguments=arguments,
            )

    texts = [item.text for item in result.content if hasattr(item, "text")]
    return "\n".join(texts) if texts else "MCP call succeeded but returned no text content."


async def run_mcp_flight_call() -> str:
    return await call_mcp_tool(
        "search_flight_offers",
        {
            "origin": ORIGIN,
            "destination": DESTINATION,
            "departure_date": DEPARTURE_DATE,
            "adults": ADULTS,
            "travel_class": TRAVEL_CLASS,
        },
    )


async def run_mcp_hotel_call() -> str:
    return await call_mcp_tool(
        "search_hotel_offers",
        {
            "city_code": HOTEL_CITY,
            "check_in_date": HOTEL_CHECK_IN_DATE,
            "check_out_date": HOTEL_CHECK_OUT_DATE,
            "adults": ADULTS,
        },
    )


async def main() -> None:
    print("== Direct Amadeus SDK flight call ==")
    try:
        print(run_direct_flight_call())
    except Exception as exc:  # pragma: no cover - diagnostic script
        print(f"[ERROR] {exc}")

    print("\n== Direct Amadeus SDK hotel call ==")
    try:
        print(run_direct_hotel_call())
    except Exception as exc:
        print(f"[ERROR] {exc}")

    print("\n== MCP server flight call ==")
    try:
        print(await run_mcp_flight_call())
    except Exception as exc:
        print(f"[ERROR] {exc}")

    print("\n== MCP server hotel call ==")
    try:
        print(await run_mcp_hotel_call())
    except Exception as exc:
        print(f"[ERROR] {exc}")


if __name__ == "__main__":
    asyncio.run(main())
