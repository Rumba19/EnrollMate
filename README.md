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
2. Agent receives the application via SQS trigger
3. Agent checks eligibility — year level, course availability, prerequisites, zone
4. Agent detects missing documents — identifies exactly what's needed per student
5. Agent sends a **personalised** email to the parent detailing what's missing and how to upload it
6. Parent uploads the missing document
7. Agent detects the upload, re-evaluates, confirms enrollment, and sends a welcome email with timetable
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
│              ASP.NET Core Minimal APIs                       │
│         Applications │ Agent status │ Notifications         │
└──────────────────────────────┬──────────────────────────────┘
                               │
┌──────────────────────────────▼──────────────────────────────┐
│                      Agent layer                             │
│                    Semantic Kernel                           │
│         Orchestrator → Tool selection → LLM reasoning       │
└────┬──────────────┬──────────────┬────────────┬─────────────┘
     │              │              │            │
  Data tools    Action tools   RAG tools   Escalation
  (read DB)    (write/send)  (policies)    (staff)
└─────────────────────────────────────────────────────────────┘
                               │
┌──────────────────────────────▼──────────────────────────────┐
│                   Infrastructure (AWS)                       │
│   EC2 │ SQS (triggers) │ SES (email) │ SNS (alerts) │ S3   │
└─────────────────────────────────────────────────────────────┘
```

---

## Agent tools

The agent is built as a collection of C# tool methods decorated with `[KernelFunction]`. Semantic Kernel decides which tools to call and in what order based on the application context.

### Data tools — read enrollment data
| Tool | Description |
|------|-------------|
| `CheckEligibility(studentId, courseId)` | Validates age, prerequisites, zone, and capacity |
| `GetCourseAvailability(courseId, semester)` | Returns available places and waitlist status |
| `GetRequiredDocuments(yearLevel)` | Returns checklist of required documents per year level |
| `GetUploadedDocuments(applicationId)` | Returns list of documents already submitted |
| `GetApplicationById(applicationId)` | Returns full application record |

### Action tools — take real-world actions
| Tool | Description |
|------|-------------|
| `SendEmailViaSES(to, subject, body)` | Sends personalised email via AWS SES |
| `UpdateApplicationStatus(id, status)` | Updates application state in DB |
| `EnrollStudent(studentId, courseId)` | Confirms enrollment and updates records |
| `CreateStaffNotification(caseId, summary)` | Flags case for staff review via SNS |
| `ScheduleFollowUp(applicationId, delayHours)` | Queues a follow-up action via SQS |

### Knowledge tools — RAG over policy documents
| Tool | Description |
|------|-------------|
| `SearchEnrollmentPolicy(query)` | Semantic search over school policy docs |
| `GetEligibilityRules(yearLevel, courseType)` | Returns structured rules for eligibility decisions |
| `GetFAQAnswer(question)` | Answers common parent questions from indexed FAQ |

---

## Tech stack

| Layer | Technology |
|-------|-----------|
| Runtime | .NET 10 (LTS) |
| API | ASP.NET Core Minimal APIs |
| Agent orchestration | Semantic Kernel |
| LLM | AWS Bedrock (Claude) |
| Email | AWS SES |
| Queue / triggers | AWS SQS |
| Alerts | AWS SNS |
| Document storage | AWS S3 |
| Vector search | Amazon OpenSearch (pgvector for local dev) |
| Data (mock) | In-memory / JSON — no DB required for showcase |
| Data (production) | SQL Server / EF Core 10 (pluggable via interface) |

---

## Project structure

```
EnrollMateAgent/
├── src/
│   ├── EnrollMate.Agent/               # Core agent logic
│   │   ├── AgentOrchestrator.cs        # Semantic Kernel setup & run loop
│   │   ├── Tools/
│   │   │   ├── DataTools.cs            # Read tools (eligibility, docs, courses)
│   │   │   ├── ActionTools.cs          # Write tools (email, status, enroll)
│   │   │   └── KnowledgeTools.cs       # RAG tools (policy, FAQ)
│   │   └── Prompts/
│   │       └── SystemPrompt.txt        # Agent system prompt
│   │
│   ├── EnrollMate.Api/                 # ASP.NET Core Minimal API
│   │   ├── Program.cs
│   │   ├── Endpoints/
│   │   │   ├── ApplicationsEndpoints.cs
│   │   │   ├── AgentEndpoints.cs
│   │   │   └── NotificationsEndpoints.cs
│   │   └── appsettings.json
│   │
│   ├── EnrollMate.Data/                # Data abstraction layer
│   │   ├── Interfaces/
│   │   │   ├── IApplicationRepository.cs
│   │   │   ├── ICourseRepository.cs
│   │   │   └── IDocumentRepository.cs
│   │   ├── Mock/                       # In-memory mock — used for showcase
│   │   │   ├── MockApplicationRepository.cs
│   │   │   ├── MockCourseRepository.cs
│   │   │   └── SeedData.cs
│   │   └── SqlServer/                  # Real DB — plugged in for production
│   │       └── (EF Core implementations — added later)
│   │
│   ├── EnrollMate.Messaging/           # AWS SQS / SES / SNS wrappers
│   │   ├── SesEmailSender.cs
│   │   ├── SqsQueueListener.cs
│   │   └── SnsAlertPublisher.cs
│   │
│   └── EnrollMate.Shared/             # Shared models & constants
│       ├── Models/
│       │   ├── Application.cs
│       │   ├── Student.cs
│       │   ├── Course.cs
│       │   └── AgentAction.cs
│       └── Enums/
│           └── ApplicationStatus.cs
│
├── tests/
│   ├── EnrollMate.Agent.Tests/         # Agent tool unit tests
│   └── EnrollMate.Api.Tests/           # API integration tests
│
├── docs/
│   ├── SPEC.md                         # This file — full technical spec
│   ├── DEMO_SCRIPT.md                  # Step-by-step demo walkthrough
│   └── PITCH.md                        # School pitch deck notes & ROI calc
│
├── .github/
│   └── workflows/
│       └── build.yml                   # CI — build + test on push
│
├── README.md
├── .gitignore
└── EnrollMateAgent.sln
```

---

## Data abstraction — mock to real in one swap

The data layer is fully abstracted behind interfaces. The showcase runs entirely on mock in-memory data. When a real school client comes on board, you implement the same interfaces against their database — the agent doesn't change at all.

```csharp
// Interface — lives in EnrollMate.Data
public interface IApplicationRepository
{
    Task<EnrollmentApplication?> GetByIdAsync(string applicationId);
    Task UpdateStatusAsync(string applicationId, ApplicationStatus status);
    Task<IEnumerable<EnrollmentApplication>> GetPendingAsync();
}

// Mock implementation — used in showcase
public class MockApplicationRepository : IApplicationRepository { ... }

// Real implementation — added when going to production
public class SqlApplicationRepository : IApplicationRepository { ... }
```

Registration in `Program.cs`:
```csharp
// Showcase mode
builder.Services.AddScoped<IApplicationRepository, MockApplicationRepository>();

// Production mode (toggle via config)
// builder.Services.AddScoped<IApplicationRepository, SqlApplicationRepository>();
```

---

## Agent flow — sequence

```
SQS message received
        │
        ▼
AgentOrchestrator.RunAsync(applicationId)
        │
        ▼
[LLM] Reads application context
        │
        ├── calls CheckEligibility()
        ├── calls GetRequiredDocuments()
        ├── calls GetUploadedDocuments()
        │
        ▼
[LLM] Reasons over results — decides path:
        │
        ├── All good → EnrollStudent() → SendEmailViaSES(welcome)
        ├── Missing docs → SendEmailViaSES(request) → ScheduleFollowUp()
        └── Edge case → CreateStaffNotification() with full summary
        │
        ▼
UpdateApplicationStatus() → done
```

---

## API endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/applications` | Submit a new enrollment application |
| `GET` | `/applications/{id}` | Get application status and history |
| `GET` | `/applications` | List all applications (staff) |
| `POST` | `/applications/{id}/documents` | Upload a document for an application |
| `GET` | `/agent/log` | Get live agent activity log |
| `POST` | `/agent/run/{id}` | Manually trigger agent for an application |
| `GET` | `/notifications` | Get staff notifications / escalations |

---

## Application states

```
Submitted → Processing → PendingDocuments → UnderReview → Confirmed
                                                       → Waitlisted
                                                       → Rejected
                    → EscalatedToStaff
```

---

## Getting started

### Prerequisites
- .NET 10 SDK
- AWS account with Bedrock, SES, SQS, SNS enabled (or use mock mode — no AWS needed for local dev)

### Run locally (mock mode — no AWS required)
```bash
git clone https://github.com/your-username/enrollmate-agent
cd enrollmate-agent
dotnet restore
dotnet run --project src/EnrollMate.Api
```

The API runs on `https://localhost:5001`. Mock data is seeded automatically.

### Run with AWS
Set the following in `appsettings.json` or environment variables:
```json
{
  "Aws": {
    "Region": "ap-southeast-2",
    "BedrockModelId": "anthropic.claude-3-5-sonnet-20241022-v2:0",
    "SesFromEmail": "noreply@yourschool.edu",
    "SqsQueueUrl": "https://sqs.region.amazonaws.com/account/enrollmate-queue",
    "SnsTopicArn": "arn:aws:sns:region:account:enrollmate-alerts"
  },
  "DataMode": "Mock"  // or "SqlServer"
}
```

### Run tests
```bash
dotnet test
```

---

## Roadmap

### v0.1 — Showcase (now)
- [x] Project structure and spec
- [ ] Mock data layer with seed data
- [ ] Core agent tools (data + action)
- [ ] Semantic Kernel orchestration
- [ ] Email generation via AWS Bedrock
- [ ] REST API endpoints
- [ ] Agent activity log endpoint

### v0.2 — Demo polish
- [ ] Demo script and seed scenario (Sofia Nguyen application)
- [ ] Real-time agent log streaming (SignalR or SSE)
- [ ] Staff dashboard UI (separate repo)
- [ ] Timetable conflict detection tool

### v0.3 — Production ready
- [ ] SQL Server / EF Core 10 data layer
- [ ] Multi-tenant school configuration
- [ ] RAG over school policy documents (OpenSearch)
- [ ] Auth — JWT with school staff and parent roles
- [ ] AWS deployment scripts (EC2 / ECS)

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

## Contributing

This is an independent project — not affiliated with any employer. Contributions, ideas, and feedback welcome via issues and pull requests.

---

## License

MIT — use freely, attribution appreciated.
