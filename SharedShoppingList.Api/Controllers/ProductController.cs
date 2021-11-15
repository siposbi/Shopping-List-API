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
            if (listId == null) return Ok(new ResponseModel<bool>().Unsuccessful("Product does not exist."));

            if (!await _userService.UserIsMemberOfList(listId.Value, User.GetId()))
                return Unauthorized("User is not part of list, so can't get products of it.");

            return Ok(await _productService.Get(id));
        }

        [HttpPost("create")]
        public async Task<ActionResult<ResponseModel<ProductMinDto>>> CreateProduct(
            [FromBody] ProductCreateModel productModel)
        {
            if (!await _userService.UserIsMemberOfList(productModel.ShoppingListId, User.GetId()))
                return Unauthorized("User is not part of list, so can't add product to it.");

            return Ok(await _productService.Create(User.GetId(), productModel));
        }

        [HttpGet("getAllForList/{listId:long}")]
        public async Task<ActionResult<ResponseModel<IEnumerable<ProductMinDto>>>> GetProductsOfShoppingList(
            [FromRoute] long listId)
        {
            if (!await _userService.UserIsMemberOfList(listId, User.GetId()))
                return Unauthorized("User is not part of list.");

            return Ok(await _productService.GetEveryProductForList(listId));
        }

        [HttpDelete("delete/{productId:long}")]
        public async Task<ActionResult<ResponseModel<long>>> Delete([FromRoute] long productId)
        {
            var listId = await _productService.GetListIdForProduct(productId);
            if (listId == null) return Ok(new ResponseModel<long>().Unsuccessful("Product does not exist."));

            if (!await _userService.UserIsMemberOfList(listId.Value, User.GetId()))
                return Unauthorized("User is not part of list.");
            if (User.GetId() != await _productService.GetAddedByUserId(productId))
                return Ok(new ResponseModel<long>().Unsuccessful("You cant delete a product added by someone else."));

            return Ok(await _productService.Delete(productId));
        }

        [HttpPut("undoDelete/{productId:long}")]
        public async Task<ActionResult<ResponseModel<ProductMinDto>>> UndoDelete([FromRoute] long productId)
        {
            var listId = await _productService.GetListIdForProduct(productId);
            if (listId == null) return Ok(new ResponseModel<bool>().Unsuccessful("Product does not exist."));

            if (!await _userService.UserIsMemberOfList(listId.Value, User.GetId()))
                return Unauthorized("User is not part of list.");

            return Ok(await _productService.UndoDelete(productId));
        }

        [HttpPut("buy/{productId:long}")]
        public async Task<ActionResult<ResponseModel<ProductMinDto>>> Buy([FromRoute] long productId,
            [FromBody] long price)
        {
            return Ok(await _productService.Buy(User.GetId(), productId, price));
        }

        [HttpPut("undoBuy/{productId:long}")]
        public async Task<ActionResult<ResponseModel<ProductMinDto>>> UndoBuy([FromRoute] long productId)
        {
            return Ok(await _productService.UndoBuy(User.GetId(), productId));
        }

        [HttpPut("update/{productId:long}")]
        public async Task<ActionResult<ResponseModel<ProductMinDto>>> Update([FromRoute] long productId,
            [FromBody] ProductUpdateModel productModel)
        {
            var listId = await _productService.GetListIdForProduct(productId);
            if (listId == null) return Ok(new ResponseModel<bool>().Unsuccessful("Product does not exist."));

            if (!await _userService.UserIsMemberOfList(listId.Value, User.GetId()))
                return Unauthorized("User is not part of list.");

            return Ok(await _productService.Update(productId, productModel));
        }
    }
}