using MinimalHelpers.Routing;
using Pixora.BusinessLayer.Handlers.Interfaces;

namespace Pixora.Endpoints;

public class WebSocketEndpoints : IEndpointRouteHandlerBuilder
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.Map("/ws/chat", HandleSocketAsync)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithName("HandleSocket")
            .WithTags("Socket");
    }

    private static async Task<IResult> HandleSocketAsync(IChatWebSocketHandler handler, HttpContext httpContext)
    {
        if (!httpContext.WebSockets.IsWebSocketRequest)
        {
            return TypedResults.BadRequest("Not a web socket");
        }

        try
        {
            var socket = await httpContext.WebSockets.AcceptWebSocketAsync();
            await handler.HandleSocketAsync(socket, httpContext, httpContext.RequestAborted);

            return TypedResults.NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return TypedResults.Unauthorized();
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }
}