# Travel MCP Server (Python)

This Model Context Protocol (MCP) server keeps the scope intentionally tight—just flight shopping and hotel searches.  It mirrors the service that the C# client consumes, but packaged so the Python orchestrator can launch it locally through stdio.

## Features

- **Flight offers** (`[FLIGHT] search_flight_offers`) – provide origin, destination, dates, passenger count, and optional cabin class.  City names ("Bucharest") are resolved to IATA codes automatically.
- **Hotel offers** (`[HOTEL] search_hotel_offers`) – provide a city (or code) plus check-in/check-out dates to get representative prices.

Debug logs are emitted to stderr with the prefix `[DEBUG][TravelMCP]` in orange ANSI text.  Set `TRAVEL_MCP_DEBUG=0` if you want a quiet run (colors included).

## Requirements

1. Python 3.11+
2. Amadeus for Developers credentials
3. Packages listed in `requirements.txt` (shared with the parent folder)

## Configuration

Populate `.env` one level up (or create a local `.env`) with:

```env
AMADEUS_CLIENT_ID=your-client-id
AMADEUS_CLIENT_SECRET=your-client-secret
AMADEUS_ENVIRONMENT=test  # or production
TRAVEL_MCP_DEBUG=1        # optional, defaults to 1
```

Legacy names `AMADEUS_API_KEY / AMADEUS_API_SECRET` also work for compatibility with older samples.

## Run standalone (optional)

```bash
cd Student/Resources/Challenge-08/python/travel_mcp_server
python server.py
```

During normal development the orchestrator spawns the server automatically, but running it manually is useful for quick MCP CLI experiments (`mcp dev server.py`).
