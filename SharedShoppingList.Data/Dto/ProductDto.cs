using System;

namespace SharedShoppingList.Data.Dto
{
    public class ProductDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string AddedByUserFirstName { get; set; }
        public string AddedByUserLastName { get; set; }
        public string BoughtByUserFirstName { get; set; }
        public string BoughtByUserLastName { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime BoughtDateTime { get; set; }
        public long Price { get; set; }
        public bool IsShared { get; set; }
    }
}