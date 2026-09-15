using ShopingCart.DataContract.Models;
using ShopingCart.Repository;

namespace ShopingCart.BusinessLogic;

public class ProductService(ProductRepository products)
{
    public List<Product> GetAll() => products.GetAll();
}
