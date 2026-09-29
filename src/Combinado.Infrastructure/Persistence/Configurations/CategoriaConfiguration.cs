using Combinado.Domain.Caixas;
using Combinado.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Combinado.Infrastructure.Persistence.Configurations;

public sealed class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("categoria");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasConversion(id => id.Value, value => new CategoriaId(value))
            .ValueGeneratedNever();

        builder.Property(c => c.Nome)
            .HasMaxLength(Categoria.NomeMaxLength);

        builder.Property(c => c.Slug)
            .HasMaxLength(Categoria.SlugMaxLength);

        builder.Property(c => c.Ordem);
        builder.Property(c => c.Padrao);

        builder.HasIndex("CaixaId", nameof(Categoria.Slug))
            .IsUnique()
            .HasDatabaseName("ux_categoria_caixa_slug");
    }
}
