using System;
using System.ComponentModel.DataAnnotations;

namespace SharedShoppingList.Data.Entities
{
    public class Product : ISoftDeletable
    {
        [Key] public long Id { get; set; }
        [Required] public User AddedByUser { get; set; }
        public User BoughtByUser { get; set; }
        [Required] public DateTime CreatedDateTime { get; set; }
        public DateTime? BoughtDateTime { get; set; }
        [MaxLength(30)] [Required] public string Name { get; set; }
        [Required] [Range(0, long.MaxValue)] public long Price { get; set; }
        [Required] public bool IsShared { get; set; }

        [Required] public ShoppingList ShoppingList { get; set; }
        public bool IsActive { get; set; } = true;
    }
}