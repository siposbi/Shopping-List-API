using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace SharedShoppingList.Data.Mapping
{
    public static class MapperExt
    {
        /// <summary>
        ///     Helper method, that can map to object, that would require dbContext for properties.
        /// </summary>
        /// <param name="mapper">IMapper implementation, which this method extends.</param>
        /// <param name="dbSet">DbSet of the source entities.</param>
        /// <param name="obj">Source object.</param>
        /// <typeparam name="TSource">Class to be mapped.</typeparam>
        /// <typeparam name="TDto">Result class.</typeparam>
        /// <returns>A single object mapped from TSource to TDto</returns>
        public static async Task<TDto> ProjectToAsync<TSource, TDto>(this IMapper mapper, DbSet<TSource> dbSet,
            TSource obj) where TSource : class
        {
            return await mapper.ProjectTo<TDto>(dbSet.Where(x => x == obj)).SingleAsync();
        }
    }
}