using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedShoppingList.Api.Extensions;
using SharedShoppingList.Data.Dto;
using SharedShoppingList.Data.Models;
using SharedShoppingList.Data.Services;

namespace SharedShoppingList.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ShoppingListController : ControllerBase
    {
        private readonly IShoppingListService _shoppingListService;
        private readonly IUserService _userService;

        public ShoppingListController(IShoppingListService shoppingListService, IUserService userService)
        {
            _shoppingListService = shoppingListService;
            _userService = userService;
        }

        [HttpGet("getAllForUser")]
        public async Task<ActionResult<IEnumerable<ShoppingListDto>>> GetShoppingListsForUser()
        {
            return Ok(await _shoppingListService.GetAllForUser(User.GetId()));
        }

        [HttpPost("create")]
        public async Task<ActionResult<ResponseModel<ShoppingListDto>>> CreateShoppingList([FromBody] string listName)
        {
            return Ok(await _shoppingListService.Create(User.GetId(), listName));
        }

        [HttpPut("join/{shareCode}")]
        public async Task<ActionResult<ResponseModel<ShoppingListDto>>> Join([FromRoute] string shareCode)
        {
            return Ok(await _shoppingListService.Join(User.GetId(), shareCode));
        }

        [HttpPut("leave/{listId:long}")]
        public async Task<ActionResult<ResponseModel<bool>>> Leave([FromRoute] long listId)
        {
            return Ok(await _shoppingListService.Leave(User.GetId(), listId));
        }

        [HttpPut("rename/{listId:long}")]
        public async Task<ActionResult<ResponseModel<ShoppingListDto>>> Rename([FromRoute] int listId,
            [FromBody] string newName)
        {
            if (!await _userService.UserIsMemberOfList(listId, User.GetId()))
                return Unauthorized("User is not part of list, so can't rename it.");

            return Ok(await _shoppingListService.Rename(listId, newName));
        }

        [HttpGet("getMembers/{listId:long}")]
        public async Task<ActionResult<ResponseModel<IEnumerable<MemberDto>>>> GetMembersOfList([FromRoute] int listId)
        {
            if (!await _userService.UserIsMemberOfList(listId, User.GetId()))
                return Unauthorized("User is not part of list, so he can not see members.");

            return Ok(await _shoppingListService.GetMembers(listId));
        }

        [HttpGet("getExport/{listId:long}")]
        public async Task<ActionResult<ResponseModel<IEnumerable<ExportDto>>>> GetMembersOfList([FromRoute] int listId,
            [FromQuery] [Required] DateTime startDate, [FromQuery] [Required] DateTime endDatetime)
        {
            if (!await _userService.UserIsMemberOfList(listId, User.GetId()))
                return Unauthorized("User is not part of list, so he can not see members.");

            return Ok(await _shoppingListService.Export(listId, startDate, endDatetime));
        }
    }
}