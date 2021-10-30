using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging;
using SharedShoppingList.data;

namespace SharedShoppingList.Data.Design
{
    // ReSharper disable once UnusedType.Global
    internal class ShoppingListDesignTimeDbContextFactory : IDesignTimeDbContextFactory<ShoppingListDbContext>
    {
        public ShoppingListDbContext CreateDbContext(string[] args) =>
            new(new Logger<ShoppingListDbContext>(new LoggerFactory()),
                new DbContextOptionsBuilder<ShoppingListDbContext>()
                    .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=ShoppingList").Options);
    }
}