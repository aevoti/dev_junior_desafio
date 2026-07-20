using CentroTreinamentoPokemon.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CentroTreinamentoPokemon.Infrastructure.Mappings;

public class MatriculaMap : IEntityTypeConfiguration<Matricula>
{
    public void Configure(EntityTypeBuilder<Matricula> builder)
    {
        builder.ToTable(
            "Matricula",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_Matricula_Status",
                    "[Status] IN (1, 2, 3)");

                tableBuilder.HasCheckConstraint(
                    "CK_Matricula_ValorMensal",
                    "[ValorMensal] > 0");
            });

        builder.HasKey(matricula => matricula.Id);

        builder.Property(matricula => matricula.Id)
            .ValueGeneratedOnAdd();

        builder.Property(matricula => matricula.PokemonId)
            .HasColumnType("INT")
            .IsRequired();

        builder.Property(matricula => matricula.PlanoTreinamentoId)
            .HasColumnType("INT")
            .IsRequired();

        builder.Property(matricula => matricula.DataInicio)
            .HasColumnType("DATETIME2")
            .IsRequired();

        builder.Property(matricula => matricula.DataEncerramento)
            .HasColumnType("DATETIME2")
            .IsRequired(false);

        builder.Property(matricula => matricula.Status)
            .HasColumnType("INT")
            .IsRequired();

        builder.Property(matricula => matricula.ValorMensal)
            .HasColumnType("DECIMAL(10,2)")
            .IsRequired();

        builder.HasIndex(matricula => matricula.PokemonId)
            .HasDatabaseName("UX_Matricula_Pokemon_Ativa")
            .IsUnique()
            .HasFilter("[Status] = 1");
    }
}