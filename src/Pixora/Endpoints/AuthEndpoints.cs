using System.Security.Claims;
using MinimalHelpers.FluentValidation;
using MinimalHelpers.Routing;
using OperationResults.AspNetCore.Http;
using Pixora.Authentication.Extensions;
using Pixora.BusinessLayer.Services.Interfaces;
using Pixora.Shared.Models;
using Pixora.Shared.Models.Requests;

namespace Pixora.Endpoints;

public class AuthEndpoints : IEndpointRouteHandlerBuilder
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var authApiGroup = endpoints.MapGroup("/api/auth").WithTags("Auth");

        authApiGroup.MapGet("confirm", ConfirmEmailAsync)
            .Produces<AuthResponse>()
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("confirm");

        authApiGroup.MapPost("login", LoginAsync)
            .Produces<AuthResponse>()
            .Produces(StatusCodes.Status400BadRequest)
            .WithValidation<LoginRequest>()
            .WithName("login");

        authApiGroup.MapPost("register", RegisterAsync)
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithValidation<RegisterRequest>()
            .WithName("register");

        authApiGroup.MapPost("refresh", RefreshTokeAsync)
            .Produces<AuthResponse>()
            .Produces(StatusCodes.Status400BadRequest)
            .WithValidation<RefreshTokenRequest>()
            .WithName("refresh");

        endpoints.MapGet("/api/me", (ClaimsPrincipal principal) =>
        {
            var user = new User
            {
                Id = principal.GetId(),
                FirstName = principal.GetFirstName(),
                LastName = principal.GetLastName(),
                Email = principal.GetEmail(),
                UserName = principal.Identity?.Name ?? string.Empty
            };

            return TypedResults.Ok(user);
        })
        .RequireAuthorization()
        .Produces<User>()
        .WithTags("Me")
        .WithName("me");
    }

    private static async Task<IResult> ConfirmEmailAsync(string secret, string token, IIdentityService identityService, HttpContext httpContext)
    {
        var result = await identityService.ConfirmEmailAsync(secret, token, httpContext.RequestAborted);

        var response = httpContext.CreateResponse(result);
        return response;
    }

    private static async Task<IResult> LoginAsync(LoginRequest request, IIdentityService identityService, HttpContext httpContext)
    {
        var result = await identityService.LoginAsync(request, httpContext.RequestAborted);

        var response = httpContext.CreateResponse(result);
        return response;
    }

    private static async Task<IResult> RegisterAsync(RegisterRequest request, IIdentityService identityService, HttpContext httpContext)
    {
        var result = await identityService.RegisterAsync(request, httpContext.RequestAborted);

        var response = httpContext.CreateResponse(result, StatusCodes.Status201Created);
        return response;
    }

    private static async Task<IResult> RefreshTokeAsync(RefreshTokenRequest request, IIdentityService identityService, HttpContext httpContext)
    {
        var result = await identityService.RefreshTokenAsync(request, httpContext.RequestAborted);

        var response = httpContext.CreateResponse(result);
        return response;
    }
}