"use client";

import * as Sentry from "@sentry/nextjs";
import { useEffect } from "react";
import { Button } from "@/components/ui/button";

/**
 * Último recurso quando o root layout quebra. Substitui <html>/<body>, por isso
 * não usa o layout nem os providers.
 */
export default function GlobalError({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  useEffect(() => {
    Sentry.captureException(error);
  }, [error]);

  return (
    <html lang="pt-BR">
      <body className="flex min-h-screen flex-col items-center justify-center gap-4 p-6 text-center">
        <h1 className="text-2xl font-semibold">Algo deu errado por aqui.</h1>
        <p className="text-muted-foreground">
          Já fomos avisados. Tente de novo em instantes.
          {error.digest ? ` Código: ${error.digest}` : null}
        </p>
        <Button onClick={reset}>Tentar novamente</Button>
      </body>
    </html>
  );
}
