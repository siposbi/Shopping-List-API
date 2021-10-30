﻿using System;

namespace SharedShoppingList.Data.Dto
{
    public class MemberDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime JoinDateTime { get; set; }
        public bool IsOwner { get; set; }
    }
}