# Finance Tracker CLI

**Personal Finance Tracker** is a CLI application that helps users track their income and expenses. It is built with **C# (.NET 8)**, using **PostgreSQL** as the database. The project is fully **containerized with Docker**, includes **EF Core**, **unit tests**, and a **CI/CD pipeline** setup for GitHub Actions.

---

## Table of Contents

- [Features](#features)  
- [Tech Stack](#tech-stack)  
- [Installation](#installation)  
- [Usage](#usage)  
- [Docker](#docker)  

---

## Features

- Add and track **income** and **expenses**  
- View **all transactions** or filter by month/year  
- Generate **monthly summary** (income, expenses, net balance)  
- Update or delete transactions by ID  
- Export transactions to **CSV**  
- Seed demo data for testing  
- **CLI help** shows all available commands  
- **Migration-ready** with PostgreSQL  
- Fully **Dockerized** with Compose  

---

## Tech Stack

**Backend & CLI:**  

- C# (.NET 8) & Python 3.11  
- Entity Framework Core (C#) 
- PostgreSQL (Dockerized)  
- Docker & Docker Compose  
- GitHub Actions CI/CD  

**Libraries / Packages**  

- **C#:** `Microsoft.EntityFrameworkCore`, `System.CommandLine`, `Serilog`, `DotNetEnv`  

---

## Installation

**Clone the repository:**

```bash
git clone https://github.com/MohamedAmin1996/finance-tracker-csharp.git
cd finance-tracker-csharp
```
---

## Usage

**Run with Docker Compose:**

```bash
docker-compose build
docker-compose up -d
```

**Enter the FinanceTrackerApp container:**

```bash
docker exec -it finance_tracker_app bash
```

**C# CLI Commands:**

```bash
dotnet FinanceTrackerApp.dll add-income 1500 Salary --description "Test"
dotnet FinanceTrackerApp.dll list-all
dotnet FinanceTrackerApp.dll help
```

## Docker
- PostgreSQL database runs in a container
- The app container waits for Postgres to be healthy before starting
- Containers are networked via finance_network
- CLI can be executed via docker exec
