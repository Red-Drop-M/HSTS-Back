# -------------------------
# Stage 1: Build & Publish
# -------------------------
    FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
    WORKDIR /src
    
    # Copy solution and props (omit global.json)
    COPY ["HSTS-Back.sln", "Directory.Build.props", "./"]
    
    # Copy project file and restore under .NET 8.0
    COPY ["src/HSTS-Back.csproj", "src/"]
    RUN dotnet restore "src/HSTS-Back.csproj"
    
    # Copy all sources, then strip global.json to avoid SDK mismatch
    COPY . .
    RUN rm -f /src/global.json
    
    # Publish the app
    WORKDIR "/src/src"
    RUN dotnet publish -c Release -o /app/publish --no-restore
    
    # -------------------------
    # Stage 2: Development (hot-reload)
    # -------------------------
    FROM mcr.microsoft.com/dotnet/sdk:8.0 AS dev
    WORKDIR /app
    
    # Copy solution and props for restore
    COPY ["HSTS-Back.sln", "Directory.Build.props", "./"]
    # Copy project file and restore
    COPY ["src/HSTS-Back.csproj", "src/"]
    RUN dotnet restore "src/HSTS-Back.csproj"
    
    # Copy all source code
    COPY . .
    RUN rm -f /app/global.json  # Remove conflicting global.json
    
    # Set working directory to project
    WORKDIR /app/src
    
    # Dev environment settings
    ENV ASPNETCORE_ENVIRONMENT=Development \
        DOTNET_USE_POLLING_FILE_WATCHER=1 \
        ASPNETCORE_URLS=http://0.0.0.0:5000
    
    # Expose development ports
    EXPOSE 5000 5001
    
    # Launch with watch
    ENTRYPOINT ["dotnet", "watch", "run", "--urls", "http://0.0.0.0:5000"]
    
    # -------------------------
    # Stage 3: Production Runtime
    # -------------------------
    FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
    WORKDIR /app
    
    # Copy published output
    COPY --from=build /app/publish .
    
    # Listen on all interfaces
    ENV ASPNETCORE_URLS=http://0.0.0.0:5000
    
    # Expose runtime port
    EXPOSE 5000
    
    # Launch the app
    ENTRYPOINT ["dotnet", "HSTS-Back.dll"]