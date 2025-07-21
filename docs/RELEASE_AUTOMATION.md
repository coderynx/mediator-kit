# Release Automation Documentation

This document explains the automated versioning and release process implemented for the MediatorKit project.

## Overview

The project uses **Nerdbank.GitVersioning** for automatic version management and **GitHub Actions** for CI/CD automation.

## Version Management

### Automatic Versioning

- Versions are automatically calculated based on Git history and tags
- The base version is defined in `version.json`
- Pre-release versions include the Git commit hash (e.g., `1.4.0-g767eaf7b89`)
- Release versions are clean semantic versions (e.g., `1.4.0`)

### Version Configuration

The `version.json` file controls versioning behavior:

```json
{
  "version": "1.4.0",
  "publicReleaseRefSpec": [
    "^refs/heads/main$",
    "^refs/tags/v\\d+(?:\\.\\d+)*"
  ]
}
```

## Workflows

### 1. CI Workflow (`ci.yml`)

**Triggers:**
- Every push to `main` branch
- Every pull request to `main` branch

**Actions:**
- Builds all projects
- Runs all tests
- Creates NuGet packages
- Uploads artifacts for download

### 2. Release Workflow (`release.yml`)

**Triggers:**
- When a version tag is pushed (e.g., `v1.4.1`)
- Manual workflow dispatch

**Actions:**
- Builds and tests the code
- Creates NuGet packages
- Generates changelog from commits
- Creates GitHub release with artifacts
- Publishes packages to NuGet.org

### 3. Publish Workflow (`publish-nuget.yml`)

**Triggers:**
- When a GitHub release is published
- Manual workflow dispatch

**Actions:**
- Publishes packages to NuGet.org
- Handles pre-release and stable release logic

## Creating Releases

### Method 1: Using the Release Script (Recommended)

```bash
# Create a patch release (1.4.0 -> 1.4.1)
./scripts/release.sh release patch

# Create a minor release (1.4.0 -> 1.5.0)
./scripts/release.sh release minor

# Create a major release (1.4.0 -> 2.0.0)
./scripts/release.sh release major
```

The script will:
1. Prepare the release using GitVersioning
2. Create a Git tag
3. Provide instructions for pushing the tag

### Method 2: Manual Tag Creation

```bash
# Create and push a version tag
git tag v1.4.1
git push origin v1.4.1
```

### Method 3: GitHub UI

1. Go to the GitHub repository
2. Navigate to "Releases"
3. Click "Create a new release"
4. Create a new tag (e.g., `v1.4.1`)
5. Fill in release notes
6. Publish the release

## Security

### Required Secrets

The following secrets must be configured in GitHub repository settings:

- `NUGET_API_KEY`: API key for publishing to NuGet.org

### API Key Management

1. Generate a new API key at [nuget.org](https://www.nuget.org/account/apikeys)
2. Scope the key to the specific packages (`Coderynx.MediatorKit*`)
3. Set appropriate permissions (Push new packages and package versions)
4. Add the key to GitHub Secrets

## Changelog

The changelog is maintained in `CHANGELOG.md` following [Keep a Changelog](https://keepachangelog.com/) format.

### Automatic Generation

The release workflow automatically generates release notes by:
1. Extracting relevant section from `CHANGELOG.md` if available
2. Falling back to commit messages since the last release

## Local Development

### Building and Testing

```bash
# Check project status
./scripts/release.sh status

# Build and test
./scripts/release.sh build

# Create packages locally
./scripts/release.sh pack
```

### Version Information

```bash
# Get current version
nbgv get-version

# Get specific version format
nbgv get-version -v NuGetPackageVersion
nbgv get-version -v SemVer2
```

## Troubleshooting

### Common Issues

1. **Version not updating**: Ensure you're on the correct branch and have proper Git history
2. **Package conflicts**: Clear local NuGet cache and rebuild
3. **Workflow failures**: Check secret configuration and permissions

### Debug Commands

```bash
# Check Git versioning configuration
nbgv get-version --verbosity detailed

# Validate version.json
nbgv install --verify

# Test package creation locally
dotnet pack --configuration Release --verbosity normal
```

## Migration Notes

### Changes from Manual Process

- Removed hardcoded version numbers from `.csproj` files
- Added GitVersioning package reference in `Directory.Build.props`
- Updated workflows for security and automation

### Breaking Changes

- Secret name changed from `NUGET` to `NUGET_API_KEY`
- Publishing now requires version tags for releases
- Pre-release packages have different version format