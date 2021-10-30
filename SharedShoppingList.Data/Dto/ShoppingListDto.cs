using System;
using SharedShoppingList.Data.Entities;

namespace SharedShoppingList.data.Dto
{
    public class ShoppingListDto
    {
        public int NumberOfProducts { get; set; }
        public long Id { get; set; }
        public string ShareCode { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime LastEditedDateTime { get; set; }
        public long CreatedByUserId { get; set; }
        public bool IsShared { get; set; }
    }
}