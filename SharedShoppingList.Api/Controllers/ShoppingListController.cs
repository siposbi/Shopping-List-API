using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedShoppingList.Api.Extensions;
using SharedShoppingList.data.Dto;
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
            var result = await _shoppingListService.GetAllForUser(User.GetId());
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPost("create")]
        public async Task<ActionResult<ResponseModel<ShoppingListDto>>> CreateShoppingList([FromBody] string listName)
        {
            var result = await _shoppingListService.Create(User.GetId(), listName);
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("join/{shareCode}")]
        public async Task<ActionResult<ResponseModel<ShoppingListDto>>> Join([FromRoute] string shareCode)
        {
            var result = await _shoppingListService.Join(User.GetId(), shareCode);
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("leave/{listId:int}")]
        public async Task<ActionResult<ResponseModel<bool>>> Leave([FromRoute] long listId)
        {
            var result = await _shoppingListService.Leave(User.GetId(), listId);
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("rename/{listId:int}")]
        public async Task<ActionResult<ResponseModel<ShoppingListDto>>> Rename([FromRoute] int listId,
            [FromBody] string newName)
        {
            if (!await _userService.UserIsMemberOfList(listId, User.GetId()))
                return Unauthorized("User is not part of list, so can't rename it.");

            var result = await _shoppingListService.Rename(listId, newName);
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpGet("getMembers/{listId:int}")]
        public async Task<ActionResult<ResponseModel<IEnumerable<MemberDto>>>> GetMembersOfList([FromRoute] int listId)
        {
            if (!await _userService.UserIsMemberOfList(listId, User.GetId()))
                return Unauthorized("User is not part of list, so he can not see members.");

            var result = await _shoppingListService.GetMembers(listId);
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpGet("getExport/{listId:int}")]
        public async Task<ActionResult<ResponseModel<IEnumerable<ExportDto>>>> GetMembersOfList([FromRoute] int listId,
            [FromQuery, Required] DateTime startDate, [FromQuery, Required] DateTime endDatetime)
        {
            if (!await _userService.UserIsMemberOfList(listId, User.GetId()))
                return Unauthorized("User is not part of list, so he can not see members.");

            var result = await _shoppingListService.Export(listId, startDate, endDatetime);
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
    }
}