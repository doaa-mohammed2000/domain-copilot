# Domain Copilot — System Design

## 1. System Overview

Domain Copilot is an agentic Retrieval-Augmented Generation (RAG) platform
for the Government domain.

The system helps citizens describe their situation and receive grounded
information about relevant government services, eligibility conditions,
required documents, procedures, and regulations.

The system combines:

- Document ingestion and processing
- Hybrid information retrieval
- Large Language Model (LLM) providers
- Specialized AI agents
- An orchestrator for coordinating agents
- Human approval for sensitive or side-effecting actions
- Asynchronous background jobs
- Real-time progress updates
- Persistent history and audit logs
- Authentication and role-based authorization
- Observability and evaluation

The system follows Clean Architecture so that business logic remains
independent from external frameworks, databases, LLM providers, and
infrastructure technologies.

### High-Level Flow

```text
Citizen
   |
   v
API / Frontend
   |
   v
Orchestrator
   |
   +--------------------+
   |                    |
   v                    v
Specialized Agents    RAG Retrieval
   |                    |
   |                    v
   |              Vector + Keyword Search
   |                    |
   +---------+----------+
             |
             v
       Grounded Response
             |
             v
      Human Approval
             |
             v
        Final Result


## 3. Architecture Layers

The system follows Clean Architecture with four main layers.

### 3.1 Domain Layer

The Domain layer contains the core business concepts and rules of the
application.

It must not depend on:

- ASP.NET Core
- Entity Framework Core
- LLM SDKs
- Vector database SDKs
- Redis
- Qdrant
- Any external infrastructure technology

Responsibilities include:

- Domain entities
- Value objects
- Domain rules
- Domain errors
- Business invariants

Project:

`src/DomainCopilot.Domain`

---

### 3.2 Application Layer

The Application layer contains the application's use cases and business
orchestration logic.

It defines interfaces for external capabilities that the application
needs without depending on their concrete implementations.

Responsibilities include:

- Use cases
- Application services
- DTOs (Data Transfer Objects)
- Application interfaces
- Validation
- Workflow contracts
- Agent contracts
- Retrieval contracts
- Provider abstractions

Project:

`src/DomainCopilot.Application`

---

### 3.3 Infrastructure Layer

The Infrastructure layer implements the interfaces defined by the
Application layer.

It contains integrations with external technologies such as:

- PostgreSQL
- Entity Framework Core
- Qdrant
- Redis
- LLM providers
- Embedding providers
- File storage
- Background job infrastructure
- Observability infrastructure

Project:

`src/DomainCopilot.Infrastructure`

The Application and Domain layers must not reference these concrete
technologies.

---

### 3.4 API Layer

The API layer is the system entry point.

It is responsible for exposing HTTP endpoints and connecting external
requests to application use cases.

Responsibilities include:

- HTTP endpoints
- Authentication
- Authorization
- Request/response models
- Dependency Injection configuration
- Middleware
- OpenAPI documentation
- Real-time communication endpoints
- Application startup

Project:

`src/DomainCopilot.Api`

---

## 4. Dependency Direction

Dependencies must follow this direction:

```text
                    ┌─────────────────────┐
                    │      API Layer      │
                    │  ASP.NET Core       │
                    └──────────┬──────────┘
                               │
                               v
                    ┌─────────────────────┐
                    │ Application Layer  │
                    │ Use Cases / Ports   │
                    └──────────┬──────────┘
                               │
                               v
                    ┌─────────────────────┐
                    │    Domain Layer    │
                    │ Business Rules     │
                    └─────────────────────┘

                    Infrastructure
                         │
                         v
                    Application
                         │
                         v
                       Domain

## 5. Components & Technologies

The system is composed of several major components. Each component has a
specific responsibility and is isolated behind appropriate abstractions.

### 5.1 ASP.NET Core API

ASP.NET Core is used as the HTTP API layer.

Responsibilities:

- Expose REST API endpoints
- Authentication and authorization
- Request validation
- OpenAPI documentation
- Dependency Injection
- Real-time communication
- Health and readiness endpoints

The API layer does not contain core business rules.

---

### 5.2 PostgreSQL

PostgreSQL is used as the primary relational database.

It stores structured and transactional data such as:

- Users
- Roles
- Documents
- Document processing status
- Document metadata
- Jobs
- Workflow runs
- Agent execution records
- Approvals
- Conversation/history records
- Audit logs
- Evaluation results
- Token and cost usage

PostgreSQL is accessed through infrastructure adapters so that the
application layer does not depend directly on the database technology.

---

### 5.3 Qdrant

Qdrant is used as the vector database for semantic retrieval.

Document chunks are converted into embeddings and stored as vectors
together with metadata.

The metadata can include:

- Document identifier
- Source
- Section
- Page
- Clause
- Version
- Chunk identifier

Qdrant is accessed through an infrastructure adapter.

The application layer interacts with retrieval interfaces rather than
directly depending on the Qdrant SDK.

---

### 5.4 Keyword Search

The retrieval system will combine semantic vector search with keyword
search.

Vector search helps identify semantically similar content, while keyword
search helps preserve exact matches for important terms such as:

- Service names
- Regulation numbers
- Article numbers
- Required documents
- Government terminology

The final retrieval strategy will use a documented fusion method to
combine the results.

---

### 5.5 Redis

Redis is used as the messaging and job-queue infrastructure for
long-running asynchronous operations.

It supports the T7 requirement by allowing work to be submitted to a
durable queue and processed by background workers.

Examples include:

- Document ingestion
- Embedding generation
- Large retrieval workflows
- Agentic workflows
- Other long-running operations

The API should return a job identifier without waiting for the complete
operation.

---

### 5.6 Background Workers

Background workers process queued jobs independently from HTTP requests.

The worker system will:

- Consume queued jobs
- Update job status
- Report progress
- Handle retries
- Support cancellation
- Support resumable processing
- Preserve job state across API restarts
- Ensure idempotent processing

The implementation will use .NET background processing capabilities,
while queue-specific details remain inside Infrastructure.

---

### 5.7 LLM Provider Abstraction

The system will define a provider abstraction for Large Language Model
(LLM) capabilities.

The abstraction will support:

- Text completion
- Streaming
- Tool calling
- Embeddings

At least two working implementations will be provided:

1. A hosted LLM provider
2. An alternative or local provider

The selected provider will be controlled through configuration.

A fallback chain will allow the system to use an alternative provider
when the primary provider is unavailable.

The core application logic must not depend directly on a specific LLM
SDK.

---

### 5.8 Angular Frontend

Angular will provide the minimal user interface required to demonstrate
the platform.

The frontend will support the main assessment workflows, including:

- Document ingestion
- Asking questions
- Viewing grounded answers
- Viewing citations
- Monitoring asynchronous job progress
- Running agentic workflows
- Human approval
- Viewing workflow history
- Inspecting relevant traces or execution information

The frontend communicates with the backend through the HTTP API and
real-time communication endpoints.

---

### 5.9 Docker

Docker will be used to provide a reproducible development environment.

The target environment will include:

- ASP.NET Core API
- Background worker
- PostgreSQL
- Qdrant
- Redis
- Frontend

A Docker Compose configuration will provide a documented command for
starting the complete system.

The project will also provide seed/sample data or an ingestion step so
that a fresh environment can be tested without manually configuring each
component.

## 6. RAG Pipeline Architecture

Retrieval-Augmented Generation (RAG) is the core knowledge mechanism of
Domain Copilot.

The system does not rely on the LLM's internal knowledge to answer
government-related questions. Instead, it retrieves relevant evidence
from the approved document corpus and provides that evidence to the LLM
as context.

The RAG pipeline consists of two major flows:

1. Document ingestion
2. Query retrieval and answer generation

---

### 6.1 Document Ingestion Flow

The document ingestion pipeline transforms source documents into
searchable knowledge.

The high-level flow is:

```text
Source Document
      |
      v
Document Upload
      |
      v
Format Detection
      |
      v
Text Extraction
      |
      v
Cleaning & Normalization
      |
      v
Chunking
      |
      v
Metadata Enrichment
      |
      v
Embedding Generation
      |
      v
Vector Indexing
      |
      +--------------------+
      |                    |
      v                    v
 PostgreSQL             Qdrant
 Document Metadata      Vector Chunks

 ## 7. Multi-Agent Architecture

Domain Copilot uses a multi-agent architecture to separate specialized
responsibilities instead of delegating the entire workflow to a single
LLM call.

The system contains an Orchestrator and at least three specialized
agents.

### 7.1 Agent Roles

The initial specialized agents are:

1. **Service Identification Agent**
   - Identifies the government service or regulation relevant to the
     citizen's situation.
   - Uses retrieval tools to inspect the approved knowledge corpus.
   - Produces a typed identification result.

2. **Eligibility Agent**
   - Determines the eligibility conditions supported by the retrieved
     evidence.
   - Does not make unsupported eligibility assumptions.
   - Produces a structured eligibility result with evidence references.

3. **Documents & Procedures Agent**
   - Identifies required documents and procedural steps.
   - Uses retrieval tools to locate supporting regulations and official
     instructions.
   - Produces structured document and procedure information with
     citations.

Additional specialized agents may be introduced if required by the final
workflow, but the system must maintain clear responsibilities for each
agent.

---

### 7.2 Orchestrator

The Orchestrator coordinates the specialized agents and controls the
overall workflow.

A simplified workflow is:

```text
Citizen Request
      |
      v
Orchestrator
      |
      v
Service Identification Agent
      |
      v
Eligibility Agent
      |
      v
Documents & Procedures Agent
      |
      v
Evidence Validation
      |
      v
Draft Response
      |
      v
Human Approval Gate
      |
      v
Final Response


## 8. Asynchronous Job Architecture

The system implements asynchronous processing for operations that may take
a significant amount of time.

This architecture is required to satisfy the T7 Async Long-Running Jobs
variant.

### 8.1 Job Submission Flow

The client does not wait for a long-running operation to finish.

Instead:

```text
Client
  |
  | POST request
  v
API
  |
  | Create Job
  v
PostgreSQL
  |
  | Enqueue Job
  v
Redis Queue
  |
  | Immediate response
  v
Client
  |
  | Job ID
  v
Status / Progress Updates

## 9. Human Approval Architecture

Human approval is a mandatory control for sensitive or side-effecting
operations.

The system must never allow an LLM or agent to independently execute a
sensitive side effect.

### 9.1 Approval Flow

The approval workflow is:

```text
Agent
  |
  | Proposes Action
  v
Approval Request
  |
  v
Human Reviewer
  |
  +------------+-------------+
  |            |             |
  v            v             v
Approve      Reject     Edit & Approve
  |            |             |
  +------------+-------------+
               |
               v
        Continue Workflow

 ## 11. Authentication & Authorization

The system uses authentication to verify the identity of users and
authorization to control what each authenticated user is allowed to do.

Authentication and authorization are enforced on the server side.

The frontend must not be considered a security boundary.

### 11.1 User Roles

The system defines three roles:

1. **Citizen**
2. **Reviewer**
3. **Administrator**

Each role has different permissions.

---

### 11.2 Citizen

Citizens can:

- Submit questions
- View grounded answers
- View citations
- Submit or monitor permitted jobs
- View their own conversation history
- Request cancellation of their own running jobs

Citizens cannot:

- Approve sensitive actions
- Access other users' history
- Manage users
- Modify system configuration
- Access administrative audit data

---

### 11.3 Reviewer

Reviewers can:

- Perform citizen-level operations
- View approval requests assigned to them
- Approve requests
- Reject requests
- Edit and approve requests
- View relevant workflow evidence
- Inspect workflow execution information required for review

Reviewers cannot:

- Manage system administrators
- Modify security configuration
- Change application-wide provider configuration
- Access unrelated users' private information without authorization

---

### 11.4 Administrator

Administrators can:

- Manage users and roles
- Manage system configuration
- Manage approved knowledge sources
- View administrative audit information
- Inspect system health
- Manage operational settings

Administrative permissions must be enforced through server-side
authorization policies.

---

### 11.5 Authorization Policies

Authorization is implemented using explicit policies rather than relying
only on frontend visibility.

Example policies include:

```text
CanAskQuestions
CanViewOwnHistory
CanReviewApproval
CanManageUsers
CanManageKnowledgeSources
CanViewAuditLogs
CanManageConfiguration

## 12. Observability & Monitoring

Observability allows the system to understand, trace, and diagnose
requests, jobs, workflows, agents, tools, and external provider calls.

The observability design covers:

- Logging
- Distributed tracing
- Correlation identifiers
- Workflow identifiers
- Metrics
- Token and cost tracking
- Health checks
- Readiness checks
- Error tracking

---

### 12.1 Correlation ID

Every incoming request receives a Correlation ID.

The Correlation ID is propagated through the application layers and
external operations.

Example:

```text
HTTP Request
     |
     | Correlation ID
     v
Application Use Case
     |
     v
Orchestrator
     |
     +---- Agent
     |       |
     |       +---- Retrieval
     |
     +---- Tool
     |
     +---- LLM Provider

 ## 13. Security Architecture

Security is treated as a cross-cutting concern across the entire system.

The design addresses common web application risks, LLM-specific risks,
authorization failures, sensitive data exposure, and unsafe tool
execution.

---

### 13.1 Authentication and Authorization

Authentication verifies user identity before protected operations are
performed.

Authorization is enforced on the server using role and policy checks.

The frontend must never be treated as the source of truth for
permissions.

---

### 13.2 Prompt Injection Protection

The system treats retrieved documents and user-provided content as
untrusted input.

Prompt injection defenses include:

- Separating system instructions from user content.
- Separating trusted application instructions from retrieved documents.
- Restricting tools available to each agent.
- Validating tool arguments before execution.
- Preventing retrieved content from changing system-level instructions.
- Requiring human approval for protected side effects.
- Testing adversarial prompt-injection cases through the evaluation
  harness.

At least three prompt-injection scenarios will be included in the
security/evaluation test set.

---

### 13.3 Tool Allow-Lists

Agents cannot invoke arbitrary application capabilities.

Each agent receives an explicit allow-list of tools.

For example:

```text id="x9r5f2"
Service Identification Agent
    ├── Knowledge Retrieval
    ├── Document Metadata
    └── Citation

Eligibility Agent
    ├── Knowledge Retrieval
    ├── Document Metadata
    └── Citation

Documents & Procedures Agent
    ├── Knowledge Retrieval
    ├── Document Metadata
    └── Citation

## 14. Provider Abstraction

The system isolates external AI providers behind application-level
interfaces.

The Domain and Application layers must not depend directly on a specific
LLM SDK, embedding SDK, or provider-specific implementation.

### 14.1 LLM Provider Interface

The application defines a common provider contract supporting:

- Text completion
- Streaming
- Tool calling
- Embeddings

A simplified conceptual interface is:

```text
ILLMProvider
    ├── CompleteAsync(...)
    ├── StreamAsync(...)
    ├── ExecuteToolCallAsync(...)
    └── CreateEmbeddingAsync(...)

## 15. Testing Strategy

Testing is part of the architecture and is applied at multiple levels.

The test strategy separates business logic testing from external
infrastructure testing.

### 15.1 Unit Tests

Unit tests verify Domain and Application behavior in isolation.

External dependencies such as:

- LLM providers
- Vector stores
- Databases
- Redis
- File systems

are replaced with test doubles, mocks, or stubs where appropriate.

Unit tests cover areas such as:

- Domain rules
- Application use cases
- Validation
- Authorization decisions
- Agent orchestration rules
- Retry policies
- Job state transitions
- Approval state transitions
- Idempotency behavior
- Evidence sufficiency and refusal decisions

Unit tests must not require a real LLM provider.

---

### 15.2 Integration Tests

Integration tests verify interactions with real infrastructure
components.

They cover areas such as:

- Document ingestion
- PostgreSQL persistence
- Vector indexing
- Hybrid retrieval
- Job queue processing
- Background workers
- Real-time progress
- API endpoints
- Authentication and authorization

Where practical, integration tests run against containerized
dependencies.

---

### 15.3 Contract Tests

Contract tests verify that agents and tools communicate using the
expected schemas.

They validate:

- Required fields
- Data types
- Valid enum values
- Tool input schemas
- Tool output schemas
- Agent input/output contracts
- Error response structures

This prevents incompatible changes between the orchestrator, agents, and
tools.

---

### 15.4 RAG Evaluation Harness

The system includes a runnable evaluation harness for retrieval and
grounded answer quality.

The evaluation dataset contains at least:

- 25 golden questions and expected answers
- 5 or more adversarial cases

Adversarial cases include:

- Out-of-corpus questions
- Ambiguous questions
- Prompt-injection attempts
- Conflicting sources

The evaluation harness measures at least:

- Retrieval hit rate
- Groundedness
- Refusal correctness

The harness produces actual baseline measurements and an interpretation of
the results.

Evaluation data and results are versioned as project artifacts.

---

### 15.5 Security Tests

Security tests cover:

- Prompt injection
- Unauthorized access
- Resource ownership violations
- Approval bypass attempts
- Tool allow-list violations
- Invalid tool arguments
- Excessive input sizes
- Invalid LLM outputs

Security tests are included in the automated test workflow where
practical.

---

### 15.6 Async Job Tests

T7-specific tests verify:

- Immediate job submission
- Queue processing
- Progress updates
- Cancellation
- Retry behavior
- Idempotent processing
- Worker restart recovery
- Resumability
- Duplicate execution protection

---

### 15.7 Test Environment

Tests must be reproducible from a clean development environment.

External dependencies required by integration tests are provided through
the project's containerized environment where practical.

The test suite should clearly distinguish between:

```text
Unit Tests
Integration Tests
Contract Tests
Evaluation Tests
Security Tests

## 15. Testing Strategy

Testing is part of the architecture and is applied at multiple levels.

The test strategy separates business logic testing from external
infrastructure testing.

### 15.1 Unit Tests

Unit tests verify Domain and Application behavior in isolation.

External dependencies such as:

- LLM providers
- Vector stores
- Databases
- Redis
- File systems

are replaced with test doubles, mocks, or stubs where appropriate.

Unit tests cover areas such as:

- Domain rules
- Application use cases
- Validation
- Authorization decisions
- Agent orchestration rules
- Retry policies
- Job state transitions
- Approval state transitions
- Idempotency behavior
- Evidence sufficiency and refusal decisions

Unit tests must not require a real LLM provider.

---

### 15.2 Integration Tests

Integration tests verify interactions with real infrastructure
components.

They cover areas such as:

- Document ingestion
- PostgreSQL persistence
- Vector indexing
- Hybrid retrieval
- Job queue processing
- Background workers
- Real-time progress
- API endpoints
- Authentication and authorization

Where practical, integration tests run against containerized
dependencies.

---

### 15.3 Contract Tests

Contract tests verify that agents and tools communicate using the
expected schemas.

They validate:

- Required fields
- Data types
- Valid enum values
- Tool input schemas
- Tool output schemas
- Agent input/output contracts
- Error response structures

This prevents incompatible changes between the orchestrator, agents, and
tools.

---

### 15.4 RAG Evaluation Harness

The system includes a runnable evaluation harness for retrieval and
grounded answer quality.

The evaluation dataset contains at least:

- 25 golden questions and expected answers
- 5 or more adversarial cases

Adversarial cases include:

- Out-of-corpus questions
- Ambiguous questions
- Prompt-injection attempts
- Conflicting sources

The evaluation harness measures at least:

- Retrieval hit rate
- Groundedness
- Refusal correctness

The harness produces actual baseline measurements and an interpretation of
the results.

Evaluation data and results are versioned as project artifacts.

---

### 15.5 Security Tests

Security tests cover:

- Prompt injection
- Unauthorized access
- Resource ownership violations
- Approval bypass attempts
- Tool allow-list violations
- Invalid tool arguments
- Excessive input sizes
- Invalid LLM outputs

Security tests are included in the automated test workflow where
practical.

---

### 15.6 Async Job Tests

T7-specific tests verify:

- Immediate job submission
- Queue processing
- Progress updates
- Cancellation
- Retry behavior
- Idempotent processing
- Worker restart recovery
- Resumability
- Duplicate execution protection

---

### 15.7 Test Environment

Tests must be reproducible from a clean development environment.

External dependencies required by integration tests are provided through
the project's containerized environment where practical.

The test suite should clearly distinguish between:

```text
Unit Tests
Integration Tests
Contract Tests
Evaluation Tests
Security Tests