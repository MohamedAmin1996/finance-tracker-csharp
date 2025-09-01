# Use official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set working directory inside container
WORKDIR /app

# Copy the project file and restore dependencies
COPY ./FinanceTrackerApp/FinanceTrackerApp.csproj ./FinanceTrackerApp/
RUN dotnet restore ./FinanceTrackerApp/FinanceTrackerApp.csproj

# Copy the entire project
COPY ./FinanceTrackerApp ./FinanceTrackerApp

# Set working directory to the project folder
WORKDIR /app/FinanceTrackerApp

# Build the project
RUN dotnet build FinanceTrackerApp.csproj -c Release

# Publish the project
RUN dotnet publish FinanceTrackerApp.csproj -c Release -o /app/publish

# Final image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS final
WORKDIR /app

# Copy published app
COPY --from=build /app/publish .

# Copy wait script and make it executable inside container
COPY wait-for-postgres.sh /app/wait-for-postgres.sh
RUN ["chmod", "+x", "/app/wait-for-postgres.sh"]

# Use wait script as entrypoint
ENTRYPOINT ["/app/wait-for-postgres.sh", "dotnet", "FinanceTrackerApp.dll"]

