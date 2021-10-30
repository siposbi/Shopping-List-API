namespace SharedShoppingList.Data.Dto
{
    public class ExportDto
    {
        public ExportDto(string firstName, string lastName, long money)
        {
            FirstName = firstName;
            LastName = lastName;
            Money = money;
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public long Money { get; set; }
    }
}