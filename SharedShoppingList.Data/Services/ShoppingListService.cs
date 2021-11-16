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
    public interface IShoppingListService
    {
        Task<ResponseModel<ShoppingListDto>> Create(long userId, string shoppingListName);
        Task<ResponseModel<IEnumerable<ShoppingListDto>>> GetAllForUser(long userId);
        Task<ResponseModel<ShoppingListDto>> Join(long userId, string shareCode);
        Task<ResponseModel<long>> Leave(long userId, long shoppingListId);
        Task<ResponseModel<ShoppingListDto>> Rename(long shoppingListId, string newName);
        Task<ResponseModel<IEnumerable<MemberDto>>> GetMembers(long shoppingListId);

        Task<ResponseModel<IEnumerable<ExportDto>>> Export(long shoppingListId, DateTime startDateTime,
            DateTime endDateTime);
    }

    public class ShoppingListService : IShoppingListService
    {
        private readonly ICommonService _commonService;

        private readonly ShoppingListDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly Random _random;
        private readonly IUserService _userService;

        public ShoppingListService(ShoppingListDbContext dbContext, ICommonService commonService,
            IUserService userService, IMapper mapper)
        {
            _dbContext = dbContext;
            _commonService = commonService;
            _userService = userService;
            _mapper = mapper;
            _random = new Random();
        }

        public async Task<ResponseModel<ShoppingListDto>> Create(long userId, string shoppingListName)
        {
            var response = new ResponseModel<ShoppingListDto>();
            try
            {
                var user = await _userService.GetActiveUser(userId);
                if (user == null) return response.Unsuccessful("User not found.");

                if (shoppingListName.Length > 20) return response.Unsuccessful("Name cant be more than 20 characters.");

                var newShoppingList = new ShoppingList
                {
                    Name = shoppingListName,
                    CreatedByUser = user
                };
                await _dbContext.ShoppingLists.AddAsync(newShoppingList);
                var newUserShoppingList = new UserShoppingList
                {
                    User = user,
                    ShoppingList = newShoppingList
                };
                await _dbContext.AddAsync(newUserShoppingList);
                await _dbContext.SaveChangesAsync();
                newShoppingList.ShareCode =
                    $"SSLU{userId.ToString().PadLeft(5, '0')}L{newShoppingList.Id.ToString().PadLeft(5, '0')}R{RandomString(5)}";
                await _dbContext.SaveChangesAsync();
                response.Data =
                    await _mapper.ProjectToAsync<ShoppingList, ShoppingListDto>(_dbContext.ShoppingLists,
                        newShoppingList);
                return response;
            }
            catch (Exception)
            {
                return response.Exception();
            }
        }

        public async Task<ResponseModel<IEnumerable<ShoppingListDto>>> GetAllForUser(long userId)
        {
            var response = new ResponseModel<IEnumerable<ShoppingListDto>>();
            try
            {
                var user = await _userService.GetActiveUser(userId);
                if (user == null) return response.Unsuccessful("User not found.");

                var shoppingLists = await _dbContext.UserShoppingLists.Active()
                    .Where(sl => sl.UserId == userId)
                    .Include(usl => usl.ShoppingList.Products)
                    .Select(usl => usl.ShoppingList)
                    .ProjectTo<ShoppingListDto>(_mapper.ConfigurationProvider)
                    .ToListAsync();

                foreach (var shoppingList in shoppingLists)
                {
                    shoppingList.LastProductAddedDateTime =
                        await _dbContext.Products.Where(p => p.ShoppingList.Id == shoppingList.Id)
                            .Select(p => p.CreatedDateTime).DefaultIfEmpty().MaxAsync();
                }

                response.Data = shoppingLists.OrderByDescending(sl => sl.LastProductAddedDateTime);
                return response;
            }
            catch (Exception)
            {
                return response.Exception();
            }
        }

        public async Task<ResponseModel<ShoppingListDto>> Join(long userId, string shareCode)
        {
            var response = new ResponseModel<ShoppingListDto>();
            try
            {
                var user = await _userService.GetActiveUser(userId);
                if (user == null) return response.Unsuccessful("User not found.");

                var shoppingList = await _dbContext.ShoppingLists.SingleOrDefaultAsync(sl => sl.ShareCode == shareCode);
                if (shoppingList == null) return response.Unsuccessful("Shopping List not found.");

                if (await _userService.UserIsMemberOfList(shoppingList.Id, user.Id))
                    return response.Unsuccessful("User is already part of this list.");

                if (shoppingList.IsActive == false) shoppingList.IsActive = true;

                var usl = await _dbContext.UserShoppingLists.SingleOrDefaultAsync(usl =>
                    usl.User == user && usl.ShoppingList == shoppingList);
                if (usl == null)
                    await _dbContext.UserShoppingLists.AddAsync(new UserShoppingList
                        { User = user, ShoppingList = shoppingList });
                else
                    usl.IsActive = true;

                await _dbContext.SaveChangesAsync();

                response.Data =
                    await _mapper.ProjectToAsync<ShoppingList, ShoppingListDto>(_dbContext.ShoppingLists, shoppingList);
                return response;
            }
            catch (Exception)
            {
                return response.Exception();
            }
        }

        public async Task<ResponseModel<long>> Leave(long userId, long shoppingListId)
        {
            var response = new ResponseModel<long>();
            try
            {
                var user = await _userService.GetActiveUser(userId);
                if (user == null) return response.Unsuccessful("User not found.");

                var shoppingList = await _commonService.GetActiveShoppingList(shoppingListId);
                if (shoppingList == null) return response.Unsuccessful("Shopping List not found.");

                var userShoppingList = await _dbContext.UserShoppingLists.Active()
                    .SingleOrDefaultAsync(usl => usl.ShoppingList == shoppingList && usl.User == user);
                if (userShoppingList == null) return response.Unsuccessful("User is not member of this list.");

                userShoppingList.IsActive = false;
                await _dbContext.SaveChangesAsync();

                var isAnyoneStillOnList = _dbContext.UserShoppingLists.Active()
                    .Any(sl => sl.ShoppingListId == shoppingList.Id);
                if (!isAnyoneStillOnList) shoppingList.IsActive = false;

                await _dbContext.SaveChangesAsync();

                response.Data = shoppingListId;
                return response;
            }
            catch (Exception)
            {
                return response.Exception();
            }
        }

        public async Task<ResponseModel<ShoppingListDto>> Rename(long shoppingListId, string newName)
        {
            var response = new ResponseModel<ShoppingListDto>();
            try
            {
                var shoppingList = await _commonService.GetActiveShoppingList(shoppingListId);
                if (shoppingList == null) return response.Unsuccessful("Shopping List not found.");

                if (newName.Length > 20) return response.Unsuccessful("Name cant be more than 20 characters.");

                shoppingList.Name = newName;
                await _dbContext.SaveChangesAsync();

                response.Data =
                    await _mapper.ProjectToAsync<ShoppingList, ShoppingListDto>(_dbContext.ShoppingLists, shoppingList);
                return response;
            }
            catch (Exception)
            {
                return response.Exception();
            }
        }

        public async Task<ResponseModel<IEnumerable<MemberDto>>> GetMembers(long shoppingListId)
        {
            var response = new ResponseModel<IEnumerable<MemberDto>>();
            try
            {
                var shoppingList = await _commonService.GetActiveShoppingList(shoppingListId);
                if (shoppingList == null) return response.Unsuccessful("Shopping List not found.");

                var memberList = await _dbContext.UserShoppingLists.Active()
                    .Where(sl => sl.ShoppingList == shoppingList)
                    .OrderByDescending(usl => usl.User.FirstName)
                    .ThenBy(usl => usl.User.LastName)
                    .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                    .ToListAsync();
                response.Data = memberList;
                return response;
            }
            catch (Exception)
            {
                return response.Exception();
            }
        }

        // TODO finish this mess
        public async Task<ResponseModel<IEnumerable<ExportDto>>> Export(long shoppingListId, DateTime startDateTime,
            DateTime endDateTime)
        {
            var response = new ResponseModel<IEnumerable<ExportDto>>();
            try
            {
                var shoppingList = await _commonService.GetActiveShoppingList(shoppingListId);
                if (shoppingList == null) return response.Unsuccessful("Shopping List not found.");

                var start = startDateTime.Date;
                var end = endDateTime.Date.AddDays(1).AddTicks(-1);

                // TODO: Megnézni, hogy mennyi értékben vettek meg neki dolgokat és, hogy mennyi értékben vett meg ő dolgokat, ami nem az övé
                // foglalkozni azzal, hogy a termékeke lehetnek közösek is

                var exportDtoList = await _dbContext.UserShoppingLists.Active()
                    .Where(sl => sl.ShoppingList == shoppingList)
                    .Select(sl => new ExportDto(sl.User.FirstName, sl.User.LastName,
                        // Products, that he added, and others have bought
                        _dbContext.Products.Active().BoughtBetween(start, end).Where(p =>
                                p.AddedByUser == sl.User && p.BoughtByUser != null && p.BoughtByUser != sl.User)
                            .Sum(p => p.Price)
                        -
                        // Products, that he bought for others
                        _dbContext.Products.Active().BoughtBetween(start, end)
                            .Where(p => p.BoughtByUser == sl.User && p.AddedByUser != sl.User).Sum(p => p.Price)
                    )).ToListAsync();
                // TODO use mapper
                var exportDtoListOrdered = exportDtoList.OrderBy(m => m.FirstName);
                response.Data = exportDtoListOrdered;
                return response;
            }
            catch (Exception)
            {
                return response.Exception();
            }
        }

        private string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }
    }
}