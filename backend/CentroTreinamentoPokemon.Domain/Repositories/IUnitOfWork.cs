namespace CentroTreinamentoPokemon.Domain.Repositories;

public interface IUnitOfWork
{
    Task<int> CommitAsync();
}