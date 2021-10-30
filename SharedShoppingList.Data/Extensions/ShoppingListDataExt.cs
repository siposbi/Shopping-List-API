using SharedShoppingList.Data.Services;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ShoppingListDataExt
    {
        // ReSharper disable once UnusedMethodReturnValue.Global
        public static IServiceCollection AddShoppingListDataService(this IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            services.Add(new ServiceDescriptor(typeof(IIdentityService), typeof(IdentityService), serviceLifetime));
            services.Add(new ServiceDescriptor(typeof(IUserService), typeof(UserService), serviceLifetime));
            services.Add(new ServiceDescriptor(typeof(IShoppingListService), typeof(ShoppingListService), serviceLifetime));
            services.Add(new ServiceDescriptor(typeof(IProductService), typeof(ProductService), serviceLifetime));
            services.Add(new ServiceDescriptor(typeof(ICommonService), typeof(CommonService), serviceLifetime));
            return services;
        }
    }
}