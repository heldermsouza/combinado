import type { NextConfig } from "next";
import { withSentryConfig } from "@sentry/nextjs/config";

const nextConfig: NextConfig = {
  // Necessário para a imagem Docker (copia .next/standalone + server.js).
  output: "standalone",
  reactStrictMode: true,
  poweredByHeader: false,
};

// Upload de source maps só acontece com SENTRY_AUTH_TOKEN (+ SENTRY_ORG / SENTRY_PROJECT) no ambiente de build.
// Sem token o build segue normal e o SDK fica só com o DSN público.
export default withSentryConfig(nextConfig, {
  silent: !process.env.CI,
  widenClientFileUpload: true,
  sourcemaps: {
    disable: !process.env.SENTRY_AUTH_TOKEN,
  },
});
