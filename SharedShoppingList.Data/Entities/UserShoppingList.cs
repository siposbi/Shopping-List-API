using System;

namespace SharedShoppingList.Data.Entities
{
    public class UserShoppingList : ISoftDeletable
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public User User { get; set; }
        public long ShoppingListId { get; set; }
        public ShoppingList ShoppingList { get; set; }
        public DateTime JoinDateTime { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
    }
}