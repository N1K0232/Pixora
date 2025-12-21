using MinimalHelpers.Routing;
using Pixora.Authentication;
using Pixora.BusinessLayer.Services.Interfaces;
using Pixora.Shared.Models.Requests;

namespace Pixora.Endpoints;

public class AdminEndpoints : IEndpointRouteHandlerBuilder
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var ipApiGroup = endpoints.MapGroup("/api/ip").WithTags("IP").RequireAuthorization(RoleNames.Administrator);

        ipApiGroup.MapPost("ban", BanAsync)
            .Produces(StatusCodes.Status204NoContent)
            .WithName("Ban");

        ipApiGroup.MapDelete("unban/{id:guid}", UnbanAsync)
            .Produces(StatusCodes.Status204NoContent)
            .WithName("Unban");
    }

    private static async Task<IResult> BanAsync(IpBanRequest request, IIpBanService ipBanService, HttpContext httpContext)
    {
        await ipBanService.BanAsync(request.Ip, request.Reason, request.Duration, httpContext.RequestAborted);
        return TypedResults.NoContent();
    }

    private static async Task<IResult> UnbanAsync(Guid id, IIpBanService ipBanService, HttpContext httpContext)
    {
        await ipBanService.UnbanAsync(id, httpContext.RequestAborted);
        return TypedResults.NoContent();
    }
}