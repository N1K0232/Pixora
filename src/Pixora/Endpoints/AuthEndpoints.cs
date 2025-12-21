using System.Net.Mime;
using MinimalHelpers.FluentValidation;
using MinimalHelpers.Routing;
using OperationResults.AspNetCore.Http;
using Pixora.BusinessLayer.Services.Interfaces;
using Pixora.Shared.Models;
using Pixora.Shared.Models.Requests;

namespace Pixora.Endpoints;

public class AuthEndpoints : IEndpointRouteHandlerBuilder
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var authApiGroup = endpoints.MapGroup("/api/auth").AllowAnonymous().WithTags("Auth");

        authApiGroup.MapGet("confirm", ConfirmEmailAsync)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("confirm");

        authApiGroup.MapPost("forgotpassword", ForgotPasswordAsync)
            .Produces<ForgotPasswordResponse>()
            .Produces(StatusCodes.Status400BadRequest)
            .WithValidation<ForgotPasswordRequest>()
            .WithName("forgotpassword");

        authApiGroup.MapGet("qrcode", GetQrCodeAsync)
            .Produces(StatusCodes.Status200OK, contentType: MediaTypeNames.Image.Png)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("qrcode");

        authApiGroup.MapPost("login", LoginAsync)
            .Produces<AuthResponse>()
            .Produces(StatusCodes.Status400BadRequest)
            .WithValidation<LoginRequest>()
            .WithName("login");

        authApiGroup.MapPost("logout", LogoutAsync)
            .Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization()
            .WithName("logout");

        authApiGroup.MapPost("register", RegisterAsync)
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithValidation<RegisterRequest>()
            .WithName("register");

        authApiGroup.MapPost("refresh", RefreshTokenAsync)
            .Produces<AuthResponse>()
            .Produces(StatusCodes.Status400BadRequest)
            .WithValidation<RefreshTokenRequest>()
            .WithName("refresh");

        authApiGroup.MapPost("resetpassword", ResetPasswordAsync)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .WithValidation<ResetPasswordRequest>()
            .WithName("resetpassword");

        authApiGroup.MapPost("validate2fa", ValidateTwoFactorAsync)
            .Produces<AuthResponse>()
            .Produces(StatusCodes.Status400BadRequest)
            .WithValidation<TwoFactorValidationRequest>()
            .WithName("validate2fa");
    }

    private static async Task<IResult> ConfirmEmailAsync(string secret, string token, IIdentityService identityService, HttpContext httpContext)
    {
        var result = await identityService.ConfirmEmailAsync(secret, token, httpContext.RequestAborted);

        var response = httpContext.CreateResponse(result);
        return response;
    }

    private static async Task<IResult> ForgotPasswordAsync(ForgotPasswordRequest request, IIdentityService identityService, HttpContext httpContext)
    {
        var result = await identityService.ForgotPasswordAsync(request, httpContext.RequestAborted);

        var response = httpContext.CreateResponse(result);
        return response;
    }

    private static async Task<IResult> GetQrCodeAsync(string token, IIdentityService identityService, HttpContext httpContext)
    {
        var result = await identityService.GetQrCodeAsync(token, httpContext.RequestAborted);

        var response = httpContext.CreateResponse(result);
        return response;
    }

    private static async Task<IResult> LoginAsync(LoginRequest request, IIdentityService identityService, HttpContext httpContext)
    {
        var result = await identityService.LoginAsync(request, httpContext.RequestAborted);

        var response = httpContext.CreateResponse(result);
        return response;
    }

    private static async Task<IResult> LogoutAsync(IIdentityService identityService, HttpContext httpContext)
    {
        var result = await identityService.LogoutAsync(httpContext.RequestAborted);

        var response = httpContext.CreateResponse(result);
        return response;
    }

    private static async Task<IResult> RegisterAsync(RegisterRequest request, IIdentityService identityService, HttpContext httpContext)
    {
        var result = await identityService.RegisterAsync(request, httpContext.RequestAborted);

        var response = httpContext.CreateResponse(result, StatusCodes.Status201Created);
        return response;
    }

    private static async Task<IResult> RefreshTokenAsync(RefreshTokenRequest request, IIdentityService identityService, HttpContext httpContext)
    {
        var result = await identityService.RefreshTokenAsync(request, httpContext.RequestAborted);

        var response = httpContext.CreateResponse(result);
        return response;
    }

    private static async Task<IResult> ResetPasswordAsync(ResetPasswordRequest request, IIdentityService identityService, HttpContext httpContext)
    {
        var result = await identityService.ResetPasswordAsync(request, httpContext.RequestAborted);

        var response = httpContext.CreateResponse(result);
        return response;
    }

    private static async Task<IResult> ValidateTwoFactorAsync(TwoFactorValidationRequest request, IIdentityService identityService, HttpContext httpContext)
    {
        var result = await identityService.ValidateTwoFactorAsync(request, httpContext.RequestAborted);

        var response = httpContext.CreateResponse(result);
        return response;
    }
}