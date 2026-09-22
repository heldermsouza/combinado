# ADR-0002 — Take Blip como BSP WhatsApp, atrás de `IWhatsAppGateway`

- **Status**: Aceito
- **Data**: 2026-09-22
- **Decisores**: Helder
- **Bounded contexts afetados**: `chat`, `infra`

## Contexto

O produto tem o WhatsApp como interface primária. Precisamos de um Business Solution Provider (BSP) homologado pela Meta. O brief indicava Take Blip com Twilio como fallback e pedia confirmação na semana 1. Fatos verificados em 2026-09-22:

- Take Blip e Twilio são ambos BSPs homologados pela Meta para o Brasil.
- **Preço da Meta é por mensagem** (modelo de conversa foi extinto em 2025). Mensagens *service* (iniciadas pelo usuário, janela de 24h) são **gratuitas**. *Utility* custa ≈ R$0,15–0,19; *marketing* ≈ R$0,31–0,38. Desde 07/2026, WABAs em BRL são faturadas pela Facebook Brasil.
- Tráfego do Combinado é majoritariamente iniciado pelo usuário (registro de gasto, `/posso`, `/resumo`). Custo Meta tende a zero. O custo real é (a) taxa de plataforma do BSP e (b) mensagens proativas: notificação do parceiro (RF08), alertas 80/100/120% (RF09), resumo de sexta (RF07) e e-mails/mensagens de trial D-3/D-1. Todas devem ser templates **utility**, nunca marketing.
- **Take Blip**: empresa brasileira, nota fiscal em BRL, suporte em PT-BR, markup maior (~30% sobre mensagens + plataforma), stack orientada ao Builder. HTTP API + webhook existem. Homologação intermediada pelo time deles, 5–10 dias úteis.
- **Twilio**: markup ~10%, API e documentação superiores, sandbox imediato e embedded signup self-serve; cobra em USD (câmbio + IOF), suporte em inglês.
- **Latência**: ambos são proxies finos sobre a Cloud API da Meta. Diferença < 500 ms, irrelevante frente ao orçamento de 5 s dominado por LLM + Whisper.

## Decisão

**Take Blip em produção.** Motivos: faturamento em BRL com NF, suporte local, previsibilidade fiscal para uma empresa brasileira em fase inicial.

Mitigação de lock-in: toda integração passa por `IWhatsAppGateway` em `Combinado.Application`, com duas responsabilidades:

1. **Inbound**: normalizar o payload do webhook do BSP em `IncomingWhatsAppMessage` (id, remetente E.164, tipo texto/áudio, conteúdo ou URL de mídia, timestamp).
2. **Outbound**: `SendTextAsync`, `SendTemplateAsync` com retry Polly.

O adapter `TakeBlipGateway` vive em `Combinado.Infrastructure/WhatsApp/`. Um segundo adapter (Twilio ou Meta Cloud API direta) deve ser implementável sem tocar em `Application`.

Desenvolvimento local da Fase 2 roda contra **Twilio Sandbox** ou **número de teste da Meta Cloud API** desde o dia 1, para que a homologação do Blip não bloqueie o core bot.

## Alternativas consideradas

| Alternativa | Por que não |
|---|---|
| Twilio em produção | Faturamento em USD + IOF e suporte em inglês pesam para uma empresa BR pequena. Fica como fallback real via adapter. |
| Meta Cloud API direta | Sem markup e faturada em BRL, mas exige verificação de negócio e gestão de WABA por conta própria; sem suporte humano. Candidata natural para v2 quando o volume justificar. |

## Consequências

- **Positivas**: decisão de produto fecha; BSP vira detalhe de infraestrutura trocável; dev não espera homologação.
- **Negativas / dívida assumida**: markup maior do Blip; dois formatos de webhook para testar (sandbox de dev ≠ prod). Mitigação: testes de contrato por adapter com payloads gravados.
- **Critério de revisão**: custo mensal do BSP > 15% do MRR, ou latência p95 do envio > 1 s medida no Grafana, ou incidente de disponibilidade > 4 h.

## Referências

- https://developers.facebook.com/documentation/business-messaging/whatsapp/pricing
- https://docs.blip.ai/
- https://www.messagecentral.com/blog/whatsapp-business-api-pricing-brazil
