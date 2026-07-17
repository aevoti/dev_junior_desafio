using CentroTreinamentoPokemon.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CentroTreinamentoPokemon.Infrastructure.Mappings;

public class TreinadorMap : IEntityTypeConfiguration<Treinador>
{
    public void Configure(EntityTypeBuilder<Treinador> builder)
    {
        builder.ToTable("Treinador");

        builder.HasKey(treinador => treinador.Id);

        builder.Property(treinador => treinador.Id)
            .ValueGeneratedOnAdd();

        builder.Property(treinador => treinador.Nome)
            .HasColumnType("VARCHAR(150)")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(treinador => treinador.Email)
            .HasColumnType("VARCHAR(254)")
            .HasMaxLength(254)
            .IsRequired();

        builder.Property(treinador => treinador.CidadeOrigem)
            .HasColumnType("VARCHAR(150)")
            .HasMaxLength(150)
            .IsRequired();

        builder.HasIndex(treinador => treinador.Email)
            .IsUnique();

        builder.HasMany(treinador => treinador.Pokemons)
            .WithOne(pokemon => pokemon.Treinador)
            .HasForeignKey(pokemon => pokemon.TreinadorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}