# Use an official .NET SDK image as a base
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS base

# Install gRPC tools and protobuf-compiler
# Using a specific version of protobuf-tools-google that is known to work
# Update package lists and install prerequisites
RUN apt-get update && apt-get install -y curl unzip

# Download and install protoc
ENV PROTOC_VERSION=25.3
RUN curl -Lo protoc.zip https://github.com/protocolbuffers/protobuf/releases/download/v${PROTOC_VERSION}/protoc-${PROTOC_VERSION}-linux-x86_64.zip \
    && unzip protoc.zip -d /usr/local \
    && rm protoc.zip

# Install gRPC C# tools
# These are typically installed as NuGet packages within a project,
# but having them globally available or ensuring protoc can find plugins might be useful.
# For now, we'll rely on project-level dependencies for Grpc.Tools.
# The SDK image includes dotnet CLI, which can manage these.

# Set working directory
WORKDIR /workspace

# Keep the container running
CMD ["sleep", "infinity"]
