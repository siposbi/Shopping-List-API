using System.Security.Claims;

namespace SharedShoppingList.Api.Extensions
{
    public static class ClaimsPrincipalExt
    {
        public static long GetId(this ClaimsPrincipal claimsPrincipal)
        {
            return long.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier));
        }
    }
}