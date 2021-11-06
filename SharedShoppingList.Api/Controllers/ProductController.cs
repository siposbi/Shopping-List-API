using System.Collections.Generic;
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
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IUserService _userService;

        public ProductController(IProductService productService, IUserService userService)
        {
            _productService = productService;
            _userService = userService;
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<ResponseModel<ProductMinDto>>> GetOne([FromRoute] long id)
        {
            var listId = await _productService.GetListIdForProduct(id);
            if (listId == null)
            {
                var response = new ResponseModel<bool>().Unsuccessful("Product does not exist.");
                return BadRequest(response);
            }

            if (!await _userService.UserIsMemberOfList(listId.Value, User.GetId()))
                return Unauthorized("User is not part of list, so can't get products of it.");

            var result = await _productService.Get(id);
            if (result.IsSuccess) return Ok(result);

            return BadRequest(result);
        }

        [HttpPost("create")]
        public async Task<ActionResult<ResponseModel<ProductMinDto>>> CreateProduct(
            [FromBody] ProductCreateModel productModel)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (!await _userService.UserIsMemberOfList(productModel.ShoppingListId, User.GetId()))
                return Unauthorized("User is not part of list, so can't add product to it.");

            var result = await _productService.Create(User.GetId(), productModel);
            if (result.IsSuccess) return Ok(result);

            return BadRequest(result);
        }

        [HttpGet("getAllForList/{listId:long}")]
        public async Task<ActionResult<ResponseModel<IEnumerable<ProductMinDto>>>> GetProductsOfShoppingList(
            [FromRoute] long listId)
        {
            if (!await _userService.UserIsMemberOfList(listId, User.GetId()))
                return Unauthorized("User is not part of list.");

            var result = await _productService.GetEveryProductForList(listId);
            if (result.IsSuccess) return Ok(result);

            return BadRequest(result);
        }

        [HttpDelete("delete/{productId:long}")]
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
                var response =
                    new ResponseModel<bool>().Unsuccessful("You cant delete a product added by someone else.");
                return BadRequest(response);
            }

            var result = await _productService.Delete(productId);
            if (result.IsSuccess) return Ok(result);

            return BadRequest(result);
        }

        [HttpPut("undoDelete/{productId:long}")]
        public async Task<ActionResult<ResponseModel<ProductMinDto>>> UndoDelete([FromRoute] long productId)
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
            if (result.IsSuccess) return Ok(result);

            return BadRequest(result);
        }

        [HttpPut("buy/{productId:long}")]
        public async Task<ActionResult<ResponseModel<ProductMinDto>>> Buy([FromRoute] long productId)
        {
            var result = await _productService.Buy(User.GetId(), productId);
            if (result.IsSuccess) return Ok(result);

            return BadRequest(result);
        }

        [HttpPut("undoBuy/{productId:long}")]
        public async Task<ActionResult<ResponseModel<ProductMinDto>>> UndoBuy([FromRoute] long productId)
        {
            var result = await _productService.UndoBuy(User.GetId(), productId);
            if (result.IsSuccess) return Ok(result);

            return BadRequest(result);
        }

        [HttpPut("update/{productId:long}")]
        public async Task<ActionResult<ResponseModel<ProductMinDto>>> Update([FromRoute] long productId,
            [FromBody] ProductUpdateModel productModel)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var listId = await _productService.GetListIdForProduct(productId);
            if (listId == null)
            {
                var response = new ResponseModel<bool>().Unsuccessful("Product does not exist.");
                return BadRequest(response);
            }

            if (!await _userService.UserIsMemberOfList(listId.Value, User.GetId()))
                return Unauthorized("User is not part of list.");
            var result = await _productService.Update(productId, productModel);
            if (result.IsSuccess) return Ok(result);

            return BadRequest(result);
        }
    }
}