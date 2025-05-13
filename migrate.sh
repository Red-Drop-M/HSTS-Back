#!/bin/bash
# Usage: 
# ./migrate.sh add MigrationName - Add a new migration
# ./migrate.sh update - Update database without creating migration
# ./migrate.sh snapshot - Create initial snapshot of existing database
# ./migrate.sh mark-applied MigrationName - Mark migration as applied without running it

# Get the command and migration name
COMMAND=$1
MIGRATION_NAME=$2

# Navigate to the src directory
cd "$(dirname "$0")/src"

# Set connection string as environment variable (EF Core uses this)
export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=HSTS;Username=postgres;Password=hikarosubahiko;"

case "$COMMAND" in
  "add")
    if [ -z "$MIGRATION_NAME" ]; then
      echo "Migration name required"
      echo "Usage: ./migrate.sh add MigrationName"
      exit 1
    fi
    # Create migration
    dotnet ef migrations add "$MIGRATION_NAME" \
      --output-dir Infrastructure/Persistence/Migrations \
      --msbuildprojectextensionspath /home/hiki-zrx/Desktop/HSTS-Back/obj/
    
    echo "Migration '$MIGRATION_NAME' created in Infrastructure/Persistence/Migrations folder"
    ;;
    
  "update")
    # Apply migrations
    dotnet ef database update \
      --msbuildprojectextensionspath /home/hiki-zrx/Desktop/HSTS-Back/obj/
    
    echo "Database updated with all pending migrations"
    ;;
    
  "snapshot")
    echo "Creating initial migration snapshot..."
    
    # Create a snapshot of the existing database  
    dotnet ef migrations add InitialCreate \
      --output-dir Migrations \
      --msbuildprojectextensionspath /home/hiki-zrx/Desktop/HSTS-Back/obj/
    
    # Find the actual migration ID
    MIGRATION_ID=$(find . -name "*_InitialCreate.cs" | head -1 | sed 's/.*\/\([0-9]\{14\}_InitialCreate\).cs/\1/')
    
    if [ -z "$MIGRATION_ID" ]; then
      echo "Error: Failed to find migration ID. Migration may not have been created."
      exit 1
    fi
    
    echo "Found migration ID: $MIGRATION_ID"
    
    # Insert migration record without making changes
    echo "Recording migration in database..."
    PGPASSWORD=hikarosubahiko psql -h localhost -U postgres -d HSTS -c "INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ('$MIGRATION_ID', '9.0.4');"
    
    echo "Created snapshot of existing database structure with migration ID: $MIGRATION_ID"
    ;;
    
  "mark-applied")
    if [ -z "$MIGRATION_NAME" ]; then
      echo "Migration name required"
      echo "Usage: ./migrate.sh mark-applied MigrationName"
      exit 1
    fi
    
    echo "Marking migration '$MIGRATION_NAME' as applied..."
    PGPASSWORD=hikarosubahiko psql -h localhost -U postgres -d HSTS -c "INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ('$MIGRATION_NAME', '9.0.4');"
    
    if [ $? -eq 0 ]; then
      echo "Migration '$MIGRATION_NAME' marked as applied without executing it"
    else
      echo "Error: Failed to mark migration as applied"
      exit 1
    fi
    ;;
    
  "list-migrations")
    echo "Listing available migrations:"
    dotnet ef migrations list \
      --msbuildprojectextensionspath /home/hiki-zrx/Desktop/HSTS-Back/obj/
    ;;
    
  *)
    echo "Unknown command: $COMMAND"
    echo "Usage: ./migrate.sh <command> [arguments]"
    echo ""
    echo "Available commands:"
    echo "  add <name>        - Add a new migration"
    echo "  update            - Update database with all pending migrations"
    echo "  snapshot          - Create initial snapshot of existing database"
    echo "  mark-applied <id> - Mark migration as applied without running it"
    echo "  list-migrations   - List all available migrations"
    exit 1
    ;;
esac