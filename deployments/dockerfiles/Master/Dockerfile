# Stage 1: Build the service
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy the .csproj file and restore dependencies
COPY ["src/Services/Master/MasterService.csproj", "src/Services/Master/"]
# Copy the .proto file
COPY ["src/Services/Master/master.proto", "src/Services/Master/"]
# Copy implementation files
COPY ["src/Services/Master/MasterServiceImplementation.cs", "src/Services/Master/"]
COPY ["src/Services/Master/RoundtableLogic.cs", "src/Services/Master/"]

# It's often better to copy only necessary project/solution files first, then restore, then copy rest of source.
# For a single project service like this, the above is okay, but for larger solutions,
# one might copy a solution file, then run restore, then copy all source.
# Given we only have one .csproj for Master service now, this is fine.

WORKDIR /src/src/Services/Master
RUN dotnet restore

# Copy the rest of the source code for the Master service (if any other files were there)
# For now, we've copied all known .cs and .proto files individually.
# If the Master service directory had other subdirectories or files, a broader copy would be needed.
# e.g., COPY ["src/Services/Master/", "./"] if MasterService.csproj was in /src

# Build the service
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Create the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose the port the gRPC service will listen on.
# This should match the Kestrel configuration in the application.
# Using 80 for HTTP, 81 for HTTPS as placeholders if not configured yet.
# Or a single port like 8080 or 5000. Let's use 8080 for non-HTTPS for now.
EXPOSE 8080

# Define the entry point for the container.
# This assumes MasterService is the name of the assembly generated from MasterService.csproj.
ENTRYPOINT ["dotnet", "MasterService.dll"]
