# EnrollMate Agent
> AI-powered enrollment administration automation — built on .NET 10, Semantic Kernel, and AWS.

---

## What is this?

EnrollMate Agent is an autonomous AI agent that handles student enrollment application processing end to end. It replaces manual, repetitive admin work — eligibility checks, document verification, parent communications, follow-ups, and staff escalations — with an intelligent agent that works around the clock.

Built as a **showcase and future SaaS product** targeting schools and education providers.

---

## The problem it solves

During enrollment periods, school admin staff manually process hundreds of applications — checking eligibility rules, chasing missing documents, sending status emails, and following up with unresponsive parents. A single application can take 15–20 minutes to process. For a school receiving 400 applications per intake, that's **100+ hours of staff time** on work that follows predictable, automatable rules.

EnrollMate Agent handles the routine 80% autonomously — leaving staff to focus only on the edge cases that genuinely need human judgment.

---

## Demo scenario

1. Parent submits enrollment application for a student via the portal
2. Agent receives the application and begins processing
3. Agent checks eligibility — year level, zone, school capacity
4. Agent detects missing documents — identifies exactly what's needed per student
5. Agent sends a **personalised** email to the parent detailing what's missing and how to upload it
6. Parent uploads the missing document
7. Agent re-evaluates, confirms enrollment, and sends a welcome email
8. Staff open the dashboard in the morning and see: 47 processed overnight, 3 flagged for their review

**Zero staff involvement for routine cases.**

---

## Architecture overview

```
┌─────────────────────────────────────────────────────────────┐
│                        Client layer                         │
│              Staff dashboard  │  Parent portal              │
└──────────────────┬────────────────────────┬─────────────────┘
                   │                        │
┌──────────────────▼────────────────────────▼─────────────────┐
│                     API layer (.NET 10)                      │
│              ASP.NET Core Minimal APIs + Scalar UI           │
│         Applications │ Agent status │ Notifications         │
└──────────────────────────────┬──────────────────────────────┘
                               │
┌──────────────────────────────▼──────────────────────────────┐
│                      Agent layer                             │
│                    Semantic Kernel                           │
│         Orchestrator → Tool selection → LLM reasoning       │
└────┬──────────────┬────────────────────────────────────────-─┘
     │              │
  Data tools    Action tools
  (read only)   (write / side effects)
└─────────────────────────────────────────────────────────────┘
                               │
┌──────────────────────────────▼──────────────────────────────┐
│                   Infrastructure (AWS)                       │
│         Bedrock (LLM) │ SES (email) │ SQS │ SNS │ S3       │
└─────────────────────────────────────────────────────────────┘
```

---

## Agent tools

The agent is built as a collection of C# methods decorated with `[KernelFunction]`. Semantic Kernel decides which tools to call and in what order based on the application context.

### Data tools — read-only

| Tool | Description |
|------|-------------|
| `GetApplicationById(applicationId)` | Returns full application record including student, parent, documents, and agent log |
| `CheckEligibility(applicationId, schoolId)` | Validates year level and zone — returns eligible or specific reasons for rejection |
| `GetRequiredDocuments(yearLevel)` | Returns checklist of required documents for a given year level |
| `GetUploadedDocuments(applicationId)` | Returns uploaded vs missing documents for an application |
| `GetSchoolAvailability(schoolId)` | Returns capacity, enrolled count, available places, and waitlist count for a school |

### Action tools — write / side effects

| Tool | Description |
|------|-------------|
| `EnrollStudent(applicationId, schoolId)` | Confirms enrollment, increments enrolled count, logs action |
| `PlaceOnWaitlist(applicationId, schoolId)` | Adds student to waitlist, increments waitlist count, logs action |
| `UpdateApplicationStatus(applicationId, status)` | Updates application state and sets resolved timestamp for terminal states |
| `SendEmailViaSES(applicationId, to, subject, body)` | Sends personalised email via AWS SES (mock in showcase), logs action |
| `ScheduleFollowUp(applicationId, delayHours)` | Queues a follow-up, increments follow-up counter, logs action |
| `CreateStaffNotification(applicationId, summary)` | Escalates the case to staff with a concise summary, logs action |

---

## Agent decision flow

```
AgentOrchestrator.RunAsync(applicationId)
        │
        ▼
[LLM] GetApplicationById → reads full context
        │
        ├── For each requested school:
        │     CheckEligibility + GetSchoolAvailability
        │
        ├── GetUploadedDocuments → check document completeness
        │
        ▼
[LLM] Reasons over results — takes one of these paths:

  ALL GOOD (eligible + docs complete + places available)
  → EnrollStudent → UpdateApplicationStatus(Confirmed)
  → SendEmailViaSES (warm welcome email to parent)

  SCHOOL FULL (eligible + docs complete + school full)
  → PlaceOnWaitlist → enroll at any schools with places
  → SendEmailViaSES (confirmed + waitlisted breakdown)

  MISSING DOCUMENTS
  → UpdateApplicationStatus(PendingDocuments)
  → SendEmailViaSES (list exactly what's missing)
  → ScheduleFollowUp (48h delay)
  → If follow-ups exhausted → CreateStaffNotification

  NOT ELIGIBLE
  → UpdateApplicationStatus(Rejected)
  → SendEmailViaSES (respectful explanation)

  SPECIAL REQUIREMENTS or EDGE CASE
  → CreateStaffNotification (full summary)
  → UpdateApplicationStatus(EscalatedToStaff)
```

---

## Tech stack

| Layer | Technology |
|-------|-----------|
| Runtime | .NET 10 |
| API | ASP.NET Core Minimal APIs |
| API Explorer | Scalar UI |
| Agent orchestration | Semantic Kernel |
| LLM | AWS Bedrock (Claude) — mock mode available without AWS |
| Email | AWS SES (mock in showcase) |
| Queue / triggers | AWS SQS |
| Alerts | AWS SNS |
| Document storage | AWS S3 |
| Data (showcase) | In-memory mock — no DB or AWS required |
| Data (production) | SQL Server / EF Core 10 (pluggable via interface) |

---

## Project structure

```
EnrollMateAgent/
├── src/
│   ├── EnrollMate.Agent/               # Core agent logic
│   │   ├── AgentOrchestrator.cs        # Semantic Kernel setup and run loop
│   │   ├── Tools/
│   │   │   ├── DataTools.cs            # Read-only KernelFunctions
│   │   │   └── ActionTools.cs          # Write KernelFunctions with side effects
│   │   └── Prompts/
│   │       └── SystemPrompt.txt        # Agent system prompt and decision rules
│   │
│   ├── EnrollMate.Api/                 # ASP.NET Core Minimal API host
│   │   ├── Program.cs                  # DI registration, Semantic Kernel setup
│   │   └── Endpoints/
│   │       ├── ApplicationsEndpoints.cs  # Submit, list, get, upload documents
│   │       ├── AgentEndpoints.cs         # Run agent, get activity log
│   │       └── NotificationsEndpoints.cs # Staff escalation notifications
│   │
│   ├── EnrollMate.Data/                # Data abstraction layer
│   │   ├── Interfaces/
│   │   │   ├── IApplicationRepository.cs
│   │   │   ├── ISchoolRepository.cs
│   │   │   └── IDocumentRepository.cs
│   │   └── Mock/                       # In-memory mock — no DB required
│   │       ├── MockApplicationRepository.cs
│   │       ├── MockSchoolRepository.cs
│   │       ├── MockDocumentRepository.cs
│   │       └── SeedData.cs
│   │
│   └── EnrollMate.Shared/             # Models and enums shared across projects
│       ├── Models/
│       │   ├── Application.cs
│       │   ├── Student.cs
│       │   ├── School.cs
│       │   ├── Document.cs
│       │   └── AgentAction.cs
│       └── Enums/
│           └── ApplicationStatus.cs
│
├── tests/                              # Coming in v0.2
│
├── .claude/
│   └── commands/                       # Claude Code developer commands
│       ├── add-agent-tool.md           # /add-agent-tool
│       └── add-endpoint.md             # /add-endpoint
│
├── README.md
├── .gitignore
└── EnrollMateAgent.slnx
```

---

## API endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/applications` | Submit a new enrollment application |
| `GET` | `/applications` | List all applications (staff view) |
| `GET` | `/applications/{id}` | Get full application detail including agent log |
| `POST` | `/applications/{id}/documents` | Upload a document for an application |
| `POST` | `/agent/run/{id}` | Trigger the agent to process an application |
| `GET` | `/agent/log` | Get activity log across all applications |
| `GET` | `/notifications` | Get staff escalation notifications |

Scalar UI is available at `/scalar/v1` in development.

---

## Application states

```
Submitted → Processing → Confirmed
                       → Waitlisted
                       → PendingDocuments → (follow-up) → Confirmed / Abandoned
                       → Rejected
                       → EscalatedToStaff
```

---

## Data abstraction — mock to real in one swap

The data layer is fully abstracted behind interfaces. The showcase runs on in-memory mock data — no database or AWS account required. When moving to production, implement the same interfaces against a real database.

```csharp
// Showcase mode (current)
builder.Services.AddSingleton<IApplicationRepository, MockApplicationRepository>();

// Production mode (swap this in)
// builder.Services.AddScoped<IApplicationRepository, SqlApplicationRepository>();
```

---

## Getting started

### Prerequisites
- .NET 10 SDK

### Run locally (no AWS required)
```bash
git clone https://github.com/your-username/enrollmate-agent
cd enrollmate-agent
dotnet restore
dotnet run --project src/EnrollMate.Api
```

The API starts with mock seed data. Open the Scalar UI at `https://localhost:{port}/scalar/v1` to explore and call endpoints interactively.

To trigger the agent:
```
POST /agent/run/{applicationId}
```

The agent will return a 503 until an LLM connector is configured (see below).

### Connect to AWS Bedrock

Add the following to `appsettings.json` or environment variables, then uncomment the Bedrock connector in `Program.cs`:

```json
{
  "Aws": {
    "Region": "ap-southeast-2",
    "BedrockModelId": "anthropic.claude-3-5-sonnet-20241022-v2:0",
    "SesFromEmail": "noreply@yourschool.edu"
  }
}
```

---

## Claude Code developer commands

This project includes custom Claude Code commands for common development tasks. In Claude Code, type the command with a plain-English description of what you want — Claude reads the codebase context automatically.

### `/add-agent-tool <description>`

Adds a new `[KernelFunction]` to `DataTools` or `ActionTools`, and updates `SystemPrompt.txt` so the LLM knows the tool exists.

```
/add-agent-tool a tool that checks if a student has previously been rejected at a school
/add-agent-tool a tool that returns the waitlist position for a student at a given school
```

### `/add-endpoint <description>`

Adds a new minimal API endpoint following the existing pattern — correct file, route style, status codes, and DI usage.

```
/add-endpoint GET /applications/{id}/agent-log that returns only the agent action history for one application
/add-endpoint POST /applications/{id}/reprocess that re-triggers the agent for a resolved application
```

---

## Roadmap

### v0.1 — Showcase (current)
- [x] Project structure
- [x] Mock data layer with seed data
- [x] Core agent tools — data tools and action tools
- [x] Semantic Kernel orchestration
- [x] REST API endpoints
- [x] Agent activity log endpoint
- [x] Scalar UI for API exploration
- [x] Claude Code developer commands
- [ ] AWS Bedrock LLM connector wired up

### v0.2 — Demo polish
- [ ] Demo script and seed scenario
- [ ] Real-time agent log streaming (SignalR or SSE)
- [ ] Staff dashboard UI (separate repo)
- [ ] Unit and integration tests
- [ ] RAG tools over school policy documents

### v0.3 — Production ready
- [ ] SQL Server / EF Core 10 data layer
- [ ] Auth — JWT with school staff and parent roles
- [ ] AWS SES email integration
- [ ] AWS SQS trigger listener
- [ ] Multi-tenant school configuration

### v1.0 — SaaS
- [ ] School onboarding flow
- [ ] Per-school rule configuration
- [ ] Usage analytics and reporting
- [ ] Subscription billing integration

---

## ROI for schools

| Metric | Manual | With EnrollMate |
|--------|--------|----------------|
| Time per application | 15–20 min | ~0 min (routine cases) |
| 400 applications/intake | 100+ hrs staff time | ~20 hrs (edge cases only) |
| Follow-up emails | Manual, inconsistent | Automatic, personalised |
| After-hours processing | None | 24/7 |
| Staff focus | Routine admin | Complex cases only |

---

## License

MIT — use freely, attribution appreciated.
