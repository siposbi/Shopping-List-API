namespace SharedShoppingList.data.Dto
{
    public class ProductDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string AddedByUserFirstName { get; set; }
        public string AddedByUserLastName { get; set; }
        public long Price { get; set; }
        public bool IsShared { get; set; }
    }
}