namespace ShopingCart.BusinessLogic;

public class BusinessException(string message, int statusCode = 409) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}
