using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SharedShoppingList.data;
using SharedShoppingList.data.Dto;
using SharedShoppingList.Data.Entities;
using SharedShoppingList.Data.Extensions;
using SharedShoppingList.data.Mapping;
using SharedShoppingList.Data.Models;

namespace SharedShoppingList.Data.Services
{
    public interface IProductService
    {
        Task<ResponseModel<ProductDto>> Create(long userId, ProductCreateDto productDto);
        Task<ResponseModel<IEnumerable<ProductDto>>> GetEveryProductForList(long listId);
        Task<ResponseModel<bool>> Delete(long productId);
        Task<ResponseModel<ProductDto>> UndoDelete(long productId);
        Task<ResponseModel<ProductDto>> Buy(long userId, long productId);
        Task<ResponseModel<ProductDto>> UndoBuy(long userId, long productId);
        Task<long?> GetListIdForProduct(long productId);
        Task<ResponseModel<ProductDto>> Update(long productId, ProductCreateDto productDto);
        Task<long> GetAddedByUserId(long productId);
    }

    public class ProductService : IProductService
    {
        public ProductService(ShoppingListDbContext dbContext, ICommonService commonService, IUserService userService,
            IMapper mapper)
        {
            _dbContext = dbContext;
            _commonService = commonService;
            _userService = userService;
            _mapper = mapper;
        }

        private readonly ShoppingListDbContext _dbContext;
        private readonly ICommonService _commonService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public async Task<ResponseModel<ProductDto>> Create(long userId, ProductCreateDto productDto)
        {
            var response = new ResponseModel<ProductDto>();
            try
            {
                var user = await _userService.GetActiveUser(userId);
                if (user == null)
                {
                    return response.Unsuccessful("User not found.");
                }

                var shoppingList = await _commonService.GetActiveShoppingList(productDto.ShoppingListId);
                if (shoppingList == null)
                {
                    return response.Unsuccessful("Shopping List not found.");
                }

                if (productDto.Price <= 0)
                {
                    return response.Unsuccessful("Product price can not be less than 0.");
                }

                var newProduct = new Product
                {
                    AddedByUser = user,
                    CreatedDateTime = DateTime.Now,
                    Name = productDto.Name,
                    Price = productDto.Price,
                    IsShared = productDto.IsShared,
                    ShoppingList = shoppingList
                };

                await _dbContext.Products.AddAsync(newProduct);

                await _dbContext.SaveChangesAsync();

                response.Data = _mapper.Map<ProductDto>(newProduct);
                return response;
            }
            catch (Exception)
            {
                return response.Exception();
            }
        }

        public async Task<ResponseModel<IEnumerable<ProductDto>>> GetEveryProductForList(long listId)
        {
            var response = new ResponseModel<IEnumerable<ProductDto>>();
            try
            {
                var shoppingList = await _commonService.GetActiveShoppingList(listId);
                if (shoppingList == null)
                {
                    return response.Unsuccessful("Shopping List not found.");
                }

                var products = await _dbContext.Products.Active().Where(p => p.ShoppingList == shoppingList)
                    .OrderByDescending(p => p.CreatedDateTime)
                    .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
                    .ToListAsync();
                response.Data = products;
                return response;
            }
            catch (Exception)
            {
                return response.Exception();
            }
        }

        public async Task<ResponseModel<bool>> Delete(long productId)
        {
            var response = new ResponseModel<bool>();
            try
            {
                var product = await GetActiveProduct(productId);
                if (product == null)
                {
                    return response.Unsuccessful("Product not found.");
                }

                if (await GetBoughtUserId(product.Id) != null)
                {
                    return response.Unsuccessful("Can't delete a product that had already been bought.");
                }

                product.IsActive = false;
                await _dbContext.SaveChangesAsync();

                response.Data = true;
                return response;
            }
            catch (Exception)
            {
                return response.Exception();
            }
        }

        public async Task<ResponseModel<ProductDto>> UndoDelete(long productId)
        {
            var response = new ResponseModel<ProductDto>();
            try
            {
                var product = await _dbContext.Products.SingleOrDefaultAsync(p => p.Id == productId);
                if (product == null)
                {
                    return response.Unsuccessful("Product not found.");
                }

                product.IsActive = true;
                await _dbContext.SaveChangesAsync();

                response.Data = _mapper.Map<ProductDto>(product);
                return response;
            }
            catch (Exception)
            {
                return response.Exception();
            }
        }

        public async Task<ResponseModel<ProductDto>> Buy(long userId, long productId)
        {
            var response = new ResponseModel<ProductDto>();
            try
            {
                var user = await _userService.GetActiveUser(userId);
                if (user == null)
                {
                    return response.Unsuccessful("User not found.");
                }

                var product = await GetActiveProduct(productId);
                if (product == null)
                {
                    return response.Unsuccessful("Product not found.");
                }

                if (await GetBoughtUserId(product.Id )!= null)
                {
                    return response.Unsuccessful("Product already bought.");
                }

                product.BoughtByUser = user;
                product.BoughtDateTime = DateTime.Now;
                await _dbContext.SaveChangesAsync();

                response.Data = _mapper.Map<ProductDto>(product);
                return response;
            }
            catch (Exception)
            {
                return response.Exception();
            }
        }

        public async Task<ResponseModel<ProductDto>> UndoBuy(long userId, long productId)
        {
            var response = new ResponseModel<ProductDto>();
            try
            {
                var user = await _userService.GetActiveUser(userId);
                if (user == null)
                {
                    return response.Unsuccessful("User not found.");
                }

                var product = await GetActiveProduct(productId);
                if (product == null)
                {
                    return response.Unsuccessful("Product not found.");
                }

                if (await GetBoughtUserId(product.Id) != user.Id)
                {
                    return response.Unsuccessful("You didn't buy this product.");
                }

                product.BoughtByUser = null;
                product.BoughtDateTime = null;
                await _dbContext.SaveChangesAsync();

                response.Data = _mapper.Map<ProductDto>(product);
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

        public async Task<ResponseModel<ProductDto>> Update(long productId, ProductCreateDto productDto)
        {
            var response = new ResponseModel<ProductDto>();
            try
            {
                var product = await GetActiveProduct(productId);
                if (product == null)
                {
                    return response.Unsuccessful("Product not found.");
                }

                if (await GetBoughtUserId(product.Id) != null)
                {
                    return response.Unsuccessful("Can't update a product that had already been bought.");
                }

                product.Name = productDto.Name;
                product.Price = productDto.Price;
                product.IsShared = productDto.IsShared;

                await _dbContext.SaveChangesAsync();

                response.Data = await _mapper.ProjectToAsync<Product, ProductDto>(_dbContext.Products, product);
                return response;
            }
            catch (Exception)
            {
                return response.Exception();
            }
        }

        private async Task<Product> GetActiveProduct(long id)
        {
            return await _dbContext.Products.Active().SingleOrDefaultAsync(p => p.Id == id);
        }

        public async Task<long> GetAddedByUserId(long productId)
        {
            return await _dbContext.Products.Where(p => p.Id == productId).Select(p => p.AddedByUser.Id).SingleAsync();
        }

        private async Task<long?> GetBoughtUserId(long productId)
        {
            var id = await _dbContext.Products.Where(p => p.Id == productId && p.BoughtByUser != null)
                .Select(p => p.BoughtByUser.Id).SingleOrDefaultAsync();
            return id == 0 ? null : id;
        }
    }
}