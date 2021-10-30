using System.ComponentModel.DataAnnotations;

namespace SharedShoppingList.Data.Models
{
    public class RegisterModel
    {
        [MaxLength(35)] [Required] public string FirstName { get; set; }
        [MaxLength(35)] [Required] public string LastName { get; set; }
        [MaxLength(320)] [Required] public string Email { get; set; }
        [Required] public string Password { get; set; }
    }
}