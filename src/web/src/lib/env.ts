import { z } from "zod";

/**
 * Variáveis públicas (inlined no build pelo Next). Validadas uma vez no import;
 * valor inválido quebra o build/boot em vez de virar erro silencioso em runtime.
 */
const publicEnvSchema = z.object({
  NEXT_PUBLIC_API_URL: z.url().default("http://localhost:8080"),
  NEXT_PUBLIC_SENTRY_DSN: z.union([z.url(), z.literal("")]).default(""),
});

export const env = publicEnvSchema.parse({
  NEXT_PUBLIC_API_URL: process.env.NEXT_PUBLIC_API_URL,
  NEXT_PUBLIC_SENTRY_DSN: process.env.NEXT_PUBLIC_SENTRY_DSN,
});

export type PublicEnv = z.infer<typeof publicEnvSchema>;
