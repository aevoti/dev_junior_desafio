using CentroTreinamentoPokemon.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CentroTreinamentoPokemon.Infrastructure.Mappings;

public class PlanoTreinamentoMap :
    IEntityTypeConfiguration<PlanoTreinamento>
{
    public void Configure(
        EntityTypeBuilder<PlanoTreinamento> builder)
    {
        builder.ToTable(
            "PlanoTreinamento",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_PlanoTreinamento_ValorMensal",
                    "[ValorMensal] > 0");

                tableBuilder.HasCheckConstraint(
                    "CK_PlanoTreinamento_NivelPlano",
                    "[NivelPlano] BETWEEN 1 AND 3");
            });

        builder.HasKey(plano => plano.Id);

        builder.Property(plano => plano.Id)
            .ValueGeneratedOnAdd();

        builder.Property(plano => plano.Nome)
            .HasColumnType("VARCHAR(100)")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(plano => plano.ValorMensal)
            .HasColumnType("DECIMAL(10,2)")
            .IsRequired();

        builder.Property(plano => plano.Descricao)
            .HasColumnType("VARCHAR(300)")
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(plano => plano.NivelPlano)
            .HasColumnType("INT")
            .IsRequired();

        builder.HasIndex(plano => plano.Nome)
            .IsUnique();

        builder.HasIndex(plano => plano.NivelPlano)
            .IsUnique();

        builder.HasMany(plano => plano.Matriculas)
            .WithOne(matricula => matricula.PlanoTreinamento)
            .HasForeignKey(matricula => matricula.PlanoTreinamentoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}