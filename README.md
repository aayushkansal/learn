# Survey & Roundtable Microservices Project

This project aims to build a microservices-based platform for managing surveys and roundtables. Users or groups of users can participate in surveys, and these surveys and groups are organized based on roundtables.

## Technology Stack

- **Framework:** .NET Core
- **Architecture:** Microservices
- **Communication:** gRPC
- **Deployment:** Docker containers orchestrated by Kubernetes

## Project Structure

The project is organized into the following main directories:

- `src/Services/`: Contains the source code for individual microservices.
  - `Users/`: Manages user information.
  - `Clients/`: Manages client information (entities that can create/own roundtables/surveys).
  - `Roundtables/`: Manages roundtables, including user participation.
  - `Surveys/`: Manages surveys, questions, and responses, linked to roundtables.
  - `Meeting/`: Manages meeting schedules and details related to roundtables.
- `src/Shared/`: Contains shared libraries, common code, or DTOs used across multiple services.
- `deployments/`: Contains deployment-related files.
  - `dockerfiles/`: Dockerfiles for building service images.
  - `kubernetes/`: Kubernetes manifests for deploying the services.

## Microservice Architecture

The system is composed of several microservices, each responsible for a specific domain:

- **UserService:** Handles user creation, authentication, and profile management.
  - gRPC definition: `src/Services/Users/users.proto`
- **ClientService:** Manages clients who can initiate roundtables or surveys.
  - gRPC definition: `src/Services/Clients/clients.proto`
- **RoundtableService:** Manages the lifecycle of roundtables, including associating users.
  - gRPC definition: `src/Services/Roundtables/roundtables.proto`
- **SurveyService:** Manages surveys, their questions, and collects responses from users. Surveys are typically associated with a roundtable.
  - gRPC definition: `src/Services/Surveys/surveys.proto`
- **MeetingService:** Handles scheduling and management of meetings related to roundtables.
  - gRPC definition: `src/Services/Meeting/meeting.proto`

### Communication

Services communicate with each other primarily via gRPC. Each service exposes a gRPC API defined by `.proto` files located within their respective directories.

### Data Management

Each microservice is expected to have its own dedicated database to ensure loose coupling. The specific database technology per service can be chosen based on its needs (e.g., SQL Server, PostgreSQL, MongoDB).

## Getting Started

(This section will be updated with build, test, and deployment instructions as the project develops.)

### Prerequisites

- .NET Core SDK
- Docker
- kubectl (for Kubernetes deployment)
- Protocol Buffer Compiler (`protoc`) and gRPC tools for .NET

### Initial Setup

1.  **Clone the repository:**
    ```bash
    git clone <repository_url>
    cd <project_directory>
    ```
2.  **Compile Protocol Buffers:**
    For each service, navigate to its directory (e.g., `src/Services/Users/`) and use `protoc` to generate the C# gRPC stubs from the `.proto` files. This step will be integrated into the build process later.

## Future Development

- Implement the gRPC services and business logic for each microservice.
- Set up build pipelines for each service.
- Create Dockerfiles for containerization.
- Write Kubernetes manifests for deployment and orchestration.
- Implement inter-service communication patterns (e.g., API Gateway, direct calls, event-driven).
- Add authentication and authorization.
- Set up logging, monitoring, and tracing.
- Write comprehensive unit and integration tests.
