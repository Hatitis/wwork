# SDP Runbook

## Health check
- API: `GET /health`
- Expect: `{ status: "ok", db: "up" }`

## Common ops
- Rebuild stack: `docker compose up --build`
- Reset database volume: `docker compose down -v`

## Troubleshooting
- If migrations fail, verify `ConnectionStrings__SdpDb` and Postgres readiness.
- If simulation returns 422, check cycle detection message in response.
