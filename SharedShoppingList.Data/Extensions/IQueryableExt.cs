using System;
using System.Collections.Generic;
using System.Linq;
using SharedShoppingList.Data.Entities;

namespace SharedShoppingList.Data.Extensions
{
    public static class QueryableExt
    {
        public static IQueryable<T> Active<T>(this IQueryable<T> dbSet) where T : class, ISoftDeletable
        {
            return dbSet.Where(o => o.IsActive);
        }

        public static IEnumerable<T> Active<T>(this IEnumerable<T> dbSet) where T : class, ISoftDeletable
        {
            return dbSet.Where(o => o.IsActive);
        }

        public static IQueryable<Product> BoughtBetween(this IQueryable<Product> products, DateTime start, DateTime end)
        {
            return products.Where(p => p.BoughtDateTime >= start && p.BoughtDateTime <= end);
        }
    }
}