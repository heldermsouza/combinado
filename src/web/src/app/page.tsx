import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";

const pilares = [
  {
    titulo: "Registra em segundos",
    descricao:
      "“Gastei 85 no mercado” por texto ou áudio no WhatsApp. A IA categoriza e o casal vê na hora.",
  },
  {
    titulo: "Avisa antes de estourar",
    descricao:
      "Alertas em 80%, 100% e 120% do orçamento de cada categoria. Sem surpresa no fim do mês.",
  },
  {
    titulo: "Responde “posso?”",
    descricao:
      "“Posso gastar 300 num tênis?” Resposta em menos de 5 segundos, considerando orçamento e metas do casal.",
  },
] as const;

export default function Home() {
  return (
    <div className="flex flex-1 flex-col bg-background text-foreground">
      <header className="mx-auto flex w-full max-w-5xl items-center justify-between px-6 py-6">
        <span className="text-lg font-semibold tracking-tight">Combinado</span>
        <Button variant="outline" size="sm" disabled>
          Entrar
        </Button>
      </header>

      <main className="mx-auto flex w-full max-w-5xl flex-1 flex-col gap-16 px-6 pb-24 pt-12">
        <section className="flex flex-col gap-6 sm:max-w-2xl">
          <p className="text-sm font-medium uppercase tracking-widest text-muted-foreground">
            Para casais que já tentaram planilha e app e desistiram
          </p>
          <h1 className="text-4xl font-semibold leading-tight tracking-tight sm:text-5xl">
            O CFO do casal mora no WhatsApp.
          </h1>
          <p className="text-lg leading-8 text-muted-foreground">
            Registrem gastos por texto ou áudio, recebam alertas em tempo real e
            perguntem “posso gastar?” antes de comprar. Grátis por 30 dias, sem
            cartão. Depois, R$ 29,90/mês para o casal.
          </p>
          <div className="flex flex-col gap-3 sm:flex-row">
            <Button size="lg" disabled>
              Começar grátis (em breve)
            </Button>
            <Button size="lg" variant="ghost" disabled>
              Ver como funciona
            </Button>
          </div>
        </section>

        <section className="grid gap-4 sm:grid-cols-3">
          {pilares.map((p) => (
            <Card key={p.titulo}>
              <CardHeader>
                <CardTitle>{p.titulo}</CardTitle>
                <CardDescription>{p.descricao}</CardDescription>
              </CardHeader>
              <CardContent />
            </Card>
          ))}
        </section>
      </main>

      <footer className="mx-auto w-full max-w-5xl px-6 py-8 text-sm text-muted-foreground">
        © {new Date().getFullYear()} Combinado. Beta fechado.
      </footer>
    </div>
  );
}
