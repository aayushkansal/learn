# Survey & Roundtable Microservices Project

This project aims to build a microservices-based platform for managing surveys and roundtables, along with various other business modules. Users or groups of users can participate in surveys, and these surveys and groups are organized based on roundtables.

## Technology Stack

- **Framework:** .NET Core
- **Architecture:** Microservices
- **Communication:** gRPC
- **Deployment:** Docker containers orchestrated by Kubernetes (planned)
- **Development Environment:** Dev Containers

## Project Structure

The project is organized into the following main directories:

- `src/Services/`: Contains the source code for individual microservices.
  - `Master/`
  - `Meeting/`
  - `Survey/`
  - `User/`
  - `Community/`
  - `AuriemmaExchange/`
  - `Home/`
  - `Tool/`
  - `Help/`
  - `Alert/`
  - `AdvanceSearch/`
- `src/Shared/`: Contains shared libraries, common code, or DTOs used across multiple services. (Planned)
- `deployments/`: Contains deployment-related files. (Planned)
  - `dockerfiles/`: Dockerfiles for building service images for production.
  - `kubernetes/`: Kubernetes manifests for deploying the services.
- `.devcontainer/`: Contains the configuration for the Dev Container development environment.
  - `Dockerfile`: Defines the Docker image for the dev environment, including .NET SDK and `protoc`.
  - `devcontainer.json`: Configures the Dev Container settings and VS Code extensions.

## Microservice Architecture

The system is composed of several microservices, each responsible for a specific domain. Each service will expose a gRPC API defined by `.proto` files located within their respective directories (e.g., `src/Services/User/user.proto`).

**Core Modules:**
- **MasterService:** Manages master data or configurations.
- **MeetingService:** Handles scheduling and management of meetings.
- **SurveyService:** Manages surveys, questions, and responses.
- **UserService:** Handles user creation, authentication, and profile management.
- **CommunityService:** Manages community features, potentially linking users and roundtables.
- **AuriemmaExchangeService:** Manages functionalities related to "Auriemma Exchange".
- **HomeService:** Provides data or services for the main dashboard/home screen.
- **ToolService:** Provides various tools or utilities within the platform.
- **HelpService:** Manages help documentation or support features.
- **AlertService:** Handles notifications and alerts.
- **AdvanceSearchService:** Provides advanced search capabilities across modules.


### Communication

Services will communicate with each other primarily via gRPC.

### Data Management

Each microservice is expected to have its own dedicated database to ensure loose coupling.

## Getting Started with Dev Containers

This project is configured to run in a Dev Container, which provides a consistent and fully configured development environment.

### Prerequisites

- **Docker Desktop:** Install Docker Desktop for your operating system.
- **Visual Studio Code:** Install VS Code.
- **VS Code Remote - Containers extension:** Install the "Remote - Containers" (ms-vscode-remote.remote-containers) extension from the VS Code Marketplace.

### Setup and Running the Project Locally

1.  **Clone the repository:**
    ```bash
    git clone <repository_url>
    cd <project_directory>
    ```
2.  **Open in Dev Container:**
    - Open the cloned project folder in VS Code.
    - VS Code should automatically detect the `.devcontainer/devcontainer.json` file and prompt you: "Folder contains a Dev Container configuration file. Reopen in Container?"
    - Click "Reopen in Container".
    - If you don't see the prompt, you can open the Command Palette (Ctrl+Shift+P or Cmd+Shift+P) and type/select "Remote-Containers: Reopen in Container".
3.  **First-time Build:**
    - The first time you open the project in the Dev Container, VS Code will build the Docker image defined in `.devcontainer/Dockerfile`. This might take a few minutes.
    - You can see the progress in the terminal window.
    - Once the build is complete, VS Code will connect to the container, and your project files will be available. The `postCreateCommand` in `devcontainer.json` will run, verifying `dotnet` and `protoc` versions.
4.  **Developing in the Container:**
    - You can now open terminals, install dependencies, run `dotnet` commands, compile `.proto` files, and debug your application directly from within the Dev Container environment.
    - All required tools (like the .NET SDK and `protoc`) are available in the container.
    - For example, to compile a `.proto` file (actual commands will depend on your gRPC C# setup within each project):
      ```bash
      # Example: Navigate to a service directory
      # cd src/Services/User
      # dotnet build (if Grpc.Tools is configured in the .csproj)
      ```

### Rebuilding the Dev Container

If you make changes to `devcontainer.json` or the `Dockerfile` in `.devcontainer/`, you'll need to rebuild the container for the changes to take effect.
- Open the Command Palette (Ctrl+Shift+P or Cmd+Shift+P).
- Type/select "Remote-Containers: Rebuild Container".

## Future Development

- Implement the gRPC services and business logic for each microservice.
- Define `.csproj` files for each service and configure `Grpc.Tools` for automatic C# stub generation from `.proto` files during build.
- Set up build pipelines for each service.
- Create production Dockerfiles for each service.
- Write Kubernetes manifests for deployment and orchestration.
- Implement inter-service communication patterns (e.g., API Gateway, direct calls, event-driven).
- Add authentication and authorization.
- Set up logging, monitoring, and tracing.
- Write comprehensive unit and integration tests.
