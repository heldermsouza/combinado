using Combinado.Domain.Caixas;
using Combinado.Domain.Common;
using Combinado.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Combinado.Infrastructure.Persistence.Configurations;

public sealed class ParceiroConfiguration : IEntityTypeConfiguration<Parceiro>
{
    public void Configure(EntityTypeBuilder<Parceiro> builder)
    {
        builder.ToTable("parceiro");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasConversion(id => id.Value, value => new ParceiroId(value))
            .ValueGeneratedNever();

        builder.Property(p => p.Nome)
            .HasMaxLength(Parceiro.NomeMaxLength);

        builder.Property(p => p.NumeroWhatsApp)
            .HasColumnName("numero_whatsapp") // a convenção snake_case geraria "numero_whats_app"
            .HasConversion(numero => numero.Valor, valor => NumeroWhatsApp.DeE164(valor))
            .HasMaxLength(16);

        // RN01: um número pertence a exatamente um Caixa. O agregado impede repetição interna;
        // este índice impede repetição entre Caixas.
        builder.HasIndex(p => p.NumeroWhatsApp)
            .IsUnique()
            .HasDatabaseName("ux_parceiro_numero_whatsapp");

        builder.Property(p => p.EmailPrimario);
        builder.Property(p => p.ConvidadoEm);
        builder.Property(p => p.VinculadoEm);

        builder.Ignore(p => p.Vinculado);
    }
}
