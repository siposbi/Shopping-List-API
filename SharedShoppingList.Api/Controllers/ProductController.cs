using System.Collections.Generic;
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
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IUserService _userService;

        public ProductController(IProductService productService, IUserService userService)
        {
            _productService = productService;
            _userService = userService;
        }

        [HttpPost("create")]
        public async Task<ActionResult<ResponseModel<ProductDto>>> CreateProduct([FromBody] ProductCreateDto productDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!await _userService.UserIsMemberOfList(productDto.ShoppingListId, User.GetId()))
                return Unauthorized("User is not part of list, so can't add product to it.");

            var result = await _productService.Create(User.GetId(), productDto);
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpGet("getAllForList/{listId:int}")]
        public async Task<ActionResult<ResponseModel<IEnumerable<ProductDto>>>> GetProductsOfShoppingList(
            [FromRoute] long listId)
        {
            if (!await _userService.UserIsMemberOfList(listId, User.GetId()))
                return Unauthorized("User is not part of list.");

            var result = await _productService.GetEveryProductForList(listId);
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpDelete("delete/{productId:int}")]
        public async Task<ActionResult<ResponseModel<bool>>> Delete([FromRoute] long productId)
        {
            var listId = await _productService.GetListIdForProduct(productId);
            if (listId == null)
            {
                var response = new ResponseModel<bool>().Unsuccessful("Product does not exist.");
                return BadRequest(response);
            }
            if (!await _userService.UserIsMemberOfList(listId.Value, User.GetId()))
                return Unauthorized("User is not part of list.");
            if (User.GetId() != await _productService.GetAddedByUserId(productId))
            {
                var response = new ResponseModel<bool>().Unsuccessful("You cant delete a product added by someone else.");
                return BadRequest(response);
            }

            var result = await _productService.Delete(productId);
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("undoDelete/{productId:int}")]
        public async Task<ActionResult<ResponseModel<ProductDto>>> UndoDelete([FromRoute] long productId)
        {
            var listId = await _productService.GetListIdForProduct(productId);
            if (listId == null)
            {
                var response = new ResponseModel<bool>().Unsuccessful("Product does not exist.");
                return BadRequest(response);
            }
            if (!await _userService.UserIsMemberOfList(listId.Value, User.GetId()))
                return Unauthorized("User is not part of list.");

            var result = await _productService.UndoDelete(productId);
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("buy/{productId:int}")]
        public async Task<ActionResult<ResponseModel<ProductDto>>> Buy([FromRoute] long productId)
        {
            var result = await _productService.Buy(User.GetId(), productId);
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("undoBuy/{productId:int}")]
        public async Task<ActionResult<ResponseModel<ProductDto>>> UndoBuy([FromRoute] long productId)
        {
            var result = await _productService.UndoBuy(User.GetId(), productId);
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("update/{productId:int}")]
        public async Task<ActionResult<ResponseModel<ProductDto>>> Update([FromRoute] long productId, [FromBody] ProductCreateDto productDto)
        {
            var listId = await _productService.GetListIdForProduct(productId);
            if (listId == null)
            {
                var response = new ResponseModel<bool>().Unsuccessful("Product does not exist.");
                return BadRequest(response);
            }
            if (!await _userService.UserIsMemberOfList(listId.Value, User.GetId()))
                return Unauthorized("User is not part of list.");
            var result = await _productService.Update(productId, productDto);
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
    }
}