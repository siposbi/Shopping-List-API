using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedShoppingList.Data.Entities;

namespace SharedShoppingList.Data
{
    public class ShoppingListDbContext : DbContext
    {
        public ShoppingListDbContext(ILogger<ShoppingListDbContext> logger,
            DbContextOptions<ShoppingListDbContext> options) : base(options)
        {
            Logger = logger;
        }

        private ILogger<ShoppingListDbContext> Logger { get; }
        public DbSet<User> Users { get; set; }
        public DbSet<ShoppingList> ShoppingLists { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<UserShoppingList> UserShoppingLists { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
    }
}