# Combinado

> O CFO do casal mora no WhatsApp.

Copiloto financeiro compartilhado para casais brasileiros. Interface primária no WhatsApp, painel web de suporte.

O contrato de trabalho (stack, roadmap, regras de negócio, guardrails) está em [CLAUDE.md](CLAUDE.md). Decisões arquiteturais em [docs/adr/](docs/adr/).

## Rodar local

Pré-requisitos: .NET SDK 10, Node 24 + pnpm, Docker Desktop.

```bash
# 1. dependências (postgres, redis, rabbitmq, seq)
cp infra/.env.example infra/.env
docker compose -f infra/docker-compose.yml up -d

# 2. banco
dotnet ef database update --project src/Combinado.Infrastructure --startup-project src/Combinado.Api

# 3. backend
dotnet watch --project src/Combinado.Api          # http://localhost:8080/health

# 4. frontend
cd src/web && pnpm install && pnpm dev            # http://localhost:3000

# 5. tudo em containers (checkpoint)
docker compose -f infra/docker-compose.yml --profile app up -d --build
```

Portas de host: Postgres `5435`, Redis `6380`, RabbitMQ `5672`/`15672`, Seq `5341` (deslocadas para não colidir com outros projetos; ajuste em `infra/.env`).

Logs estruturados: Seq em http://localhost:5341. RabbitMQ management em http://localhost:15672 (combinado / combinado).

## Testes

```bash
dotnet test                       # unidade + integração (Testcontainers precisa do Docker rodando)
cd src/web && pnpm test
```

## Estrutura

```
src/Combinado.Api              HTTP (minimal APIs /api/v1, webhooks, /health)
src/Combinado.Application      use cases, MediatR, contratos
src/Combinado.Domain           entidades, aggregates, value objects, eventos
src/Combinado.Infrastructure   EF Core, Redis, Rebus/RabbitMQ, integrações
src/Combinado.WhatsAppWorker   consumidor da fila wa.incoming
src/Combinado.SchedulerWorker  crons (resumo sexta 20h, trial D-3/D-1, reset mensal)
src/web                        Next.js 16
tests/                         Domain, Application, Integration (Testcontainers), E2E (Playwright)
infra/                         docker-compose, terraform
docs/adr/                      architecture decision records
```

## Contribuindo

Branches `feat/*` ou `fix/*`, nunca direto na `main`. Conventional Commits com escopo por bounded context (`chat`, `budget`, `goals`, `billing`, `onboarding`, `infra`, `web`). PRs < 400 linhas. Definition of Done em [CLAUDE.md §9](CLAUDE.md).
