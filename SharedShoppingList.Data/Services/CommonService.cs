using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SharedShoppingList.Data;
using SharedShoppingList.Data.Entities;
using SharedShoppingList.Data.Extensions;

namespace SharedShoppingList.Data.Services
{
    public interface ICommonService
    {
        Task<ShoppingList> GetActiveShoppingList(long shoppingListId);
    }

    public class CommonService : ICommonService
    {
        public CommonService(ShoppingListDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private readonly ShoppingListDbContext _dbContext;

        public async Task<ShoppingList> GetActiveShoppingList(long shoppingListId)
        {
            return await _dbContext.ShoppingLists.Active().SingleOrDefaultAsync(sl => sl.Id == shoppingListId);
        }
    }
}