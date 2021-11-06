namespace SharedShoppingList.Data.Models
{
    public class ServiceConfiguration
    {
        public JwtSettings JwtSettings { get; set; }
    }

    public class JwtSettings
    {
        public string Secret { get; set; }

        public int TokenLifetimeInDays { get; set; }
    }
}