using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging;

namespace SharedShoppingList.Data.Design
{
    // ReSharper disable once UnusedType.Global
    internal class ShoppingListDesignTimeDbContextFactory : IDesignTimeDbContextFactory<ShoppingListDbContext>
    {
        public ShoppingListDbContext CreateDbContext(string[] args)
        {
            return new ShoppingListDbContext(new Logger<ShoppingListDbContext>(new LoggerFactory()),
                new DbContextOptionsBuilder<ShoppingListDbContext>()
                    .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=ShoppingList").Options);
        }
    }
}