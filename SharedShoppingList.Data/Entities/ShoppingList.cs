﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SharedShoppingList.Data.Entities
{
    public class ShoppingList : ISoftDeletable
    {
        [Key] public long Id { get; set; }
        [MaxLength(50)] [Required] public string Name { get; set; }
        [Required] public DateTime CreatedDateTime { get; set; } = DateTime.Now;
        [Required] public DateTime LastEditedDateTime { get; set; } = DateTime.Now;
        [Required] public User CreatedByUser { get; set; }
        public string ShareCode { get; set; }
        public bool IsActive { get; set; } = true;
        
        public ICollection<UserShoppingList> Users { get; set; } = null!;
        public ICollection<Product> Products { get; set; } = null!;
    }
}