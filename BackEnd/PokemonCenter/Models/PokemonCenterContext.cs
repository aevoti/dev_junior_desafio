using Microsoft.EntityFrameworkCore;

namespace PokemonCenter.Models;

public partial class PokemonCenterContext : DbContext
{
    public PokemonCenterContext()
    {
    }

    public PokemonCenterContext(DbContextOptions<PokemonCenterContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Matricula> Matriculas { get; set; }

    public virtual DbSet<Plano> Planos { get; set; }

    public virtual DbSet<Pokemon> Pokemons { get; set; }

    public virtual DbSet<Treinador> Treinadors { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Matricula>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Matricul__3213E83F18F98DC1");

            entity.ToTable("Matricula");

            entity.HasIndex(e => e.PokemonId, "UX_Matricula_PokemonAtiva")
                .IsUnique()
                .HasFilter("([status]='Ativa')");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DataFim).HasColumnName("dataFim");
            entity.Property(e => e.DataInicio).HasColumnName("dataInicio");
            entity.Property(e => e.PlanoId).HasColumnName("planoId");
            entity.Property(e => e.PokemonId).HasColumnName("pokemonId");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.ValorPrimeiraCobranca)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("valorPrimeiraCobranca");

            entity.HasOne(d => d.Plano).WithMany(p => p.Matriculas)
                .HasForeignKey(d => d.PlanoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Matricula_Plano");

            entity.HasOne(d => d.Pokemon).WithMany(p => p.Matriculas)
                .HasForeignKey(d => d.PokemonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Matricula_Pokemon");
        });

        modelBuilder.Entity<Plano>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Planos__3213E83F4AF5123B");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Descricao)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("descricao");
            entity.Property(e => e.Nome)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nome");
            entity.Property(e => e.ValorMensal)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("valorMensal");
        });

        modelBuilder.Entity<Pokemon>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Pokemon__3213E83F61EAAE18");

            entity.ToTable("Pokemon");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Nivel).HasColumnName("nivel");
            entity.Property(e => e.Nome)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nome");
            entity.Property(e => e.Tipo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("tipo");
            entity.Property(e => e.TreinadorId).HasColumnName("treinadorId");

            entity.HasOne(d => d.Treinador).WithMany(p => p.Pokemons)
                .HasForeignKey(d => d.TreinadorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pokemon_Treinador");
        });

        modelBuilder.Entity<Treinador>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Treinado__3213E83F374DCBE9");

            entity.ToTable("Treinador");

            entity.HasIndex(e => e.Email, "UQ__Treinado__AB6E6164725D7CED").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CidadeDeOrigem)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("cidadeDeOrigem");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Nome)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nome");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
