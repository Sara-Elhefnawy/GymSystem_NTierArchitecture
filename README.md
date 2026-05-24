# Gym Management System - Power Fitness

## 📋 Project Overview

Power Fitness is a complete gym management system built with ASP.NET Core MVC using **NTier Architecture** principles.

**Dependency Flow:** UI → Infrastructure → Domain
- UI references Infrastructure (for DI setup)
- Infrastructure references Domain (implements interfaces)
- Domain has NO dependencies on other projects (pure business logic)

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) with ASP.NET workload

### Step 1: Clone the Repository

```bash
git clone https://github.com/Sara-Elhefnawy/GymSystem_NTierArchitecture.git
cd GymSystem
```

### Step 2: Update Database Connection

Open `GymSystem.UI/appsettings.json` and update the connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME; Database=GymSystem; Trusted_Connection=True; TrustServerCertificate=True;"
  }
}
```

### Step 3: Set Startup Project

In Visual Studio:
1. Right-click on **GymSystem.UI** → **Set as Startup Project**
2. Ensure **GymSystem.UI** is bolded

### Step 4: Apply Database Migrations

Open **Package Manager Console** (Tools → NuGet Package Manager → Package Manager Console)

**Set Default project to:** `GymSystem.Infrastructure`

Then run:

```powershell
Update-Database
```

If you need to create a new migration (after model changes):

```powershell
Add-Migration "MigrationName"
Update-Database
```

### Step 5: Seed Initial Data

> ⚠️ **Important**: The database is seeded automatically when you run the application for the first time.

The seeder is configured in `GymSystem.Infrastructure/Seeders/` and registered in `Program.cs`. It will run at application startup and populate:
- **Categories** (Yoga, Boxing, CrossFit, GeneralFitness)
- **Plans** (Basic, Standard, Premium, Annual)

### Step 6: Run the Application

**In Visual Studio:** Press `F5` or click **Run**

The application will launch at:
- `http://localhost:5142`

## 📁 Project Structure Details

```
GymSystem/
├── GymSystem.Domain/           # Core Business Layer
│
├── GymSystem.Infrastructure/   # Data Access Layer
│   ├── Data/                   # DbContext
│   ├── Configuration/          # Entity configurations (Fluent API)
│   └── Entities/               # Plan, Member, Trainer, Session, etc.
│   ├── Interceptor/            # EF Core interceptors
│   ├── Migrations/             # Database migrations
│   ├── Repositories/           # Data access implementations
│   └── Seeders/                # Database seeders
│       ├── DatabaseSeeder.cs   # Main seeder orchestrator
│       ├── CategorySeeder.cs   # Session categories
│       └── PlanSeeder.cs       # Membership plans
│
└── GymSystem.UI/               # Presentation Layer
    ├── Controllers/            # MVC Controllers
    ├── Views/                  # Razor Views
    ├── wwwroot/                # Static files (CSS, JS, images)
    ├── appsettings.json        # Configuration
    └── Program.cs              # App entry point
```
