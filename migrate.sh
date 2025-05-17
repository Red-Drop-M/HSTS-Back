#!/bin/bash
# filepath: /home/hiki-zrx/Desktop/HSTS-Back/migrate.sh

COMMAND=$1
MIGRATION_NAME=$2
ROOT_DIR=$(dirname "$(realpath "$0")")
PROJ_PATH="$ROOT_DIR/src/HSTS-Back.csproj"

export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=HSTS;Username=postgres;Password=hikarosubahiko;"

clean() {
    echo "🧹 Cleaning project..."
    dotnet clean "$PROJ_PATH"
}

execute_ef() {
    dotnet ef $@ \
        --project "$PROJ_PATH" \
        --configuration Debug \
        --verbose
}

case "$COMMAND" in
  "add")
    if [ -z "$MIGRATION_NAME" ]; then
      echo "❌ Migration name required"
      echo "Usage: ./migrate.sh add <MigrationName>"
      exit 1
    fi

    echo "🔧 Restoring & building..."
    dotnet restore "$PROJ_PATH"
    dotnet build "$PROJ_PATH"
    echo "🔄 Creating migration '$MIGRATION_NAME'..."
    execute_ef migrations add "$MIGRATION_NAME" --output-dir "Infrastructure/Persistence/Migrations"
    ;;

  "update")
    echo "📦 Applying migrations..."
    execute_ef database update
    ;;

  "clean")
    clean
    ;;

  *)
    echo "❌ Unknown command: $COMMAND"
    echo "Usage: ./migrate.sh [add|update|clean]"
    exit 1
    ;;
esac