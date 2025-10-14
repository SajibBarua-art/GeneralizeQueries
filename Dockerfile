# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used for the final production image. It contains the .NET runtime.
# Using the aspnet image is fine for Api services and is often a standard base image.
FROM docker.bracits.com/mcr/dotnet/aspnet:8.0 AS base
WORKDIR /app


# This stage is used to build the service project. It contains the full .NET SDK.
FROM docker.bracits.com/mcr/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy all .csproj files first. This leverages Docker layer caching.
# The 'dotnet restore' step below will only re-run if these files change.
# IMPORTANT: Replace 'GeneralizeQueries.Api' if your project is named differently.
COPY ["GeneralizeQueries.Api/GeneralizeQueries.Api.csproj", "GeneralizeQueries.Api/"]
COPY ["GeneralizeQueries.Application/GeneralizeQueries.Application.csproj", "GeneralizeQueries.Application/"]
COPY ["GeneralizeQueries.Core/GeneralizeQueries.Core.csproj", "GeneralizeQueries.Core/"]
COPY ["GeneralizeQueries.Infrastructure/GeneralizeQueries.Infrastructure.csproj", "GeneralizeQueries.Infrastructure/"]

# Copy your private NuGet feed configuration, if you have one.
RUN mkdir -p /root/.nuget/NuGet
COPY ./config/NuGetPackageSource.Config /root/.nuget/NuGet/NuGet.Config

# Restore NuGet packages for the main Api project.
RUN dotnet restore "GeneralizeQueries.Api/GeneralizeQueries.Api.csproj"

# Copy the rest of the source code into the container.
COPY . .

# Set the working directory to the main project folder.
WORKDIR "/src/GeneralizeQueries.Api"

# Build the project.
RUN dotnet build "GeneralizeQueries.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage.
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "GeneralizeQueries.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This is the final, small, production-ready image.
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Optional: Include your APM agent by copying it from its base image.
#COPY --from=docker.bracits.com/app-modernization/apm-agent:1.31.0 /elastic_apm_profiler /elastic_apm_profiler

# Set the entrypoint to run the Api service.
# IMPORTANT: Replace the DLL name if your project name is different.
ENTRYPOINT ["dotnet", "GeneralizeQueries.Api.dll"]