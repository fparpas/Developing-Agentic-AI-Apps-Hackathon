# Travel MCP Server (Python)

This Model Context Protocol (MCP) server intentionally focuses on the two
capabilities learners touch first: flight shopping and hotel searches. Keeping
the surface area small makes it easy to trace how Amadeus SDK calls travel
through MCP tools into the Microsoft Agent Framework coordinator.

## Features

- **Flight offers** (`[FLIGHT] search_flight_offers`) – provide origin, destination,
  travel dates, passenger count, and optional cabin class to retrieve live
  pricing. City names (e.g., “Bucharest”) are automatically resolved to IATA codes.
- **Hotel offers** (`[HOTEL] search_hotel_offers`) – provide a city code (or city name)
  plus check-in/check-out dates to list sample properties and prices.

Both tools print basic `[DEBUG][TravelMCP]` traces (arguments, resolved codes, result counts)
so you can see the exact inputs/outputs while testing. Set `TRAVEL_MCP_DEBUG=0`
to disable these logs once you are confident everything works.

That’s it—no caching, no filtering utilities, just the smallest amount of code
needed to make a real Amadeus call and format a friendly response.

## Requirements

1. Python 3.11+
2. Amadeus for Developers credentials
3. The packages listed in `requirements.txt`

Install dependencies in an isolated environment:

```bash
cd Coach/Solutions/Challenge-08/python/travel_mcp_server
python -m venv .venv
source .venv/bin/activate  # Windows: .venv\Scripts\activate
pip install -r requirements.txt
```

## Configuration

Create a `.env` file next to this README (or rely on the parent `.env`) with
Amadeus settings:

```env
AMADEUS_CLIENT_ID=your-client-id
AMADEUS_CLIENT_SECRET=your-client-secret
# Optional: production or test (defaults to test)
AMADEUS_ENVIRONMENT=test
```

The server also honours the legacy names `AMADEUS_API_KEY` / `AMADEUS_API_SECRET`
for compatibility with earlier samples.

## Running Locally

The orchestrator launches this server through stdio, but you can inspect the
available tools manually with the MCP CLI:

```bash
mcp dev server.py
```

## Tool Reference

| Tag | Tool Name | Purpose |
|-----|-----------|---------|
| `[FLIGHT]` | `search_flight_offers` | Retrieve up to five priced itineraries for a route |
| `[HOTEL]` | `search_hotel_offers` | List hotel options for a city code and date range |

Each function returns a plain-text list so you can see exactly what the agent
received without digging through JSON.

## Next Steps

- Reintroduce additional tools (reference data, activities, transfers) once
  learners are comfortable with the basics.
- Teach error handling patterns—retry logic, rate-limit messaging, etc.
- Add unit tests that stub the Amadeus client for offline validation.
