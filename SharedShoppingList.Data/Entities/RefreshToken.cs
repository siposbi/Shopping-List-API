using System;

namespace SharedShoppingList.Data.Entities
{
    public class RefreshToken
    {
        public int RefreshTokenId { get; set; }
        public string Token { get; set; }
        public string JwtId { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool? Used { get; set; }
        public long UserId { get; set; }
        public User User { get; set; }
    }
}