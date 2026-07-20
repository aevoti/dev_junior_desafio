using CentroTreinamentoPokemon.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CentroTreinamentoPokemon.Infrastructure.Mappings;

public class PokemonMap : IEntityTypeConfiguration<Pokemon>
{
    public void Configure(EntityTypeBuilder<Pokemon> builder)
    {
        builder.ToTable(
            "Pokemon",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_Pokemon_Nivel",
                    "[Nivel] BETWEEN 1 AND 100");
            });

        builder.HasKey(pokemon => pokemon.Id);

        builder.Property(pokemon => pokemon.Id)
            .ValueGeneratedOnAdd();

        builder.Property(pokemon => pokemon.Nome)
            .HasColumnType("VARCHAR(100)")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(pokemon => pokemon.Tipo)
            .HasColumnType("INT")
            .IsRequired();

        builder.Property(pokemon => pokemon.Nivel)
            .HasColumnType("INT")
            .IsRequired();

        builder.Property(pokemon => pokemon.TreinadorId)
            .HasColumnType("INT")
            .IsRequired();

        builder.HasMany(pokemon => pokemon.Matriculas)
            .WithOne(matricula => matricula.Pokemon)
            .HasForeignKey(matricula => matricula.PokemonId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}