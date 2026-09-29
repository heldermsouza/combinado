using Combinado.Domain.Caixas;
using Combinado.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Combinado.Infrastructure.Persistence.Configurations;

public sealed class CaixaConfiguration : IEntityTypeConfiguration<Caixa>
{
    public void Configure(EntityTypeBuilder<Caixa> builder)
    {
        builder.ToTable("caixa");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasConversion(id => id.Value, value => new CaixaId(value))
            .ValueGeneratedNever();

        builder.Property(c => c.Status)
            .HasConversion<string>()
            .HasMaxLength(16);

        builder.Property(c => c.Timezone)
            .HasMaxLength(Caixa.TimezoneMaxLength);

        builder.Property(c => c.CriadoEm);

        // Coleções do agregado: sempre carregadas junto (agregado pequeno, 2 parceiros + ~10 categorias).
        builder.HasMany(c => c.Parceiros)
            .WithOne()
            .HasForeignKey("CaixaId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(c => c.Parceiros)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();

        builder.HasMany(c => c.Categorias)
            .WithOne()
            .HasForeignKey("CaixaId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(c => c.Categorias)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();

        builder.Ignore(c => c.DomainEvents);
    }
}
