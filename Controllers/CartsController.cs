using Microsoft.AspNetCore.Mvc;
using ShopingCart.BusinessLogic;
using ShopingCart.DataContract.Models;
using ShopingCart.DataContract.Requests;

namespace ShopingCart.Api.Controllers;

[ApiController]
[Route("api/carts")]
public class CartsController(CartService carts, CheckoutService checkout) : ControllerBase
{
    [HttpPost]
    public ActionResult<Cart> Create()
    {
        var cart = carts.Create();
        return CreatedAtAction(nameof(Get), new { cartId = cart.Id }, cart);
    }
    [HttpGet("{cartId:guid}")]
    public ActionResult<Cart> Get(Guid cartId) => carts.Get(cartId.ToString());

    [HttpPost("{cartId:guid}/items")]
    public ActionResult<Cart> Add(Guid cartId, AddCartItemRequest request) =>
        carts.AddItem(cartId.ToString(), request.ProductId, request.Quantity);

    [HttpPut("{cartId:guid}/items/{productId:int:min(1)}")]
    public ActionResult<Cart> Update(Guid cartId, int productId, UpdateCartItemRequest request) =>
        carts.UpdateQuantity(cartId.ToString(), productId, request.Quantity!.Value);

    [HttpDelete("{cartId:guid}/items/{productId:int:min(1)}")]
    public ActionResult<Cart> Remove(Guid cartId, int productId) => carts.RemoveItem(cartId.ToString(), productId);

    [HttpDelete("{cartId:guid}/items")]
    public ActionResult<Cart> Clear(Guid cartId) => carts.Clear(cartId.ToString());

    [HttpPost("{cartId:guid}/checkout")]
    public ActionResult<Order> Checkout(Guid cartId, CheckoutRequest request) =>
        checkout.Checkout(cartId.ToString(), request.ExpectedTotalSatang!.Value);
}
