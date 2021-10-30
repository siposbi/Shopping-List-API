using System.Collections.Generic;

namespace SharedShoppingList.Data.Models
{
    public class TokenModel
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
      
    }

    public class AuthenticationResult: TokenModel
    {
        public bool Success { get; set; }
        public IEnumerable<string> Errors { get; set; }
    }
}
