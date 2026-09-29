namespace Combinado.Domain.Caixas;

public enum StatusCaixa
{
    /// <summary>Só um parceiro; aguardando o "SIM COMBINADO" do outro.</summary>
    Pendente = 1,

    /// <summary>Dois parceiros vinculados. Registro de gastos liberado.</summary>
    Ativo = 2,
}
