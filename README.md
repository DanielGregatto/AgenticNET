# AgenticNET

**Production-ready multi-agent AI orchestration for .NET 8 and Azure.**

AgenticNET is an open-source framework that brings intelligent, observable, configuration-driven AI agents to any .NET application — with enterprise-grade Clean Architecture, Retrieval-Augmented Generation, full decision traceability, and first-class Azure AI integration out of the box.

> Built to demonstrate what a real AI backend looks like in C# — not a prototype, not a tutorial wrapper. A full production stack.

---

## Why AgenticNET?

Most AI demos in .NET are a single `kernel.InvokeAsync()` call. AgenticNET is what comes next:

- **Multi-agent routing** — a RouterAgent classifies intent and dispatches to the right specialist automatically.
- **Pluggable RAG** — add a new knowledge base by dropping files in blob storage and adding one line of config. No code, no Terraform changes.
- **Reviewer loop** — agents can be configured to self-critique answers and retry until a confidence threshold is met.
- **Zero-secret Azure auth** — `AzureCliCredential` locally, Managed Identity in production. No API keys stored anywhere.
- **Full decision audit trail** — every response includes a `trace` array showing the exact chain of decisions: which agent was selected, which functions were called, what the reviewer scored, and whether a retry was triggered. Every AI decision is observable.
- **Fully config-driven** — agents, models, plugins, temperature, and review behaviour all live in `appsettings.json`.

---

## Architecture

```
POST /api/v1/chat
  └─ ChatCommandHandler (MediatR)
       └─ SemanticKernelOrchestrator
            ├─ RouterAgent          ← classifies intent, returns agent name
            ├─ Specialist Agent     ← GeneralAdvisor | ProductCatalog | SupplierAdvisor | ...
            │    └─ Plugins         ← RAG:<CatalogKey> | ProductCatalog | (extensible)
            ├─ ReviewerAgent        ← optional confidence-gated retry loop
            └─ SaveConversationTurn ← persists history to SQL
```

```
src/
├── Domain.Core          # EntityBase, domain-event interfaces
├── Domain               # Entities, Result<T>/Error, strongly-typed config, contracts
├── Data                 # EF Core, UnitOfWork, domain-event wiring
├── Identity             # ASP.NET Core Identity, JWT, social login
├── AgentInfrastructure  # Semantic Kernel, Azure OpenAI, AI Search, RAG pipeline, plugins
├── Services             # CQRS handlers + FluentValidation
├── Util                 # Shared utilities (image, text, Base64, browser)
├── UI.API               # ASP.NET Core controllers, middleware, Swagger
├── IoC                  # Single composition root
└── Tests/               # xUnit + Moq + Bogus + FluentAssertions
```

**Patterns used:** Result pattern, CQRS with MediatR, Domain-Driven Design (entities, domain events, bounded contexts), Onion / Clean Architecture, Options pattern, Semantic Kernel plugin model.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 8, ASP.NET Core |
| AI Orchestration | Microsoft Semantic Kernel |
| LLMs | Azure OpenAI (GPT-4o-mini), Azure AI Foundry (DeepSeek-R1) |
| Vector Search | Azure AI Search (hybrid: keyword + vector) |
| Embeddings | Azure OpenAI (text-embedding-ada-002) |
| Auth | ASP.NET Core Identity, JWT, OAuth2 (Google, Facebook) |
| ORM | Entity Framework Core 8 |
| Database | Azure SQL / SQL Server |
| Storage | Azure Blob Storage |
| Infrastructure | Terraform, Azure Container Apps |
| CI/CD | GitHub Actions (OIDC, no stored secrets) |
| Observability | Azure Application Insights |

---

## Agents

Agents are defined entirely in configuration. No code changes needed to add a new one.

| Agent | Model | Plugin | Behaviour |
|---|---|---|---|
| `RouterAgent` | GPT-4o-mini | — | Classifies message → returns agent name |
| `GeneralAdvisor` | DeepSeek-R1 | — | General-purpose fallback |
| `ProductCatalog` | GPT-4o-mini | ProductCatalog | Queries product database via SK function |
| `SupplierAdvisor` | GPT-4o-mini | RAG:Suppliers | Searches knowledge base; reviewed at 0.85 confidence |
| `ReviewerAgent` | GPT-4o-mini | — | Scores answers, instructs improvement if below threshold |

### Adding an agent — zero code

```jsonc
// appsettings.json → AgentOrchestration:Agents
{
  "Name": "LegalAdvisor",
  "Description": "Answers regulatory and compliance questions.",
  "SystemPrompt": "You are a regulatory compliance expert...",
  "Provider": "AzureAI",
  "DeploymentOrModel": "chat",
  "Plugins": ["RAG:Regulations"],
  "Temperature": 0.1,
  "MaxTokens": 2000,
  "Review": {
    "Required": true,
    "AgentReviewerName": "ReviewerAgent",
    "ConfidenceScore": 0.90,
    "AttemptsToImprove": 2
  }
}
```

---

## RAG — Multi-Catalog Knowledge Base

Convention-based naming eliminates per-index configuration:

```
Agent plugin: "RAG:Suppliers"  →  AI Search index: rag-suppliers
Agent plugin: "RAG:FAQ"        →  AI Search index: rag-faq
Agent plugin: "RAG:Regulations"→  AI Search index: rag-regulations
```

`RAGSearch:Catalogs` in `appsettings.json` is the **single source of truth**. The CI/CD pipeline reads it and provisions the full AI Search pipeline (index + data source + skillset + indexer) for each catalog automatically.

### Add a new knowledge base in 3 steps

Say you want to add a **Regulations** catalog alongside the existing `Suppliers` and `FAQ` ones:

1. Add the new key to `appsettings.json` — `"Regulations"` is new, the others already exist:
   ```json
   "RAGSearch": { "Catalogs": ["Suppliers", "FAQ", "Regulations"] }
   ```
2. Add `"RAG:Regulations"` to the agent's `Plugins` list in the same file.
3. Upload `.pdf`, `.docx`, or `.txt` files to `documents/regulations/` in Azure Blob Storage.

Push → CI/CD detects the new catalog, creates the `rag-regulations` index, skillset, and indexer, and triggers the initial run. **No Terraform changes. No code changes.**

---

## API

All endpoints require `Authorization: Bearer <jwt>` except `/api/v1/auth/*`.

### Chat

```http
POST /api/v1/chat
Content-Type: application/json

{
  "message": "Which suppliers offer certified raw materials?",
  "conversationId": "optional-existing-guid",
  "canUseDefaultAgent": true
}
```

```jsonc
// Response
{
  "agentName": "SupplierAdvisor",
  "conversationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "content": "Based on the knowledge base, the following suppliers...",
  "timestamp": "2026-06-08T12:00:00Z",
  "trace": [
    { "type": "RouterDecision", "data": { "selectedAgent": "SupplierAdvisor" } },
    { "type": "FunctionCall",   "data": { "function": "SearchDocuments", "query": "certified raw materials" } },
    { "type": "ReviewerDecision", "data": { "confidence": 0.91, "isValid": true } }
  ]
}
```

| Method | Path | Description |
|---|---|---|
| `POST` | `/api/v1/chat` | Auto-route to best agent |
| `GET` | `/api/v1/agents` | List registered agents |
| `POST` | `/api/v1/agents/{name}/messages` | Send directly to a named agent |
| `POST` | `/api/v1/auth/login` | Obtain JWT |
| `POST` | `/api/v1/auth/refresh` | Refresh JWT |

---

## Getting Started

### Deploy to Azure — no local tooling required

For anyone who wants to spin up the full stack on Azure and test via API.

**Prerequisites:** Azure subscription · GitHub account · [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli) (`az login`) · [GitHub CLI](https://cli.github.com) (`gh auth login`)

```powershell
# 1. One-time setup per environment.
#    Creates the Azure service principal, OIDC federation, and all GitHub secrets automatically.
#    No passwords stored anywhere — OIDC only.
#    The script will ask for a short name suffix (max 6 chars, e.g. "abc123").
#    This suffix is appended to globally unique Azure resource names (ACR, SQL, OpenAI, Search).
#    Use the SAME suffix for both dev and prod — it is stored as a shared repo-level variable.
powershell -ExecutionPolicy Bypass -File .\platform\scripts\setup-azure.ps1 dev
powershell -ExecutionPolicy Bypass -File .\platform\scripts\setup-azure.ps1 prod

# 2. Push to the right branch — CI/CD does the rest
git push origin development   # → dev environment
git push origin master        # → production environment

# 3. Follow the GitHub Actions run in your repository.
#    When the pipeline finishes, the job summary will show the Container App URL — ready to call.
```

The pipeline provisions all Azure resources via Terraform, builds and pushes the Docker image, sets up AI Search indexes for every catalog in `RAGSearch:Catalogs`, and deploys the Container App. **Nothing to install, nothing to configure manually.**

> To tear everything down, set `destroy_environment = true` in `platform/cloud/tvars/terraform-dev.tfvars` and push. The pipeline destroys all resources and skips deployment.

> **Cost tip:** Azure AI Search carries a fixed monthly cost regardless of usage. To keep things near-free, I recommend running only the `dev` environment. The `prod` pipeline is available for when you are ready to go live.

**Resources provisioned automatically:**

Resource Group · Virtual Network · Subnets · Container App Environment · Container App · Container Registry · User-Assigned Managed Identity · Azure SQL · Azure OpenAI · Azure AI Search · Azure Blob Storage · Log Analytics Workspace · Application Insights · RBAC role assignments

---

### Run Locally — for developers

**Prerequisites:** Azure subscription · GitHub account · [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli) (`az login`) · [GitHub CLI](https://cli.github.com) (`gh auth login`) · .NET 8 SDK

> Complete steps 1, 2, and 3 from **Deploy to Azure** above first. All endpoints (SQL, OpenAI, AI Search) come from the provisioned `dev` environment — there is nothing to install locally beyond the SDK.

```powershell
# Fork the repo on GitHub, then clone your fork
git clone https://github.com/<your-username>/AgenticNET.git
cd AgenticNET

# Authenticate — must use terminal, not VS debugger (VS strips PATH and breaks Azure CLI auth)
az login

# Set user secrets
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=sql-dev-agenticnet-<suffix>.database.windows.net;Database=agenticnet;Authentication=Active Directory Default;Encrypt=True;MultipleActiveResultSets=True;" --project src/UI.API
dotnet user-secrets set "AgentOrchestration:Providers:AzureAI:Endpoint" "https://ai-dev-agenticnet-<suffix>.cognitiveservices.azure.com/" --project src/UI.API
dotnet user-secrets set "AgentOrchestration:Providers:AzureAIFoundry:Endpoint" "https://ai-dev-agenticnet-<suffix>.services.ai.azure.com/" --project src/UI.API
dotnet user-secrets set "Embedding:Endpoint" "https://ai-dev-agenticnet-<suffix>.cognitiveservices.azure.com/" --project src/UI.API
dotnet user-secrets set "Embedding:Deployment" "embeddings" --project src/UI.API
dotnet user-secrets set "RAGSearch:Endpoint" "https://<resource>.search.windows.net" --project src/UI.API

# Run — EF Core migrations apply automatically on startup
dotnet run --project src/UI.API/UI.API.csproj
```

> The project uses `ChainedTokenCredential(AzureCliCredential, ManagedIdentityCredential)`. Locally, `az login` is enough. In production, the Container App's Managed Identity is picked up automatically. No API keys stored anywhere.

---

## Build & Test

```powershell
# Build
dotnet build AgenticNET.sln

# Run all unit tests
dotnet test src/Tests/Unitary/Domain.Unit.Tests/Unit.Tests.csproj

# Run a single test class
dotnet test src/Tests/Unitary/Domain.Unit.Tests/Unit.Tests.csproj --filter "FullyQualifiedName~LoginCommandHandlerTests"

# Docker
docker build -t agenticnet -f src/UI.API/Dockerfile .
```

---

## License

MIT
