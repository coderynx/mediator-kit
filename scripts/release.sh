#!/bin/bash

# Mediator Kit Release Script
# This script helps create releases with proper versioning and changelog updates

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Check if nbgv is installed
if ! command -v nbgv &> /dev/null; then
    echo -e "${RED}Error: nbgv (Nerdbank.GitVersioning) is not installed.${NC}"
    echo "Install it with: dotnet tool install --global nbgv"
    exit 1
fi

# Function to print colored output
print_info() {
    echo -e "${GREEN}$1${NC}"
}

print_warning() {
    echo -e "${YELLOW}$1${NC}"
}

print_error() {
    echo -e "${RED}$1${NC}"
}

# Function to get current version
get_current_version() {
    nbgv get-version -v SemVer2
}

# Function to create a new release
create_release() {
    local release_type=$1
    
    print_info "Creating a new $release_type release..."
    
    # Get current version
    current_version=$(get_current_version)
    print_info "Current version: $current_version"
    
    # Update version based on release type
    case $release_type in
        "major")
            nbgv prepare-release --tag
            ;;
        "minor")
            nbgv prepare-release --tag
            ;;
        "patch")
            nbgv prepare-release --tag
            ;;
        *)
            print_error "Invalid release type. Use: major, minor, or patch"
            exit 1
            ;;
    esac
    
    new_version=$(get_current_version)
    print_info "New version will be: $new_version"
    
    # Ask for confirmation
    read -p "Do you want to create release $new_version? (y/N): " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        print_warning "Release cancelled."
        exit 0
    fi
    
    # Create git tag
    tag_name="v$new_version"
    git tag -a "$tag_name" -m "Release $new_version"
    
    print_info "Created tag: $tag_name"
    print_info "Push the tag to trigger the release workflow:"
    print_info "git push origin $tag_name"
}

# Function to show current status
show_status() {
    print_info "Current Git Status:"
    git status --short
    echo
    
    print_info "Current Version:"
    get_current_version
    echo
    
    print_info "Available tags:"
    git tag --sort=-version:refname | head -5
}

# Function to build and test locally
build_and_test() {
    print_info "Building and testing locally..."
    
    dotnet restore src/Coderynx.MediatorKit.Abstractions/Coderynx.MediatorKit.Abstractions.csproj
    dotnet restore src/Coderynx.MediatorKit/Coderynx.MediatorKit.csproj
    dotnet restore tests/Coderynx.CqrsKit.Tests/Coderynx.CqrsKit.Tests.csproj
    
    dotnet build src/Coderynx.MediatorKit.Abstractions/Coderynx.MediatorKit.Abstractions.csproj --configuration Release
    dotnet build src/Coderynx.MediatorKit/Coderynx.MediatorKit.csproj --configuration Release
    
    dotnet test tests/Coderynx.CqrsKit.Tests/Coderynx.CqrsKit.Tests.csproj --configuration Release --verbosity normal
    
    print_info "Build and tests completed successfully!"
}

# Function to create local packages
create_packages() {
    print_info "Creating NuGet packages..."
    
    # Clean previous packages
    rm -rf ./nupkg
    mkdir -p ./nupkg
    
    dotnet pack src/Coderynx.MediatorKit.Abstractions/Coderynx.MediatorKit.Abstractions.csproj --configuration Release --output ./nupkg
    dotnet pack src/Coderynx.MediatorKit/Coderynx.MediatorKit.csproj --configuration Release --output ./nupkg
    
    print_info "Packages created in ./nupkg/:"
    ls -la ./nupkg/
}

# Main script logic
case "${1:-help}" in
    "status")
        show_status
        ;;
    "build")
        build_and_test
        ;;
    "pack")
        build_and_test
        create_packages
        ;;
    "release")
        if [ -z "$2" ]; then
            print_error "Please specify release type: major, minor, or patch"
            print_info "Usage: $0 release <major|minor|patch>"
            exit 1
        fi
        build_and_test
        create_release "$2"
        ;;
    "help"|*)
        echo "Mediator Kit Release Script"
        echo ""
        echo "Usage: $0 <command> [options]"
        echo ""
        echo "Commands:"
        echo "  status                 Show current git and version status"
        echo "  build                  Build and test the solution"
        echo "  pack                   Build, test, and create NuGet packages"
        echo "  release <type>         Create a new release (major, minor, patch)"
        echo "  help                   Show this help message"
        echo ""
        echo "Examples:"
        echo "  $0 status"
        echo "  $0 build"
        echo "  $0 pack"
        echo "  $0 release patch"
        ;;
esac