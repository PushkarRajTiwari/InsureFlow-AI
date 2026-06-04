# InsureFlow AI

Demo-quality Phase 1 MVP for independent insurance agencies to import Gmail messages, classify them with AI, and review the results in a dashboard.

## Scope

Built:

- Google OAuth sign-in
- Gmail mailbox connection
- Latest 100 email import
- Duplicate prevention by Gmail message id
- AI classification into the approved insurance categories
- Dashboard stats, filters, search, email table, and email detail page
- JWT-protected ASP.NET Core API
- PostgreSQL persistence with EF Core migrations
- Docker Compose local development

Not built:

- Email sending
- Billing
- CRM
- Outlook
- Mobile app
- Admin portal

## Structure

```text
api/
  src/
    InsureFlow.Api/             ASP.NET Core 9 Web API
    InsureFlow.Application/     DTOs and service abstractions
    InsureFlow.Domain/          Entities and enums
    InsureFlow.Infrastructure/  EF Core, Gmail, OpenAI, auth, encryption
frontend/
  src/app/                      Next.js App Router pages
  src/components/               App and shadcn-style UI components
docker-compose.yml
```

## Local Setup

1. Create an environment file:

```bash
cp .env.example .env
```

2. Fill in `.env`.

Generate `ENCRYPTION_KEY` with:

```bash
openssl rand -base64 32
```

Use a long random value for `JWT_SECRET`.

3. Configure Google Cloud:

- Create an OAuth 2.0 Web Client.
- Add `http://localhost:3001` as an authorized JavaScript origin.
- Enable the Gmail API.
- Use these scopes in consent review/testing:
  - `openid`
  - `email`
  - `profile`
  - `https://www.googleapis.com/auth/gmail.readonly`

The frontend uses the Google Identity Services popup authorization-code flow. The API exchanges codes with `Google__RedirectUri=postmessage`.

4. Start everything:

```bash
docker compose up --build
```

5. Open:

- Frontend: http://localhost:3001
- API Swagger: http://localhost:8080/swagger
- Postgres: `localhost:5433`

## Required Flow

1. Sign in with Google.
2. Click `Connect Gmail` and grant Gmail read access.
3. Click `Sync Inbox`.
4. The API imports up to 100 Gmail messages, avoids duplicates, and classifies new messages with OpenAI.
5. Review results in the dashboard table.
6. Click an email subject to view the original preview and AI category, confidence, and reasoning.

## API Endpoints

- `POST /api/auth/google`
- `POST /api/mailbox/connect`
- `POST /api/emails/sync`
- `GET /api/emails`
- `GET /api/emails/{id}`
- `GET /api/dashboard/stats`

## Classification Categories

- Policy Change
- Claim
- Billing
- Coverage Question
- Renewal
- Proof Of Insurance
- General Inquiry
- Spam

## Notes

- Refresh tokens and access tokens are encrypted before storage.
- Secrets are read from configuration/environment variables and should not be committed.
- Serilog logs login, Gmail sync, OpenAI calls, and errors.
- EF Core migrations run at API startup in Docker for fast MVP setup.
