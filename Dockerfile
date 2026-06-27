# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution file
COPY Redot-Documentation.sln .

# Copy ALL .csproj files
COPY Redot-Documentation/Redot-Documentation.csproj ./Redot-Documentation/
COPY Redot-Documentation.Client/Redot-Documentation.Client.csproj ./Redot-Documentation.Client/
COPY Redot-Documentation-Tests/Redot-Documentation-Tests.csproj ./Redot-Documentation-Tests/

# Restore ALL projects
RUN dotnet restore

# Copy everything else
COPY . .

# Publish the main project
WORKDIR /src/Redot-Documentation
RUN dotnet publish -c Release -o /app/publish  -p:BlazorWebAssemblyEnableRuntime=true -p:BlazorWebAssemblyStaticWebAssets=true

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080

# Copy published output
COPY --from=build /app/publish .

# Run the application
ENTRYPOINT ["dotnet", "Redot-Documentation.dll"]