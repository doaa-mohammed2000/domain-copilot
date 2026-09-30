# Business Requirements Document (BRD)

## 1. Project Overview

### 1.1 Project Name

Domain Copilot

### 1.2 Project Description

Domain Copilot is an agentic Retrieval-Augmented Generation (RAG) platform
designed to assist citizens with government services and regulations.

The system allows a citizen to describe their situation and receive a
grounded response based on an approved government knowledge base.

The response may include:
- The relevant government service.
- Eligibility requirements.
- Required documents.
- Procedures and next steps.
- Citations to the source documents.

The system uses multiple specialized AI agents coordinated by an orchestrator.
For actions that require human approval, the system must pause and wait for
an authorized human reviewer before continuing.

The platform also supports asynchronous long-running jobs for operations such
as document ingestion and processing.

## 2. Business Goals & Objectives

### 2.1 Business Goals

The primary goal of Domain Copilot is to provide citizens with reliable,
traceable, and understandable assistance when dealing with government
services and regulations.

The system aims to:

1. Help citizens identify the appropriate government service for their
   situation.

2. Provide eligibility requirements based on the available government
   knowledge base.

3. Identify required documents and explain the relevant procedures.

4. Generate responses grounded in retrieved source documents.

5. Provide exact citations so users and reviewers can trace information
   back to its source.

6. Support human review and approval for responses or actions that require
   authorization.

7. Process long-running operations asynchronously without blocking the
   user's request.

8. Provide observable and auditable execution through logs, traces,
   correlation IDs, and workflow history.

### 2.2 Objectives

The project will deliver a working prototype that demonstrates:

- Retrieval-Augmented Generation (RAG).
- Hybrid document retrieval.
- Multi-agent collaboration.
- Orchestrated agent workflows.
- Human-in-the-loop approval.
- Asynchronous job processing.
- Real-time job progress.
- Authentication and role-based authorization.
- Evaluation of retrieval and answer quality.
- Security controls against prompt injection and unsafe tool usage.
- Persistent execution and approval history.

## 3. Users & Roles

### 3.1 Citizen

The Citizen is the primary user of the platform.

The Citizen can:

- Describe a government-related situation or request.
- Ask questions about government services and regulations.
- Receive grounded answers based on the approved knowledge base.
- View citations and source information.
- Submit long-running requests when applicable.
- Track the progress of submitted jobs.
- View their own request and workflow history.

The Citizen cannot:

- Approve or reject AI-generated responses.
- Modify the government knowledge base.
- Execute administrative or destructive tools.
- Access another user's private history.

### 3.2 Reviewer

The Reviewer is an authorized human responsible for reviewing
AI-generated outputs that require human approval.

The Reviewer can:

- Review generated responses before finalization.
- View the retrieved evidence and citations.
- Approve a response.
- Reject a response.
- Edit a response and then approve it.
- View workflow execution details related to the approval request.
- Access the audit history required for reviewing decisions.

The Reviewer cannot:

- Modify system configuration directly.
- Bypass the approval workflow without an audited action.

### 3.3 Administrator

The Administrator manages the platform and its operational resources.

The Administrator can:

- Manage users and roles.
- Manage document ingestion operations.
- View document processing status and failures.
- Monitor system health and operational information.
- Access system-level workflow and audit information.
- Manage platform configuration through authorized configuration mechanisms.

The Administrator cannot:

- Bypass security controls or approval requirements without an
  explicitly authorized and audited operation.
  ## 4. Main User Workflows & Use Cases

### 4.1 Citizen Service Assistance Workflow

The main workflow starts when a citizen describes a government-related
situation or request.

The system should:

1. Receive the citizen's request.
2. Identify the relevant government service or regulation.
3. Retrieve relevant information from the approved knowledge base.
4. Determine the applicable eligibility requirements.
5. Identify required documents and procedures.
6. Generate a grounded draft response.
7. Provide citations for the information used.
8. Refuse to provide an unsupported answer when sufficient evidence
   cannot be found.

For workflows requiring human approval:

9. Submit the generated response to the approval workflow.
10. Pause execution until an authorized reviewer makes a decision.
11. Allow the reviewer to approve, reject, or edit and approve the response.
12. Continue the workflow according to the review decision.
13. Record the approval decision in the audit history.

### 4.2 Document Ingestion Workflow

Authorized users can submit government documents to the platform.

The ingestion workflow should:

1. Accept supported document formats.
2. Extract the document content.
3. Clean and normalize the extracted content.
4. Split the content into meaningful chunks.
5. Generate embeddings for the chunks.
6. Store the chunks and metadata.
7. Index the content for retrieval.
8. Track the processing status of each document.
9. Record failures with sufficient information for diagnosis.
10. Avoid creating duplicate data when the same document is ingested
    repeatedly.

Each indexed chunk should preserve relevant metadata such as:

- Source document.
- Section.
- Page.
- Clause or article when available.
- Document version.

### 4.3 Asynchronous Long-Running Job Workflow

Long-running operations must not block the initial HTTP request.

The system should:

1. Accept a long-running operation request.
2. Create a unique job identifier.
3. Persist the job state.
4. Place the job in a durable queue.
5. Return the job identifier immediately to the client.
6. Process the job using a background worker.
7. Persist progress updates.
8. Push progress updates to connected clients.
9. Allow the client to request cancellation.
10. Persist the final job state.

Supported job states should include at least:

- Pending
- Running
- Completed
- Failed
- Cancelled

Jobs should survive application restarts and support safe retry or
resume behavior where applicable.

### 4.4 Approval Workflow

When an operation requires human approval:

1. The system creates an approval request.
2. The workflow pauses at the approval gate.
3. An authorized reviewer inspects the generated result and evidence.
4. The reviewer can:
   - Approve.
   - Reject.
   - Edit and approve.
5. The decision is persisted.
6. The workflow resumes or terminates according to the decision.
7. The approval action is recorded in the audit trail.

No side-effecting or destructive tool should execute before the required
approval has been granted.

### 4.5 Observability Workflow

Each important operation should be traceable using a correlation ID
and a workflow/run ID.

The system should allow authorized users to inspect:

- Request status.
- Job status.
- Workflow steps.
- Agent execution.
- Retrieval activity.
- Tool calls.
- Approval decisions.
- Errors and failures.
- Token and cost information where available.
## 5. Functional Requirements

### FR-01: Document Ingestion

The system shall support ingestion of government documents from at least
two supported document formats.

The ingestion pipeline shall:

- Extract text from the document.
- Clean and normalize the extracted content.
- Split the content into meaningful chunks.
- Generate embeddings.
- Store the chunks and their metadata.
- Index the content for retrieval.
- Track the processing status of each document.
- Record processing failures.
- Support idempotent re-ingestion.

Each indexed chunk shall preserve, where available:

- Source document.
- Section.
- Page.
- Clause or article.
- Document version.

### FR-02: Retrieval

The system shall retrieve relevant information from the indexed knowledge
base to support user questions.

The retrieval system shall:

- Use deliberate document chunking.
- Support hybrid retrieval using dense vector search and keyword search.
- Combine retrieval results using a documented fusion strategy.
- Return exact source chunks used to support the response.
- Provide citations for retrieved evidence.
- Refuse to generate a definitive answer when sufficient evidence cannot
  be found.

The retrieval implementation shall include at least one documented
retrieval enhancement beyond the basic hybrid search.

### FR-03: Evaluation

The system shall provide a runnable evaluation harness for measuring
retrieval and answer quality.

The evaluation dataset shall contain:

- At least 25 golden question/answer cases.
- At least 5 adversarial cases.

Adversarial cases shall include examples covering:

- Out-of-corpus questions.
- Ambiguous questions.
- Prompt injection attempts.
- Conflicting sources.

The evaluation harness shall measure at least:

- Retrieval hit rate.
- Answer groundedness.
- Refusal correctness.

The system shall record actual baseline measurements and provide an
interpretation of the results.

### FR-04: Multi-Agent System

The system shall implement a multi-agent architecture containing at least
three specialized agents and an orchestrator.

Each specialized agent shall have:

- An explicit role.
- Restricted tools.
- Typed input and output.
- A clear termination condition.

The system shall provide at least four tools, including at least one
tool capable of producing a write or other side effect.

Side-effecting tools shall never execute without the required human
approval.

The orchestrator shall coordinate the specialized agents and manage the
overall workflow.

### FR-05: Workflow Orchestration

The system shall implement an explicit orchestration pattern for
multi-agent workflows.

The orchestration layer shall provide:

- A documented orchestration pattern.
- Maximum iteration limits.
- Per-step timeouts.
- Retry and backoff behavior.
- Graceful degradation to plain RAG when agentic execution cannot
  continue safely.
- Inspectable execution using a unique run ID.

The approval workflow shall support:

- Approve.
- Reject.
- Edit and approve.

All approval decisions shall be persisted and auditable.

### FR-06: Asynchronous Long-Running Jobs

The system shall support asynchronous processing for long-running
operations.

Submitting a long-running operation shall:

- Return immediately with a unique job ID.
- Persist the job state.
- Place the job into a durable queue.
- Process the job using a background worker.
- Persist progress.
- Push progress updates to the client.
- Support client-requested cancellation.

Jobs shall survive application restarts.

The system shall support safe retry, resume, and idempotent processing
where applicable.

### FR-07: Real-Time Progress

The system shall provide real-time progress updates for asynchronous jobs.

The client shall be able to receive:

- Job status changes.
- Progress updates.
- Relevant processing messages.
- Completion or failure notifications.

The system shall support token streaming for applicable AI responses using
a supported streaming mechanism such as Server-Sent Events (SSE) or
WebSockets.

Client cancellation shall propagate to the server and stop the associated
server-side work.

### FR-08: HTTP API and User Interface

The system shall expose an HTTP API documented using OpenAPI.

The platform shall provide a minimal user interface or CLI that supports
at least:

- Document ingestion.
- Asking questions and viewing citations.
- Starting workflows.
- Viewing workflow progress.
- Reviewing approval requests.
- Approving, rejecting, or editing responses.
- Viewing execution traces.
- Viewing persistent history.

### FR-09: Authentication and Authorization

The system shall require authentication for protected operations.

The system shall provide at least two roles with genuinely different
server-enforced permissions.

The initial roles shall be:

- Citizen.
- Reviewer.
- Administrator.

Authorization shall be enforced on the server and shall not rely only on
frontend visibility.

### FR-10: Observability

The system shall provide observability across the complete execution path.

The system shall support:

- Correlation IDs.
- Workflow/run IDs.
- Request tracing.
- Agent execution tracing.
- LLM interaction tracing.
- Tool-call tracing.
- Error logging.
- Health checks.
- Readiness checks.
- Persistent token and cost information where available.

A request shall be traceable from the initial API request through the
orchestrator, agents, tools, and external AI provider.

### FR-11: Persistent History

The system shall persist relevant system history, including:

- User requests.
- Job states.
- Workflow runs.
- Agent execution information.
- Approval requests.
- Approval decisions.
- Relevant audit events.

Users shall only be able to access history that their role and permissions
allow them to access.

### FR-12: Security Controls

The system shall implement security controls relevant to both web
applications and LLM-based applications.

The system shall include:

- Prompt injection defenses.
- Tool allow-lists.
- Approval gates for side-effecting tools.
- PII redaction where applicable.
- Rate limits and resource caps.
- Secure handling of configuration and secrets.
- Dependency and security scanning.

The system shall include at least three documented prompt injection
cases and demonstrate that unsafe instructions are not blindly followed.

### FR-13: Provider Abstraction

The system shall provide an abstraction layer for AI providers.

The abstraction shall support:

- Text completion.
- Streaming responses.
- Tool calling.
- Embeddings.

The system shall provide at least two working provider implementations:

1. A hosted API provider.
2. An alternative or local provider.

The active provider shall be selectable through configuration without
changing business logic.

A fallback provider chain shall be supported where applicable.

## 6. Non-Functional Requirements

### NFR-01: Architecture

The system shall follow Clean Architecture principles.

The architecture shall maintain clear separation between:

- Domain logic.
- Application use cases and interfaces.
- Infrastructure implementations.
- API and presentation concerns.

The Domain and Application layers shall not depend on:

- Web frameworks.
- LLM provider SDKs.
- Vector database SDKs.
- Specific infrastructure implementations.

External technologies shall be accessed through abstractions and adapters.

### NFR-02: Maintainability

The system shall be designed so that major external technologies can be
replaced without changing business logic.

Replacing an:

- LLM provider.
- Embedding model.
- Vector store.

should require configuration changes and/or a corresponding adapter rather
than changes to core business logic.

Dependency Injection shall be used throughout the application.

### NFR-03: Reliability

The system shall handle failures gracefully.

The system shall provide:

- Explicit domain and application errors.
- Retry policies where appropriate.
- Timeouts for external operations.
- Graceful degradation when optional capabilities fail.
- Persistent job states.
- Safe retry and idempotent processing where applicable.

### NFR-04: Observability

Important system operations shall be observable and traceable.

The system shall provide:

- Correlation IDs.
- Structured logging.
- Distributed tracing where applicable.
- Workflow/run identifiers.
- Agent execution information.
- Tool execution information.
- Token and cost information where available.
- Health and readiness endpoints.

Sensitive information and secrets shall not be written to logs.

### NFR-05: Security

The system shall follow secure development practices and address relevant
OWASP Web Application Security risks and LLM-specific security risks.

Security controls shall include:

- Authentication.
- Server-side authorization.
- Input validation.
- Prompt injection defenses.
- Tool allow-lists.
- Approval gates for side-effecting operations.
- PII redaction where applicable.
- Rate limiting.
- Resource limits.
- Secure secret management.
- Dependency scanning.
- Secret scanning.

No real secrets shall be committed to the repository or its history.

### NFR-06: Data Integrity

The system shall preserve the integrity and traceability of indexed
knowledge.

Document ingestion shall be idempotent.

Stored information shall preserve relevant source metadata and document
versions.

Approval decisions and important workflow events shall be auditable.

### NFR-07: Performance

Long-running operations shall be processed asynchronously so that the
initial HTTP request is not blocked.

The system shall provide progress information for long-running jobs.

External operations shall use appropriate:

- Timeouts.
- Retry limits.
- Resource limits.

The system shall avoid unbounded agent iterations or tool execution.

### NFR-08: Scalability

The architecture should allow independent scaling of components such as:

- API services.
- Background workers.
- Database services.
- Vector search.
- Queue infrastructure.

Long-running processing shall be decoupled from the HTTP request lifecycle.

### NFR-09: Availability and Recovery

The system shall maintain sufficient persistent state to recover from
application restarts.

Queued jobs shall not be silently lost when an application instance
restarts.

The system shall provide health and readiness checks for operational
monitoring.

### NFR-10: Configuration

Environment-specific configuration shall be externalized.

The repository shall provide a complete `.env.example` containing required
configuration keys without real secrets.

Secrets shall never be stored directly in source code, configuration files,
logs, generated traces, or committed repository history.

### NFR-11: Testability

Core domain and application logic shall be testable independently of
external AI providers and infrastructure services.

Tests shall include:

- Unit tests.
- Integration tests.
- Contract tests for agent and tool schemas.

LLM-dependent tests shall use stubs or controlled test doubles where
appropriate.

### NFR-12: Documentation

The project shall maintain documentation covering:

- Business requirements.
- System architecture.
- Important architecture decisions.
- Setup and development instructions.
- API usage.
- Security considerations.
- Agentic workflow.
- AI usage during development.
- Evaluation methodology and results.

The project shall also include the required teaching materials and
demonstration documentation.

### NFR-13: Reproducibility

A fresh checkout of the repository shall be able to start the complete
system using the documented setup procedure.

The project shall provide a Docker Compose or equivalent setup for the
required infrastructure and application services.

Dependencies shall be pinned using appropriate lock files or equivalent
dependency management mechanisms.

## 7. Scope

### 7.1 In Scope

The following capabilities are included in the project:

- Government service and regulation assistance.
- Retrieval-Augmented Generation (RAG).
- Government document ingestion.
- Hybrid document retrieval.
- Source citations.
- Multi-agent orchestration.
- Specialized domain agents.
- Human-in-the-loop approval.
- Side-effecting tool approval gates.
- Asynchronous long-running jobs.
- Durable job processing.
- Job progress tracking.
- Real-time progress updates.
- Client-requested job cancellation.
- Authentication and authorization.
- Persistent workflow and audit history.
- Evaluation datasets and evaluation harness.
- Security controls for web and LLM-related threats.
- Observability and tracing.
- HTTP API with OpenAPI documentation.
- Minimal web user interface.
- Docker-based local development environment.
- Teaching materials and demonstration videos required by the assessment.

### 7.2 Out of Scope

The following capabilities are outside the initial project scope:

- Integration with real government production systems.
- Processing of private or confidential government data.
- Real-world execution of government transactions.
- Production deployment for real citizens.
- Full enterprise-scale infrastructure.
- Native mobile applications.
- Advanced visual design beyond the required functional interface.
- Support for every possible government service or regulation.

The project will use public or synthetic data suitable for demonstration
and evaluation.

## 8. Project Constraints

The project is subject to the following constraints:

### 8.1 Assessment Variant

The assigned domain variant is:

- D4 — Government: Citizen Services & Regulations.

The domain variant is fixed and shall not be changed.

### 8.2 Mandatory Twist

The assigned mandatory twist is:

- T7 — Async Long-Running Jobs.

The implementation must include a real asynchronous queue and worker
architecture rather than a simulated asynchronous workflow.

### 8.3 Architecture

The system shall follow Clean Architecture principles.

Core business logic shall remain independent from:

- Web frameworks.
- LLM SDKs.
- Vector-store SDKs.
- Specific infrastructure implementations.

### 8.4 AI Provider Abstraction

The system shall provide a provider abstraction with at least two working
implementations.

The selected provider shall be configurable without modifying business
logic.

### 8.5 Human Approval

Human approval shall be mandatory before executing side-effecting tools or
other operations explicitly requiring approval.

### 8.6 Security

The system shall not store real secrets in the repository or its history.

The project shall include security controls and documented prompt injection
tests.

### 8.7 Evaluation

The project shall include the required golden and adversarial evaluation
cases and a runnable evaluation harness.

### 8.8 Engineering Process

Development shall use:

- Feature branches.
- Pull requests.
- Meaningful commits.
- Automated CI.
- Issue tracking.
- Documentation of important architectural decisions.

## 9. Success Criteria

The project will be considered successful when the following conditions
are satisfied:

### Functional Success

- Citizens can ask government-related questions.
- The system retrieves relevant evidence.
- Responses include source citations.
- The system refuses unsupported answers.
- Multiple specialized agents can participate in a workflow.
- Human approval can pause and resume the workflow.
- Long-running operations execute through a durable queue and worker.
- Job progress is visible to the client.
- Jobs support cancellation and safe retry/resume behavior.
- Authentication and server-side authorization are enforced.

### Quality and Evaluation Success

- The evaluation harness runs successfully.
- The required golden dataset is available.
- Adversarial cases are included.
- Retrieval hit rate is measured.
- Answer groundedness is measured.
- Refusal correctness is measured.
- Actual baseline results are recorded and interpreted.

### Security Success

- Prompt injection defenses are tested.
- Tool access is restricted through allow-lists.
- Side-effecting operations require approval.
- Secrets are absent from the repository and its history.
- Dependency and security scanning is configured.

### Engineering Success

- The system follows Clean Architecture.
- Required tests are implemented.
- CI runs successfully.
- Documentation is complete.
- The system can be started from a fresh checkout using the documented
  setup procedure.
- Required teaching materials and videos are completed.