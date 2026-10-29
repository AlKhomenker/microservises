namespace TodoApp.Shared.Kernel;

public interface IUnitOfWork
{
    /* --- Methods --- */
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default); // сохраняет все изменения, возвращает число записанных строк
}
