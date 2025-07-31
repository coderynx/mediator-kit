# MediatorKit

![CI](https://github.com/coderynx/mediator-kit/workflows/CI/badge.svg)
![Release](https://github.com/coderynx/mediator-kit/workflows/Release/badge.svg)
[![NuGet](https://img.shields.io/nuget/v/Coderynx.MediatorKit.svg)](https://www.nuget.org/packages/Coderynx.MediatorKit/)

## Introduction

**Coderynx.MediatorKit** is a lightweight, extensible mediator library for .NET applications. It simplifies the
communication between components through strongly-typed requests, promoting clean architecture and
reducing tight coupling.

## Features

- Simple and intuitive mediator pattern implementation
- Dependency injection ready
- Extensible pipeline behaviors for cross-cutting concerns
- Automated versioning and release process
- Comprehensive CI/CD pipeline

## Installation

Install via NuGet:

```bash
dotnet add package Coderynx.MediatorKit
```

Or using the NuGet Package Manager:

```powershell
Install-Package Coderynx.MediatorKit
```

## Development

### Building the Project

```bash
# Build all projects
./scripts/release.sh build

# Create NuGet packages
./scripts/release.sh pack

# Check project status
./scripts/release.sh status
```

### Automated Releases

This project uses automated versioning and releases:

1. **Versioning**: Uses [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning) for automatic version
   management
2. **CI/CD**: GitHub Actions workflows handle building, testing, and publishing
3. **Releases**: Created automatically when version tags are pushed

#### Creating a Release

```bash
# Create a patch release (1.4.0 -> 1.4.1)
./scripts/release.sh release patch

# Create a minor release (1.4.0 -> 1.5.0)
./scripts/release.sh release minor

# Create a major release (1.4.0 -> 2.0.0)
./scripts/release.sh release major
```

#### Manual Release Process

1. Update the `CHANGELOG.md` with your changes
2. Create and push a version tag:
   ```bash
   git tag v1.4.1
   git push origin v1.4.1
   ```
3. The release workflow will automatically:
    - Build and test the code
    - Create NuGet packages
    - Create a GitHub release
    - Publish packages to NuGet.org

### Security

The automation uses the following secrets:

- `NUGET_API_KEY`: API key for publishing to NuGet.org

Make sure these are properly configured in your GitHub repository settings.
