# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

# Copy csproj and restore dependencies
COPY ./FinanceTrackerApp/FinanceTrackerApp.csproj ./FinanceTrackerApp/
RUN dotnet restore ./FinanceTrackerApp/FinanceTrackerApp.csproj

# Copy entire project and build
COPY ./FinanceTrackerApp ./FinanceTrackerApp
WORKDIR /app/FinanceTrackerApp
RUN dotnet publish FinanceTrackerApp.csproj -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS final
WORKDIR /app

# Copy published output
RUN apt-get update && apt-get install -y bash && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
