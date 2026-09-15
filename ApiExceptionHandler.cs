using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using ShopingCart.BusinessLogic;

namespace ShopingCart.Api;

public class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, message) = exception switch
        {
            BusinessException business => (business.StatusCode, business.Message),
            SqliteException { SqliteErrorCode: 5 or 6 } => (409, "ระบบกำลังบันทึกคำสั่งซื้อ กรุณาลองอีกครั้ง"),
            _ => (500, "เกิดข้อผิดพลาดในระบบ กรุณาลองอีกครั้ง")
        };
        if (status == 500) logger.LogError(exception, "Unhandled API error");
        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new ProblemDetails { Status = status, Title = message },
            cancellationToken);
        return true;
    }
}
