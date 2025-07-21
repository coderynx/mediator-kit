# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Automated versioning and release process
- GitHub Actions workflows for CI/CD and releases
- Changelog generation
- Security improvements for NuGet publishing

### Changed
- Switched to Nerdbank.GitVersioning for automatic version management
- Updated .NET target framework from 9.0 to 8.0 for compatibility
- Improved project structure and dependencies

### Security
- Updated secret handling in GitHub Actions
- Implemented proper API key management for NuGet publishing

## [1.4.0] - Previous Release

### Added
- Core mediator functionality
- Pipeline behaviors
- Dependency injection integration

## [1.2.0] - Previous Release (Abstractions)

### Added
- Core abstractions for mediator pattern
- Request/response interfaces
- Handler interfaces