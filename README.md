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
  - `Master/`: Contains `master.proto`, C# implementation (`MasterServiceImplementation.cs`, `RoundtableLogic.cs`), and the project file (`MasterService.csproj`).
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
  - `dockerfiles/`: Dockerfiles for building service images (e.g., `Master/Dockerfile` for MasterService).
  - `kubernetes/`: Kubernetes manifests for deploying the services.
- `.devcontainer/`: Contains the configuration for the Dev Container development environment.
  - `Dockerfile`: Defines the Docker image for the dev environment, including .NET SDK and `protoc`.
  - `devcontainer.json`: Configures the Dev Container settings and VS Code extensions.

## Microservice Architecture

The system is composed of several microservices, each responsible for a specific domain. Each service will expose a gRPC API defined by `.proto` files located within their respective directories (e.g., `src/Services/User/user.proto`).

**Core Modules:**
- **MasterService:** Manages master data, configurations, and Roundtables (including their definition, listing, creation, and other related operations).
- **MeetingService:** Handles scheduling and management of meetings.
- **SurveyService:** Manages surveys, questions, and responses.
- **UserService:** Handles user creation, authentication, and profile management.
- **CommunityService:** Manages community features. (Details of specific features to be defined).
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

-   **Container Runtime:**
    *   **Podman:** Recommended. Install Podman for your operating system. (e.g., from [podman.io](https://podman.io/getting-started/installation)) Ensure the Podman service is running if you are using Podman Desktop or need a socket for VS Code (e.g., `podman machine start` on macOS/Windows, or ensure the user systemd socket is active on Linux: `systemctl --user start podman.socket`).
    *   **Docker Desktop:** Alternatively, Docker Desktop can be used.
-   **Visual Studio Code:** Install VS Code.
-   **VS Code Remote - Containers extension:** Install the "Remote - Containers" (ms-vscode-remote.remote-containers) extension from the VS Code Marketplace.

### Configuring VS Code with Podman

The "Remote - Containers" extension in VS Code typically looks for a Docker socket to communicate with the container runtime. Podman can provide a compatible socket.

1.  **Ensure Podman is installed and running.**
2.  **Socket Activation (Linux):** If you are on Linux and not using Podman Desktop, ensure the Podman API socket is active:
    ```bash
    systemctl --user enable --now podman.socket
    ```
    You can check its status with `systemctl --user status podman.socket`. The socket is often located at `/run/user/$UID/podman/podman.sock`.
3.  **Socket for macOS/Windows (Podman Desktop):** If using Podman Desktop, the virtual machine it runs usually exposes a compatible Docker API socket. Ensure your Podman machine is running.
4.  **VS Code Detection:**
    *   VS Code's "Remote - Containers" extension, when using its default settings, should be able to detect and use the Podman-provided Docker-compatible socket if it's available at the default Docker socket locations or if `DOCKER_HOST` environment variable is set.
    *   For explicit configuration or troubleshooting, refer to the official VS Code documentation on [Using Podman with Dev Containers](https://code.visualstudio.com/docs/devcontainers/containers#_using-podman). Some users might need to set the `docker.host` setting in VS Code's `settings.json` if the socket is in a non-standard location, or ensure that the `DOCKER_HOST` environment variable points to the Podman socket (e.g., `export DOCKER_HOST=unix:///run/user/$UID/podman/podman.sock` on Linux).

*The `Dockerfile` provided in `.devcontainer/Dockerfile` is compatible with both Podman and Docker.*

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
    - The first time you open the project in the Dev Container, VS Code (using Podman/Docker) will build the Docker image defined in `.devcontainer/Dockerfile`. This might take a few minutes.
    - You can see the progress in the terminal window.
    - Once the build is complete, VS Code will connect to the container, and your project files will be available. The `postCreateCommand` in `devcontainer.json` will run, verifying `dotnet` and `protoc` versions.
4.  **Developing in the Container:**
    - You can now open terminals, install dependencies, run `dotnet` commands, compile `.proto` files, and debug your application directly from within the Dev Container environment.
    - All required tools (like the .NET SDK and `protoc`) are available in the container.

### Rebuilding the Dev Container

If you make changes to `devcontainer.json` or the `Dockerfile` in `.devcontainer/`, you'll need to rebuild the container for the changes to take effect.
- Open the Command Palette (Ctrl+Shift+P or Cmd+Shift+P).
- Type/select "Remote-Containers: Rebuild Container".

## Future Development

- Implement the gRPC services and business logic for each microservice.
- Define `.csproj` files for each service (e.g., `MasterService.csproj` created for Master service) and configure `Grpc.Tools` for automatic C# stub generation from `.proto` files during build.
- Set up build pipelines for each service.
- Create production Dockerfiles for each service (e.g., `deployments/dockerfiles/Master/Dockerfile` created for MasterService).
- Write Kubernetes manifests for deployment and orchestration.
- Implement inter-service communication patterns (e.g., API Gateway, direct calls, event-driven).
- Add authentication and authorization.
- Set up logging, monitoring, and tracing.
- Write comprehensive unit and integration tests.
