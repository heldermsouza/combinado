using Combinado.Domain.ValueObjects;

namespace Combinado.Domain.Tests.Support;

public static class Numeros
{
    public static NumeroWhatsApp Ana => NumeroWhatsApp.Criar("+55 11 91111-1111").Value;

    public static NumeroWhatsApp Bruno => NumeroWhatsApp.Criar("(21) 92222-2222").Value;

    public static NumeroWhatsApp Carla => NumeroWhatsApp.Criar("31933333333").Value;
}
