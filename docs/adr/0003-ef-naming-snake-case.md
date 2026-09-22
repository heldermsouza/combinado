# ADR-0003 — Nomes snake_case no PostgreSQL via EFCore.NamingConventions

- **Status**: Aceito
- **Data**: 2026-09-22
- **Decisores**: Helder
- **Bounded contexts afetados**: `infra` (persistência de todos os contextos)

## Contexto

EF Core gera identificadores PascalCase (`"Transacoes"."CaixaId"`). No PostgreSQL isso obriga aspas em toda query manual, quebra ferramentas que assumem lower-case e destoa da convenção do ecossistema. O brief não fixa convenção de nomes no banco, mas fixa PostgreSQL e proíbe dependência nova sem ADR.

## Decisão

Usar o pacote `EFCore.NamingConventions` (MIT, mantido pelo autor do Npgsql) com `UseSnakeCaseNamingConvention()` em `PostgresOptions.Configure`, aplicado tanto em runtime quanto em design-time para que migrations e modelo coincidam.

Resultado: `Transacao.CaixaId` → tabela `transacao`, coluna `caixa_id`; índices e FKs seguem a mesma regra. A tabela de histórico mantém o nome `__EFMigrationsHistory`, mas suas colunas viram `migration_id` e `product_version` (verificado na migration inicial).

## Alternativas consideradas

| Alternativa | Por que não |
|---|---|
| PascalCase padrão do EF | Aspas obrigatórias em todo SQL manual; atrito em psql, Grafana, exports LGPD. |
| Nomear manualmente em cada `IEntityTypeConfiguration` | Repetitivo, fácil esquecer, sem ganho. |

## Consequências

- **Positivas**: SQL legível sem aspas; alinhado com convenção PostgreSQL.
- **Negativas / dívida assumida**: mais uma dependência a acompanhar por versão major do EF Core (histórico: acompanha em dias).
- **Critério de revisão**: pacote sem release compatível 30 dias após um EF Core major.

## Referências

- https://github.com/efcore/EFCore.NamingConventions
