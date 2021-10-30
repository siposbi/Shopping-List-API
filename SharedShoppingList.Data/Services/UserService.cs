using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SharedShoppingList.Data;
using SharedShoppingList.Data.Entities;
using SharedShoppingList.Data.Extensions;

namespace SharedShoppingList.Data.Services
{
    public interface IUserService
    {
        Task<User> GetActiveUser(long userId);
        Task<bool> UserIsMemberOfList(long shoppingListId, long userId);
    }

    public class UserService : IUserService
    {
        public UserService(ShoppingListDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private readonly ShoppingListDbContext _dbContext;

        public async Task<User> GetActiveUser(long userId)
        {
            return await _dbContext.Users.Active().SingleOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<bool> UserIsMemberOfList(long shoppingListId, long userId)
        {
            return await _dbContext.UserShoppingLists.Active()
                .AnyAsync(usl => usl.ShoppingListId == shoppingListId && usl.UserId == userId);
        }
    }
}