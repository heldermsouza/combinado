// Runtime Node.js (SSR, route handlers). Carregado por src/instrumentation.ts.
import * as Sentry from "@sentry/nextjs";
import { env } from "@/lib/env";

Sentry.init({
  dsn: env.NEXT_PUBLIC_SENTRY_DSN,
  enabled: env.NEXT_PUBLIC_SENTRY_DSN !== "",
  environment: process.env.NODE_ENV,
  tracesSampleRate: process.env.NODE_ENV === "production" ? 0.1 : 1.0,
  sendDefaultPii: false,
});
