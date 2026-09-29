# Combinado — web

Painel web do Combinado (Next.js 16 App Router, React 19, Tailwind v4, shadcn/ui, React Query, Zod).

```bash
cp .env.example .env.local
pnpm install
pnpm dev          # http://localhost:3000
pnpm lint
pnpm typecheck
pnpm build
```

Componentes shadcn: `pnpm dlx shadcn@latest add <componente>` (config em `components.json`, estilo `base-nova`, cor base `neutral`).

Variáveis públicas são validadas com Zod em `src/lib/env.ts`; adicionar novas lá antes de usar.

O contrato do projeto está em [../../CLAUDE.md](../../CLAUDE.md). O `AGENTS.md` deste diretório é gerado pelo `next dev` e aponta para a documentação da versão instalada do Next em `node_modules/next/dist/docs/`.
