using System.ComponentModel.DataAnnotations;

namespace SharedShoppingList.Data.Models
{
    public class RenameModel
    {
        [Required] public long ListId { get; set; }
        [Required] public string NewName { get; set; }
    }
}