using System;

namespace SharedShoppingList.Data.Dto
{
    public class ShoppingListDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int NumberOfProducts { get; set; }
        public string ShareCode { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime LastProductAddedDateTime { get; set; }
        public bool IsShared { get; set; }
    }
}