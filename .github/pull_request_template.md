## O que muda

<!-- 2–5 bullets. Bounded context afetado no título (chat, budget, goals, billing, onboarding, infra, web). -->

## Como validei

<!-- Comandos rodados e saída relevante. Testes novos por nome. -->

## Definition of Done (CLAUDE.md §9)

- [ ] Testes de unidade das RNs cobertas (`RN0X_*`)
- [ ] Teste de integração do endpoint (happy path + 2 erros)
- [ ] Migration EF Core criada e revisada
- [ ] OpenAPI revisado (diff)
- [ ] Logs estruturados nos pontos críticos
- [ ] Métricas customizadas se afeta custo/latência
- [ ] Frontend via React Query com loading + error states
- [ ] Documentação inline em endpoints complexos
- [ ] Sem secrets, sem `TODO` sem issue, sem dependência nova sem ADR
- [ ] PR < 400 linhas (fora lockfiles, migrations geradas e scaffold)

## Fora de escopo / próximos passos

<!-- O que ficou de fora de propósito e onde está trackado. -->
