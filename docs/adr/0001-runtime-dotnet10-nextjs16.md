# ADR-0001 — Adotar .NET 10 LTS e Next.js 16 no lugar de .NET 8 e Next.js 14

- **Status**: Aceito
- **Data**: 2026-09-22
- **Decisores**: Helder
- **Bounded contexts afetados**: `infra` (todos, transversal)

## Contexto

O brief original (CLAUDE.md, seção 2) fixava .NET 8 LTS, EF Core 8 e Next.js 14 App Router. Verificado em 2026-09-22:

- .NET 8 e .NET 9 perdem suporte em **10/11/2026**. O roadmap de 13 semanas começa em 22/09/2026; o EOL cai na semana 7, antes do lançamento público (semanas 12–13). Lançaríamos sobre runtime sem patches de segurança.
- .NET 10 é LTS com suporte até **novembro de 2028**. EF Core 10 é estável; `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.3 disponível.
- Next.js 14 está EOL desde **26/10/2025**. Next.js 15 expira em 21/10/2026. Next.js 16 (React 19.2, Tailwind v4) é a linha corrente e a única suportada durante todo o roadmap. shadcn/ui atual gera componentes para React 19 + Tailwind v4.
- A máquina de desenvolvimento só tem o SDK .NET 10.0.301 instalado.

## Decisão

Backend em **.NET 10** (`net10.0`, C# 14), **EF Core 10** com Npgsql 10.x. Frontend em **Next.js 16** App Router, React 19, Tailwind CSS v4, shadcn/ui corrente.

`global.json` pina SDK `10.0.100` com `rollForward: latestFeature`. Todas as versões de pacote ficam centralizadas em `Directory.Packages.props`.

## Alternativas consideradas

| Alternativa | Por que não |
|---|---|
| Manter .NET 8 / Next 14 como escrito | Lançar em runtime EOL; upgrade forçado na semana 14 com produto em produção. Instalar SDK 8 e pinar Tailwind v3 gera atrito no scaffold do shadcn. |
| .NET 10 no backend, Next 14 no frontend | Resolve metade. Frontend continua EOL e sem CVE patches. |
| .NET 9 | STS, mesmo EOL de 10/11/2026. |

## Consequências

- **Positivas**: janela de suporte cobre todo o MVP e os dois anos seguintes; sem upgrade de runtime na fase de tração.
- **Negativas / dívida assumida**: material de referência (tutoriais, respostas) para EF Core 8 / Next 14 é mais abundante; alguns pacotes de terceiros podem atrasar suporte a .NET 10. Mitigação: Central Package Management + Dependabot semanal.
- **Critério de revisão**: anúncio de EOL antecipado ou incompatibilidade bloqueante de pacote crítico (Npgsql, Rebus, Sentry).

## Referências

- https://devblogs.microsoft.com/dotnet/dotnet-8-9-end-of-support/
- https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core
- https://www.npgsql.org/efcore/release-notes/10.0.html
- https://endoflife.date/nextjs
- https://nextjs.org/blog/next-16
