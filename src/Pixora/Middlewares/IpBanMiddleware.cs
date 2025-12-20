using Pixora.BusinessLayer.Services.Interfaces;

namespace Pixora.Middlewares;

public class IpBanMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext httpContext, IIpBanService ipBanService)
    {
        var ip = httpContext.Connection?.LocalIpAddress?.ToString() ?? string.Empty;
        if (await ipBanService.IsBannedAsync(ip, httpContext.RequestAborted).ConfigureAwait(false))
        {
            httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
            await httpContext.Response.WriteAsync("Banned", httpContext.RequestAborted).ConfigureAwait(false);
        }

        await next.Invoke(httpContext).ConfigureAwait(false);
    }
}