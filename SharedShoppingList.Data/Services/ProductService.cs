using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SharedShoppingList.Data.Dto;
using SharedShoppingList.Data.Entities;
using SharedShoppingList.Data.Extensions;
using SharedShoppingList.Data.Mapping;
using SharedShoppingList.Data.Models;

namespace SharedShoppingList.Data.Services
{
    public interface IProductService
    {
        Task<ResponseModel<ProductMinDto>> Create(long userId, ProductCreateModel productModel);
        Task<ResponseModel<IEnumerable<ProductMinDto>>> GetEveryProductForList(long listId);
        Task<ResponseModel<long>> Delete(long productId);
        Task<ResponseModel<ProductMinDto>> UndoDelete(long productId);
        Task<ResponseModel<ProductMinDto>> Buy(long userId, long productId);
        Task<ResponseModel<ProductMinDto>> UndoBuy(long userId, long productId);
        Task<long?> GetListIdForProduct(long productId);
        Task<ResponseModel<ProductMinDto>> Update(long productId, ProductUpdateModel productModel);
        Task<long> GetAddedByUserId(long productId);
        Task<ResponseModel<ProductDto>> Get(long id);
    }

    public class ProductService : IProductService
    {
        private readonly ICommonService _commonService;

        private readonly ShoppingListDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public ProductService(ShoppingListDbContext dbContext, ICommonService commonService, IUserService userService,
            IMapper mapper)
        {
            _dbContext = dbContext;
            _commonService = commonService;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<ResponseModel<ProductMinDto>> Create(long userId, ProductCreateModel productModel)
        {
            var response = new ResponseModel<ProductMinDto>();
            try
            {
                var user = await _userService.GetActiveUser(userId);
                if (user == null) return response.Unsuccessful("User not found.");

                var shoppingList = await _commonService.GetActiveShoppingList(productModel.ShoppingListId);
                if (shoppingList == null) return response.Unsuccessful("Shopping List not found.");

                if (productModel.Price <= 0) return response.Unsuccessful("Product price can not be less than 0.");

                var newProduct = new Product
                {
                    AddedByUser = user,
                    CreatedDateTime = DateTime.Now,
                    Name = productModel.Name,
                    Price = productModel.Price,
                    IsShared = productModel.IsShared,
                    ShoppingList = shoppingList
                };

                await _dbContext.Products.AddAsync(newProduct);

                await _dbContext.SaveChangesAsync();

                response.Data = await _mapper.ProjectToAsync<Product, ProductMinDto>(_dbContext.Products, newProduct);
                return response;
            }
            catch (Exception)
            {
                return response.Exception();
            }
        }

        public async Task<ResponseModel<IEnumerable<ProductMinDto>>> GetEveryProductForList(long listId)
        {
            var response = new ResponseModel<IEnumerable<ProductMinDto>>();
            try
            {
                var shoppingList = await _commonService.GetActiveShoppingList(listId);
                if (shoppingList == null) return response.Unsuccessful("Shopping List not found.");

                var products = await _dbContext.Products.Active().Where(p => p.ShoppingList == shoppingList)
                    .OrderByDescending(p => p.CreatedDateTime)
                    .ProjectTo<ProductMinDto>(_mapper.ConfigurationProvider)
                    .ToListAsync();
                response.Data = products;
                return response;
            }
            catch (Exception)
            {
                return response.Exception();
            }
        }

        public async Task<ResponseModel<long>> Delete(long productId)
        {
            var response = new ResponseModel<long>();
            try
            {
                var product = await GetActiveProduct(productId);
                if (product == null) return response.Unsuccessful("Product not found.");

                if (await GetBoughtUserId(product.Id) != null)
                    return response.Unsuccessful("Can't delete a product that had already been bought.");

                product.IsActive = false;
                await _dbContext.SaveChangesAsync();

                response.Data = productId;
                return response;
            }
            catch (Exception)
            {
                return response.Exception();
            }
        }

        public async Task<ResponseModel<ProductMinDto>> UndoDelete(long productId)
        {
            var response = new ResponseModel<ProductMinDto>();
            try
            {
                var product = await _dbContext.Products.SingleOrDefaultAsync(p => p.Id == productId);
                if (product == null) return response.Unsuccessful("Product not found.");

                product.IsActive = true;
                await _dbContext.SaveChangesAsync();

                response.Data = _mapper.Map<ProductMinDto>(product);
                return response;
            }
            catch (Exception)
            {
                return response.Exception();
            }
        }

        public async Task<ResponseModel<ProductMinDto>> Buy(long userId, long productId)
        {
            var response = new ResponseModel<ProductMinDto>();
            try
            {
                var user = await _userService.GetActiveUser(userId);
                if (user == null) return response.Unsuccessful("User not found.");

                var product = await GetActiveProduct(productId);
                if (product == null) return response.Unsuccessful("Product not found.");

                if (await GetBoughtUserId(product.Id) != null) return response.Unsuccessful("Product already bought.");

                product.BoughtByUser = user;
                product.BoughtDateTime = DateTime.Now;
                await _dbContext.SaveChangesAsync();

                response.Data = _mapper.Map<ProductMinDto>(product);
                return response;
            }
            catch (Exception)
            {
                return response.Exception();
            }
        }

        public async Task<ResponseModel<ProductMinDto>> UndoBuy(long userId, long productId)
        {
            var response = new ResponseModel<ProductMinDto>();
            try
            {
                var user = await _userService.GetActiveUser(userId);
                if (user == null) return response.Unsuccessful("User not found.");

                var product = await GetActiveProduct(productId);
                if (product == null) return response.Unsuccessful("Product not found.");

                if (await GetBoughtUserId(product.Id) != user.Id)
                    return response.Unsuccessful("You didn't buy this product.");

                product.BoughtByUser = null;
                product.BoughtDateTime = null;
                await _dbContext.SaveChangesAsync();

                response.Data = await _mapper.ProjectToAsync<Product, ProductMinDto>(_dbContext.Products, product);
                return response;
            }
            catch (Exception)
            {
                return response.Exception();
            }
        }


        public async Task<long?> GetListIdForProduct(long productId)
        {
            var id = await _dbContext.Products.Where(p => p.Id == productId)
                .Select(p => p.ShoppingList.Id).SingleOrDefaultAsync();
            return id == 0 ? null : id;
        }

        public async Task<ResponseModel<ProductMinDto>> Update(long productId, ProductUpdateModel productModel)
        {
            var response = new ResponseModel<ProductMinDto>();
            try
            {
                var product = await GetActiveProduct(productId);
                if (product == null) return response.Unsuccessful("Product not found.");

                if (await GetBoughtUserId(product.Id) != null)
                    return response.Unsuccessful("Can't update a product that had already been bought.");

                product.Name = productModel.Name;
                product.Price = productModel.Price;
                product.IsShared = productModel.IsShared;

                await _dbContext.SaveChangesAsync();

                response.Data = await _mapper.ProjectToAsync<Product, ProductMinDto>(_dbContext.Products, product);
                return response;
            }
            catch (Exception)
            {
                return response.Exception();
            }
        }

        public async Task<ResponseModel<ProductDto>> Get(long id)
        {
            var response = new ResponseModel<ProductDto>();
            try
            {
                var product = await GetActiveProduct(id);
                if (product == null) return response.Unsuccessful("Product not found.");

                response.Data = await _mapper.ProjectToAsync<Product, ProductDto>(_dbContext.Products, product);
                return response;
            }
            catch (Exception)
            {
                return response.Exception();
            }
        }

        public async Task<long> GetAddedByUserId(long productId)
        {
            return await _dbContext.Products.Where(p => p.Id == productId).Select(p => p.AddedByUser.Id).SingleAsync();
        }

        private async Task<Product> GetActiveProduct(long id)
        {
            return await _dbContext.Products.Active().SingleOrDefaultAsync(p => p.Id == id);
        }

        private async Task<long?> GetBoughtUserId(long productId)
        {
            var id = await _dbContext.Products.Where(p => p.Id == productId && p.BoughtByUser != null)
                .Select(p => p.BoughtByUser.Id).SingleOrDefaultAsync();
            return id == 0 ? null : id;
        }
    }
}