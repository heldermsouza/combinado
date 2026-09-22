# ADR-0004 — Rebus como biblioteca de mensageria sobre RabbitMQ

- **Status**: Aceito
- **Data**: 2026-09-22
- **Decisores**: Helder
- **Bounded contexts afetados**: `chat`, `budget`, `goals`, `billing`, `infra`

## Contexto

O brief fixa RabbitMQ 3.13 para eventos entre serviços (gasto registrado, alerta disparado, resumo pronto), mas não fixa a biblioteca cliente. As opções em .NET eram MassTransit, Rebus, Wolverine/NServiceBus (comerciais) ou `RabbitMQ.Client` puro. Fatos verificados em 2026-09-22:

- **MassTransit v8** (Apache 2.0) recebe apenas patches de segurança até **dezembro de 2026**. Depois disso, sem manutenção.
- **MassTransit v9** (lançado Q1/2026) é comercial: US$ 400/mês ou US$ 4.000/ano para PMEs. Empresas com receita < US$ 1M "*podem* qualificar" para licença gratuita; a linguagem é discricionária e não serve como compromisso para planejamento.
- **Rebus** é MIT, mantido ativamente (core 8.x, `Rebus.RabbitMq` 10.x, `Rebus.ServiceProvider` 10.x), com transporte RabbitMQ, retries em dois níveis, fila de erro, sagas, outbox e integração com DI e Serilog.
- O Combinado é um monolito modular com meia dúzia de domain events e três hosts (Api, WhatsAppWorker, SchedulerWorker). Não precisa de saga orchestration complexa nem de suporte multi-broker.

Helder inicialmente indicou MassTransit; após os fatos acima, escolheu Rebus.

## Decisão

**Rebus** com transporte RabbitMQ, configurado em `Combinado.Infrastructure/Messaging/RebusConfiguration.cs`:

- Api é **one-way client** (só publica). Workers têm **fila de entrada** própria (`wa.incoming` no WhatsAppWorker; SchedulerWorker define a sua quando tiver consumidores).
- Retry simples com **5 tentativas**, depois fila de erro única `combinado.error`. Reprocessamento manual via RabbitMQ management até que exista tooling.
- Serialização padrão do Rebus 8 (System.Text.Json). Contratos de mensagem vivem em `Combinado.Application/Contracts/Messages/`.
- Logging via `Rebus.Serilog` para que o bus apareça no Seq com as mesmas propriedades.

## Alternativas consideradas

| Alternativa | Por que não |
|---|---|
| MassTransit v8 | EOL em 3 meses; mesma armadilha que .NET 8 (ADR-0001). |
| MassTransit v9 comercial | Free tier discricionário; US$ 4k/ano é ~20% do MRR alvo do MVP. |
| `RabbitMQ.Client` puro | Publisher, consumer, retry, DLX e correlação escritos e testados por nós. Mais código, sem ganho no MVP. |
| Wolverine / NServiceBus | Comerciais ou com modelo de suporte pago; não justificam para este volume. |

## Consequências

- **Positivas**: zero risco de licença; API pequena; retries e fila de erro prontos; um único pacote extra por transporte.
- **Negativas / dívida assumida**: comunidade menor que MassTransit; menos material de referência. Outbox transacional com EF Core precisa ser validado na Fase 3 (RF08/RF09) antes de assumir entrega exatamente-uma-vez.
- **Critério de revisão**: se o outbox EF Core do Rebus não atender RF08 (notificação ≤ 30 s com garantia de entrega) ou se surgir necessidade real de sagas multi-etapa, reabrir esta decisão.

## Referências

- https://github.com/rebus-org/Rebus
- https://masstransit.io/introduction/v9-announcement
- https://antondevtips.com/blog/masstransit-rabbitmq-and-azure-service-bus-is-it-worth-a-commercial-license
