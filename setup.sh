#!/usr/bin/env bash
# =============================================================================
# Setup inicial — Centro de Treinamento Pokémon "Alto Nível"
#
# Sobe o SQL Server e a API via Docker Compose, aplica o schema.sql e o
# seed.sql (dados de teste). Rode este script uma vez depois de clonar o
# repositório. Idempotente: pode rodar de novo sem duplicar dados.
#
# Uso: bash setup.sh   (ou ./setup.sh, se estiver com permissão de execução)
# =============================================================================
set -euo pipefail

# Evita que o Git Bash / MSYS no Windows converta paths tipo "/tmp/x.sql"
# em caminhos do Windows antes de repassar pro "docker exec". Não afeta
# Linux/macOS (a variável é ignorada nesses sistemas).
export MSYS_NO_PATHCONV=1

SA_PASSWORD="YourStrong!Passw0rd"
DB_CONTAINER="pkm-sqlserver"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "==> Checando dependências (docker + docker compose)..."
if ! command -v docker >/dev/null 2>&1; then
  echo "Docker não encontrado no PATH. Instale o Docker Desktop antes de continuar."
  exit 1
fi
if ! docker compose version >/dev/null 2>&1; then
  echo "Docker Compose (plugin 'docker compose') não encontrado. Atualize o Docker Desktop."
  exit 1
fi

echo "==> Subindo SQL Server e API (docker compose up -d --build sqlserver api)..."
(cd "$SCRIPT_DIR" && docker compose up -d --build sqlserver api)

echo "==> Aguardando o SQL Server aceitar conexões..."
READY=0
for _ in $(seq 1 30); do
  if docker exec "$DB_CONTAINER" /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -C -Q "SELECT 1" >/dev/null 2>&1; then
    READY=1
    break
  fi
  sleep 3
done
if [ "$READY" -ne 1 ]; then
  echo "SQL Server não respondeu a tempo. Verifique 'docker logs $DB_CONTAINER'."
  exit 1
fi
echo "    SQL Server pronto."

DB_EXISTS="$(docker exec "$DB_CONTAINER" /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -C -h -1 -Q "SET NOCOUNT ON; SELECT CASE WHEN DB_ID('PokemonTrainingCenter') IS NULL THEN 0 ELSE 1 END" | tr -d '[:space:]')"

if [ "$DB_EXISTS" = "1" ]; then
  echo "==> Banco PokemonTrainingCenter já existe — pulando schema.sql (ele não é reexecutável)."
else
  echo "==> Aplicando schema.sql..."
  docker exec -i "$DB_CONTAINER" /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -C -b < "$SCRIPT_DIR/db/schema.sql"
fi

echo "==> Aplicando seed.sql (10 Treinadores, 10 Pokémon, 10 Matrículas de exemplo)..."
docker exec -i "$DB_CONTAINER" /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -C -b < "$SCRIPT_DIR/db/seed.sql"

echo ""
echo "=============================================================================="
echo " Setup concluído!"
echo ""
echo " API + Swagger: http://localhost:5000/swagger"
echo ""
echo " Para rodar o frontend (em outro terminal):"
echo "   cd web && npm install && npm start"
echo "   Depois acesse http://localhost:4200"
echo "=============================================================================="
