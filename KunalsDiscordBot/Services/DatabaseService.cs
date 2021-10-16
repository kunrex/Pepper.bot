using System;
using System.Threading.Tasks;

using DiscordBotDataBase.Dal;

namespace KunalsDiscordBot.Services
{
    public abstract class DatabaseService : BotService
    {
        protected readonly DataContext context;

        public DatabaseService(DataContext _context) => context = _context;

        protected async Task<bool> AddEntity<T>(T entityToAdd)
        {
            if (entityToAdd == null)
                return false;

            var addEntry = context.Add(entityToAdd);
            await context.SaveChangesAsync();

            addEntry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            return true;
        }

        protected async Task<bool> RemoveEntity<T>(T entityToRemove)
        {
            if (entityToRemove == null)
                return false;

            var removeEntry = context.Remove(entityToRemove);
            await context.SaveChangesAsync();

            removeEntry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            return true;
        }

        protected async Task<bool> UpdateEntity<T>(T entityToUpdate)
        {
            if (entityToUpdate == null)
                return false;

            var updateEntry = context.Update(entityToUpdate);

            await context.SaveChangesAsync();
            updateEntry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            return true;
        }

        protected async Task<bool> ModifyEntity<T>(T entity, Action<T> modification) where T : IEntity
        {
            modification.Invoke(entity);

            return await UpdateEntity(entity);
        }
    }
}
