#!/bin/bash

# BudgetForHouseholds Development Startup Script
# This script starts both the backend API and frontend React app

set -e  # Exit on any error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Function to check if a port is in use
check_port() {
    local port=$1
    if lsof -i :$port >/dev/null 2>&1; then
        return 0  # Port is in use
    else
        return 1  # Port is free
    fi
}

# Function to cleanup processes on exit
cleanup() {
    print_info "Shutting down applications..."
    if [ ! -z "$BACKEND_PID" ]; then
        kill $BACKEND_PID 2>/dev/null || true
        print_info "Backend stopped"
    fi
    if [ ! -z "$FRONTEND_PID" ]; then
        kill $FRONTEND_PID 2>/dev/null || true
        print_info "Frontend stopped"
    fi
    exit 0
}

# Set up signal handlers
trap cleanup SIGINT SIGTERM

print_info "Starting BudgetForHouseholds Development Environment..."

# Check if required directories exist
if [ ! -d "BudgetApp.Api" ]; then
    print_error "BudgetApp.Api directory not found!"
    exit 1
fi

if [ ! -d "budget-app-frontend" ]; then
    print_error "budget-app-frontend directory not found!"
    exit 1
fi

# Check if required tools are installed
if ! command -v dotnet &> /dev/null; then
    print_error "dotnet CLI not found! Please install .NET 8 SDK"
    exit 1
fi

if ! command -v npm &> /dev/null; then
    print_error "npm not found! Please install Node.js and npm"
    exit 1
fi

# Check if ports are available
if check_port 5291; then
    print_warning "Port 5291 is already in use. Backend API might already be running."
    read -p "Continue anyway? (y/N): " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        exit 1
    fi
fi

if check_port 3000; then
    print_warning "Port 3000 is already in use. Frontend might already be running."
    read -p "Continue anyway? (y/N): " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        exit 1
    fi
fi

print_info "Installing/updating dependencies..."

# Restore .NET packages
print_info "Restoring .NET packages..."
cd BudgetApp.Api
if dotnet restore; then
    print_success ".NET packages restored"
else
    print_error "Failed to restore .NET packages"
    exit 1
fi
cd ..

# Install npm packages
print_info "Installing npm packages..."
cd budget-app-frontend
if npm install; then
    print_success "npm packages installed"
else
    print_error "Failed to install npm packages"
    exit 1
fi
cd ..

print_success "Dependencies installed successfully!"

# Start backend API
print_info "Starting Backend API..."
cd BudgetApp.Api
dotnet run &
BACKEND_PID=$!
cd ..

# Wait a moment for backend to start
sleep 3

# Start frontend
print_info "Starting Frontend..."
cd budget-app-frontend
npm start &
FRONTEND_PID=$!
cd ..

print_success "Applications started!"
print_info "Backend API: http://localhost:5291"
print_info "Frontend: http://localhost:3000"
print_info "API Documentation: http://localhost:5291/swagger"
print_info ""
print_info "Press Ctrl+C to stop both applications"

# Wait for both processes
wait