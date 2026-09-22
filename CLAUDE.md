# Combinado — Development Brief

Este arquivo é o **contrato de trabalho** entre eu (Helder, founder/tech lead) e o Claude Code. Toda sessão começa lendo isto. Se algo aqui está desatualizado, atualize antes de codar.

> **Decisões que substituem partes deste brief** vivem em `docs/adr/`. Em conflito, o ADR mais recente vence. Índice:
> - ADR-0001 — .NET 10 / EF Core 10 / Next.js 16 (substitui .NET 8 / Next 14)
> - ADR-0002 — Take Blip como BSP, atrás de `IWhatsAppGateway`
> - ADR-0003 — EF Core com snake_case (EFCore.NamingConventions)
> - ADR-0004 — Rebus no lugar de MassTransit para RabbitMQ

---

## 1. O que é o Combinado

Copiloto financeiro compartilhado para casais brasileiros, com interface primária no WhatsApp e painel web de suporte. Casal registra gastos por texto/áudio, recebe alertas em tempo real, e pode perguntar "posso gastar 300 num tênis?" — resposta em <5s considerando o orçamento e metas ativos do casal.

**One-liner**: "O CFO do casal mora no WhatsApp."

**ICP**: casais 26–40 anos, renda combinada R$ 8k–25k, ambos CLT/PJ, urbanos, já tentaram Mobills/planilha e abandonaram.

**Modelo**: freemium 30 dias sem cartão → R$ 29,90/mês casal (ou R$ 299/ano).

**Meta MVP**: 300 casais em trial em 60d pós-lançamento, ≥18% conversão pago, MRR R$ 1.600+, D30 ≥55%.

---

## 2. Stack — DECIDIDO, não repropor

### Backend
- **.NET 10** (LTS até nov/2028), C# 14, minimal APIs onde couber — ADR-0001
- **PostgreSQL 16** (Azure Database for PostgreSQL Flexible Server em prod; Docker local)
- **Redis 7** (cache + rate limit + cache de conversas curtas)
- **RabbitMQ 3.13** via **Rebus** (eventos entre serviços: gasto registrado, alerta disparado, resumo pronto) — ADR-0004
- **EF Core 10** + Npgsql para persistência, snake_case — ADR-0003
- **MediatR 13+** para CQRS interno (licença Community, key em user-secrets `MediatR:LicenseKey`)
- **FluentValidation** para validação de comandos
- **Serilog** → **Seq** (dev) / **Application Insights** (prod)
- **Polly** (via `Microsoft.Extensions.Http.Resilience`) para retry/circuit breaker em integrações externas

### Frontend
- **Next.js 16 App Router**, React 19, TypeScript strict — ADR-0001
- **Tailwind CSS v4** + **shadcn/ui**
- **React Query** para estado servidor
- **Zod** para schemas compartilhados
- **NextAuth** (Auth.js) com credentials + email verification

### Integrações
- **WhatsApp Business API** via **Take Blip** (fallback Twilio), atrás de `IWhatsAppGateway` — ADR-0002
- **Anthropic Claude Haiku** para categorização (fallback OpenAI GPT-4o-mini) — API direta
- **OpenAI Whisper API** para transcrição de áudio
- **Pagar.me** para assinatura recorrente (checkout transparente, salvar cartão)
- **Resend** para e-mail transacional

### Infra
- **Docker Compose** para dev local (postgres, redis, rabbitmq, seq)
- **GitHub Actions** para CI/CD
- **Azure App Service** (Linux, container) para backend
- **Azure Static Web Apps** ou **Vercel** para frontend
- **Terraform** para infra provisioning (opcional na fase MVP, obrigatório antes de escalar)

### Observabilidade
- **Sentry** (backend + frontend)
- **Grafana Cloud** (métricas custom: msg/casal, latência LLM, custo variável)
- **Uptime Robot** para healthchecks públicos

---

## 3. Estrutura do monorepo

```
combinado/
├── src/
│   ├── Combinado.Api/              # HTTP entry point, controllers minimal API
│   ├── Combinado.Application/      # Use cases, MediatR handlers, DTOs
│   ├── Combinado.Domain/           # Entidades, aggregates, value objects, domain events
│   ├── Combinado.Infrastructure/   # EF Core, Redis, Rabbit, integrações externas
│   ├── Combinado.WhatsAppWorker/   # Service worker que consome fila de mensagens WA
│   ├── Combinado.SchedulerWorker/  # Cron: resumo sexta 20h, D-3 D-1 trial, reset mensal
│   └── web/                        # Next.js frontend
├── tests/
│   ├── Combinado.Domain.Tests/
│   ├── Combinado.Application.Tests/
│   ├── Combinado.Integration.Tests/  # com Testcontainers pra postgres/redis
│   └── Combinado.E2E.Tests/          # Playwright do fluxo web completo
├── infra/
│   ├── docker-compose.yml
│   ├── docker-compose.override.yml   # overrides locais, gitignored
│   └── terraform/                    # opcional na fase MVP
├── docs/
│   ├── adr/                          # architecture decision records
│   ├── api/                          # OpenAPI spec gerada
│   └── runbook.md
├── .github/workflows/
├── Directory.Build.props             # TFM, nullable, warnings-as-errors
├── Directory.Packages.props          # Central Package Management
├── global.json                       # SDK pinado
├── CLAUDE.md                         # este arquivo
├── README.md
└── Combinado.slnx                    # formato XML de solution (padrão do SDK 10)
```

---

## 4. Convenções de código

### Nomenclatura
- **Português** para entidades de domínio que refletem termos do produto: `Caixa`, `Parceiro`, `Transacao`, `Cofrinho`, `Orcamento`, `Categoria`, `Alerta`
- **Inglês** para código de infraestrutura, use cases, e tudo que não é vocabulário do produto: `TransacaoRepository`, `RegisterTransactionCommand`, `SendWhatsAppMessage`
- Isso reduz atrito no code review — o negócio fica óbvio, a infra fica genérica

### Padrões
- **Aggregate roots** com métodos que garantem invariantes; setters privados
- **Domain events** para tudo que dispara side effect (alerta, notificação parceiro)
- **Value objects** para dinheiro (`Dinheiro` com moeda + valor decimal), datas de ciclo (`CicloMensal`), IDs fortes (`CaixaId`, `TransacaoId`)
- Sem exceções pra fluxo de controle — use `Result<T>` pattern (`Combinado.Domain/Common/Result.cs`)
- **Async all the way**, sem `.Result` ou `.Wait()`
- **Nullable reference types ligado**, warnings como erro
- **API versionada** via URL: `/api/v1/...`
- Todo endpoint tem OpenAPI schema; contratos em `Combinado.Application/Contracts/`

### Commits
- Conventional Commits: `feat(chat): registrar gasto por texto`, `fix(webhook): idempotência de mensagens duplicadas`, `chore(deps): bump ef core 10.0.13`
- Uma mensagem = uma unidade lógica; PRs pequenos (<400 linhas)
- Escopo do commit mapeia bounded context: `chat`, `budget`, `goals`, `billing`, `onboarding`, `infra`, `web`

### Testes
- Toda regra de negócio (RN01–RN12) tem teste de unidade no domínio
- Todo endpoint HTTP tem teste de integração cobrindo happy path + 2–3 erros esperados
- Fluxos críticos de UX têm E2E Playwright: onboarding completo, registrar gasto, `/posso`, paywall
- Coverage não é meta, comportamento coberto é
- **Testcontainers** para postgres/redis/rabbitmq nos testes de integração (um conjunto de containers por assembly, Respawn entre testes)

---

## 5. Roadmap de entrega — 13 semanas

Cada fase termina com **checkpoint executável**: você deve ser capaz de mostrar funcionando pra um usuário real.

### Fase 0 — Fundação (semanas 1–2)
**Checkpoint**: `docker-compose up` sobe backend + frontend + postgres + redis + rabbit. Health check em `/health` retorna 200. Migrations rodam. Frontend em `localhost:3000` mostra landing.

- [ ] Solution + estrutura de projetos criada
- [ ] docker-compose com postgres, redis, rabbit, seq
- [ ] EF Core configurado, migration inicial vazia
- [ ] Health check endpoint (`Combinado.Api/health`)
- [ ] GitHub Actions: build + test + lint em cada PR
- [ ] Next.js app scaffold + Tailwind + shadcn/ui base
- [ ] Sentry conectado em backend e frontend
- [ ] **Homologação BSP WhatsApp iniciada** (bloqueante — 5–10 dias úteis)

### Fase 1 — Cadastro e vínculo (semanas 3–4)
**Checkpoint**: Casal se cadastra, aceita termos LGPD, convida parceiro por número; parceiro responde "SIM COMBINADO" no bot e é vinculado; ambos veem o mesmo painel vazio.

- [ ] RF01 — cadastro/login web com email verification
- [ ] RF12 — termos e consentimento LGPD versionados
- [ ] RF02 — vínculo do parceiro por WhatsApp
- [ ] RF03 — wizard de setup de orçamento
- [ ] Webhook WhatsApp em produção (staging BSP)

### Fase 2 — Core bot (semanas 5–7) — **MAIOR RISCO TÉCNICO**
**Checkpoint**: Gasto por texto e áudio, categorização por IA com fallback, simulador `/posso`. Latência p95 <5s.

- [ ] Integração WhatsApp Business (webhook receive + send)
- [ ] RF04 — registro por texto com Claude Haiku
- [ ] RN12 — idempotência (deduplicação por hash 60s)
- [ ] RF05 — registro por áudio (Whisper)
- [ ] RN03 — confirmação de baixa confiança (<70%)
- [ ] RF06 — simulador `/posso` (com cache Redis 60s pra saldos)

### Fase 3 — Retenção (semanas 8–9)
**Checkpoint**: Sexta 20h chega resumo semanal; alertas 80% funcionam; painel web mostra tudo.

- [ ] RF07 — comando `/resumo` + cron sexta 20h
- [ ] RF08 — notificação de gasto do parceiro (assíncrono via Rabbit)
- [ ] RF09 — alerta 80/100/120% com idempotência mensal
- [ ] RF10 — dashboard web (gráfico + lista + editar)

### Fase 4 — Monetização (semanas 10–11)
**Checkpoint**: Trial termina após 30 dias do 1º gasto; Pagar.me cobra; cartão recusado dispara sequência.

- [ ] RF11 — Pagar.me integrado (checkout transparente)
- [ ] RN06/RN07 — controle de trial e bloqueio pós-trial
- [ ] E-mails transacionais: boas-vindas, D-3, D-1, cartão recusado
- [ ] Fluxo de reativação: 3 tentativas em 7 dias

### Fase 5 — Beta e lançamento (semanas 12–13)
**Checkpoint**: 20 casais reais usando; NPS ≥40; lançamento público na landing.

- [ ] E2E Playwright dos fluxos críticos
- [ ] Beta fechado — 20 casais recrutados manualmente
- [ ] Landing page + PostHog/Mixpanel
- [ ] Runbook de incidentes documentado
- [ ] Ajustes de UX e categorização baseados em dados reais
- [ ] Lançamento público

---

## 6. Domain model — entidades centrais

```
Caixa (aggregate root)
├── Id: CaixaId
├── Parceiros: [Parceiro] (exatamente 2 no MVP)
├── Categorias: [Categoria]
├── RendaMensal: Dinheiro
├── TrialIniciadoEm: DateTime?
├── PlanoAtivo: bool
├── Timezone: string (default "America/Sao_Paulo")
└── Métodos: VincularParceiro, IniciarTrial, AtivarPlano

Parceiro (entity)
├── Id: ParceiroId
├── Nome: string
├── NumeroWhatsApp: NumeroWhatsApp (VO com validação DDI+DDD)
├── EmailPrimario: bool
└── PrefsNotificacao: PrefsNotificacao

Transacao (aggregate root)
├── Id: TransacaoId
├── CaixaId: CaixaId
├── AutorId: ParceiroId
├── Valor: Dinheiro
├── Descricao: string (max 60)
├── CategoriaId: CategoriaId
├── DataGasto: DateTime
├── CriadoEm: DateTime
├── Origem: OrigemTransacao (WhatsApp | AudioWhatsApp | Web)
├── HashIdempotencia: string (autor + valor + descricao + minuto)
└── Métodos: Editar (até 30d), Excluir (soft delete)

Orcamento (aggregate root, per mês)
├── Id: OrcamentoId
├── CaixaId: CaixaId
├── Mes: CicloMensal (YYYY-MM)
├── LimitesPorCategoria: Dictionary<CategoriaId, Dinheiro>
└── Métodos: AtualizarLimite, TotalOrcado

Cofrinho (aggregate root)
├── Id: CofrinhoId
├── CaixaId: CaixaId
├── Nome: string
├── ValorAlvo: Dinheiro
├── ValorAtual: Dinheiro
├── AporteMensalPrevisto: Dinheiro
├── PrazoAlvo: DateTime?
└── Métodos: Aportar, Retirar, Concluir

Alerta (entity, filha de Orcamento)
├── Id: AlertaId
├── OrcamentoId: OrcamentoId
├── CategoriaId: CategoriaId
├── Threshold: decimal (0.8, 1.0, 1.2)
├── DisparadoEm: DateTime
```

**Value objects**: `Dinheiro`, `CicloMensal`, `NumeroWhatsApp`, `HashIdempotencia`, todos os `*Id` (strong typed).

**Domain events**:
- `TransacaoRegistrada` → dispara notificação parceiro + reavaliação de alertas
- `TransacaoEditada` / `TransacaoExcluida` → dispara reavaliação de alertas
- `AlertaDisparado` → envia mensagem WhatsApp
- `CofrinhoConcluido` → mensagem de parabéns
- `TrialIniciado` / `TrialExpirado` / `PlanoAtivado` → e-mail + mensagem

---

## 7. Contratos de integração críticos

### WhatsApp Business API (Take Blip)
- **Webhook receive**: `POST /api/v1/webhooks/whatsapp` — valida assinatura HMAC, publica na fila `wa.incoming`
- **Consumer** processa mensagem, chama LLM, persiste, publica resposta na fila `wa.outbound`
- **Sender worker** consome `wa.outbound` e chama API Take Blip com retry Polly
- **Idempotência**: `message_id` da WA armazenado 24h em Redis; segundo hit é ignorado
- **Rate limit interno**: 200 msgs/mês por número no plano base (contador em Redis, reset dia 1)

### Anthropic Claude (categorização)
- Prompt em `Combinado.Infrastructure/LLM/Prompts/CategorizeExpense.txt` versionado
- Response schema Zod compartilhado backend/frontend
- Timeout 12s, cache Redis 5min por hash da mensagem
- Fallback: se Anthropic falhar 2x seguidas, cai pra OpenAI GPT-4o-mini
- Fallback final: parser regex do MVP (mantido em `Combinado.Application/Parsers/`)

### Pagar.me
- Checkout transparente no frontend, tokeniza cartão
- Backend cria assinatura via API server-to-server
- Webhook `POST /api/v1/webhooks/pagarme` para eventos: `subscription.created`, `charge.paid`, `charge.refused`, `subscription.canceled`
- 3 tentativas de cobrança em 7 dias antes de bloquear

---

## 8. Regras de negócio críticas (do PRD)

Cada uma tem teste unitário nomeado `RN0X_*`:

- **RN01** — Vínculo: número WA pertence a exatamente 1 Caixa; rejeita duplicado
- **RN02** — Autoria: todo gasto tem autor, atribuído ao caixa compartilhado, sem "privado"
- **RN03** — Categorização: confiança <70% pede confirmação
- **RN04** — Ciclo: reset dia 1 às 00h no timezone do casal
- **RN05** — Edição até 30 dias; depois imutável
- **RN06** — Trial 30d a partir do **1º gasto**, não do cadastro
- **RN07** — Pós-trial: `/posso`, alertas e cofrinhos bloqueados; registro continua
- **RN08** — Cancelamento: dados exportáveis por 90d, depois excluídos (LGPD)
- **RN09** — Notificação parceiro em ≤30s (SLA soft); modo "não perturbe" agrupa em digest 3h
- **RN10** — `/posso` não registra; só após confirmação SIM
- **RN11** — Limite 200 msgs/mês no plano base
- **RN12** — Idempotência por hash {autor, valor, descrição, minuto}

---

## 9. Definition of Done por feature

Uma feature NÃO está pronta até:

1. Testes de unidade das RNs cobertos
2. Teste de integração do endpoint (happy + 2 erros)
3. Migration EF Core criada e revisada
4. OpenAPI atualizado (auto-gerado, mas revisar diff)
5. Logs estruturados nos pontos críticos (Serilog com propriedades)
6. Métricas customizadas se afeta cost/latency (contador em Grafana)
7. Frontend consome via React Query com loading + error states
8. Documentação inline em endpoints complexos
9. PR revisado por mim (Helder) — mesmo que seja auto-review de sanity
10. Deploy em staging + smoke test manual

---

## 10. Guardrails — o que NÃO fazer

- ❌ **Não usar** ORMs além de EF Core; sem Dapper misturado
- ❌ **Não criar** microservices no MVP — monolito modular com boas fronteiras já basta
- ❌ **Não implementar** Open Finance / integração bancária no MVP (v2)
- ❌ **Não construir** app nativo iOS/Android — WhatsApp + web resolve
- ❌ **Não guardar** senhas em plain text ou hash fraco (use BCrypt work factor ≥12)
- ❌ **Não enviar** dados sensíveis (transações, valores) para LLM sem sanitização
- ❌ **Não fazer** deploy sem migration testada em staging
- ❌ **Não permitir** que Claude Code envie mensagens em nome do Helder (WhatsApp, email) — só rascunhos no chat pra ele revisar e mandar
- ❌ **Não introduzir** dependências novas sem justificativa em ADR
- ❌ **Não commitar** secrets — usar dotnet user-secrets em dev, Azure Key Vault em prod
- ❌ **Não escrever** código com `TODO: implement later` sem issue trackada
- ❌ **Não otimizar** prematuramente — primeiro faça correto e legível, meça, depois otimize

---

## 11. Comandos úteis

```bash
# Dev
cp infra/.env.example infra/.env     # portas de host: postgres 5435, redis 6380 (evita colisão com outros projetos)
docker compose -f infra/docker-compose.yml up -d
dotnet ef database update --project src/Combinado.Infrastructure --startup-project src/Combinado.Api
dotnet watch --project src/Combinado.Api
cd src/web && pnpm dev

# Tudo em containers (checkpoint Fase 0)
docker compose -f infra/docker-compose.yml --profile app up -d --build

# Testes
dotnet test
cd src/web && pnpm test
cd tests/Combinado.E2E.Tests && pnpm playwright test

# Lint
dotnet format --verify-no-changes
cd src/web && pnpm lint

# Migrations
dotnet ef migrations add NomeDaMigracao --project src/Combinado.Infrastructure --startup-project src/Combinado.Api -o Persistence/Migrations

# Secrets de dev
dotnet user-secrets set "Sentry:Dsn" "<dsn>" --project src/Combinado.Api
dotnet user-secrets set "MediatR:LicenseKey" "<key>" --project src/Combinado.Api

# Logs locais
open http://localhost:5341   # Seq
```

---

## 12. Como o Claude Code deve trabalhar comigo

- Ao começar uma tarefa nova, **releia este arquivo** e a última issue/PR relevante
- **Sempre proponha o plano antes de codar** — 3-5 bullets do que vai fazer, arquivos afetados, testes previstos
- Se identificar ambiguidade no requisito, **pergunte** — não invente decisão de produto
- Sugira ADRs (`docs/adr/NNNN-titulo.md`) quando a decisão for irreversível ou tocar em vários bounded contexts
- Nunca abra PR direto na `main` — sempre branch `feat/*` ou `fix/*`
- Após implementar, **rode os testes localmente** antes de dizer que terminou
- **Reporte cobertura de RNs** quando entregar uma fase completa

---

## 13. Referências rápidas

- PRD completo: `docs/prd.md` (colar do que já tenho)
- MVP funcional de UX referência: `docs/mvp-ui-reference.html` (o artifact HTML que rodei antes)
- Decisões arquiteturais: `docs/adr/`
- Runbook de incidentes: `docs/runbook.md`
