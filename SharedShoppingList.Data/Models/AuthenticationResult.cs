using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using SharedShoppingList.Data.Extensions;

namespace SharedShoppingList.Data.Models
{
    public class TokenModel
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }

        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime TokenValidUntil { get; set; }

        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime RefreshTokenValidUntil { get; set; }
    }

    public class AuthenticationResult : TokenModel
    {
        public bool Success { get; set; }
        public IEnumerable<string> Errors { get; set; }
    }
}