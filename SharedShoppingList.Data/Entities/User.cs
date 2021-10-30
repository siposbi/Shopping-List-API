using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SharedShoppingList.Data.Entities
{
    public class User : ISoftDeletable
    {
        [Key] public long Id { get; set; }
        [MaxLength(35)] [Required] public string FirstName { get; set; }
        [MaxLength(35)] [Required] public string LastName { get; set; }
        [MaxLength(320)] [Required] public string Email { get; set; }
        [Required] public string Password { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<UserShoppingList> ShoppingLists { get; set; } = null!;
        public virtual ICollection<RefreshToken> RefreshToken { get; set; } = null!;
    }
}