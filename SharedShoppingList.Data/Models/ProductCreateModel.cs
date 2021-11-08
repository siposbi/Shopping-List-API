﻿using System.ComponentModel.DataAnnotations;

namespace SharedShoppingList.Data.Models
{
    public class ProductCreateModel
    {
        [Required] public long ShoppingListId { get; set; }
        [Required] [MaxLength(30)] public string Name { get; set; }
        [Required] [Range(0, long.MaxValue)] public long Price { get; set; }
        [Required] public bool IsShared { get; set; }
    }
}