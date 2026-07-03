# =========================
# Handicraft Platform Setup
# .NET 9 + PostgreSQL Clean Architecture
# =========================

$solutionName = "HandicraftPlatform"

Write-Host "Creating Solution..." -ForegroundColor Green
dotnet new sln -n $solutionName

# =========================
# Create Projects
# =========================

Write-Host "Creating Projects..." -ForegroundColor Green

dotnet new webapi -n Handicraft.Api
dotnet new classlib -n Handicraft.Domain
dotnet new classlib -n Handicraft.Application
dotnet new classlib -n Handicraft.Infrastructure

# =========================
# Add to Solution
# =========================

Write-Host "Adding Projects to Solution..." -ForegroundColor Green

dotnet sln add Handicraft.Api/Handicraft.Api.csproj
dotnet sln add Handicraft.Domain/Handicraft.Domain.csproj
dotnet sln add Handicraft.Application/Handicraft.Application.csproj
dotnet sln add Handicraft.Infrastructure/Handicraft.Infrastructure.csproj

# =========================
# Project References (Clean Architecture)
# =========================

Write-Host "Setting References..." -ForegroundColor Green

dotnet add Handicraft.Application reference Handicraft.Domain
dotnet add Handicraft.Infrastructure reference Handicraft.Domain
dotnet add Handicraft.Infrastructure reference Handicraft.Application
dotnet add Handicraft.Api reference Handicraft.Application
dotnet add Handicraft.Api reference Handicraft.Infrastructure

# =========================
# Install Packages (Infrastructure)
# =========================

Write-Host "Installing Packages..." -ForegroundColor Green

dotnet add Handicraft.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add Handicraft.Infrastructure package Microsoft.EntityFrameworkCore.Design
dotnet add Handicraft.Infrastructure package Microsoft.EntityFrameworkCore.Tools

# API Packages
dotnet add Handicraft.Api package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add Handicraft.Api package Swashbuckle.AspNetCore

# =========================
# Create Folders
# =========================

Write-Host "Creating Folder Structure..." -ForegroundColor Green

mkdir Handicraft.Domain/Entities
mkdir Handicraft.Application/Services
mkdir Handicraft.Infrastructure/Persistence
mkdir Handicraft.Infrastructure/Repositories
mkdir Handicraft.Api/Controllers

# =========================
# Done
# =========================

Write-Host "DONE! Solution Ready 🚀" -ForegroundColor Cyan
Write-Host "Open: HandicraftPlatform.sln" -ForegroundColor Yellow