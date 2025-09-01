#!/bin/sh
# wait-for-postgres.sh

set -e

host="$DB_HOST"
port="$DB_PORT"

echo "Waiting for Postgres at $host:$port..."

until nc -z "$host" "$port"; do
  >&2 echo "Postgres is unavailable - sleeping"
  sleep 1
done

echo "Postgres is up - executing command"
exec "$@"
