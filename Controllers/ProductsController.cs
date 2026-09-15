using Microsoft.AspNetCore.Mvc;
using ShopingCart.BusinessLogic;
using ShopingCart.DataContract.Models;

namespace ShopingCart.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(ProductService products) : ControllerBase
{
    [HttpGet]
    public ActionResult<List<Product>> GetAll() => products.GetAll();
}
