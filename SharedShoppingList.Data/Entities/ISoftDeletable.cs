namespace SharedShoppingList.Data.Entities
{
    public interface ISoftDeletable
    {
        public bool IsActive { get; set; }
    }
}