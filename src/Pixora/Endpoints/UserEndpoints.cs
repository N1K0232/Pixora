using System.Net.Mime;
using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MinimalHelpers.Routing;
using OperationResults.AspNetCore.Http;
using Pixora.Authentication.Extensions;
using Pixora.BusinessLayer.Services.Interfaces;
using Pixora.Shared.Models;
using TinyHelpers.AspNetCore.DataAnnotations;

namespace Pixora.Endpoints;

public class UserEndpoints : IEndpointRouteHandlerBuilder
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var userApiGroup = endpoints.MapGroup("/api/me").WithTags("User").RequireAuthorization();

        userApiGroup.MapPost("profilephoto", UploadProfilePhotoAsync)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .DisableAntiforgery()
            .WithName("uploadprofilephoto");

        userApiGroup.MapGet("profilephoto", GetProfilePhotoAsync)
            .Produces(StatusCodes.Status200OK, contentType: MediaTypeNames.Image.Jpeg)
            .Produces(StatusCodes.Status200OK, contentType: MediaTypeNames.Image.Png)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("getprofilephoto");

        userApiGroup.MapDelete("profilephoto", DeleteProfilePhotoAsync)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("deleteprofilephoto");

        userApiGroup.MapGet(string.Empty, GetMe)
            .RequireAuthorization()
            .Produces<User>()
            .WithName("me");
    }

    private static Ok<User> GetMe(ClaimsPrincipal principal)
    {
        var user = new User
        {
            Id = principal.GetId(),
            FirstName = principal.GetFirstName(),
            LastName = principal.GetLastName(),
            Email = principal.GetEmail(),
            UserName = principal.Identity?.Name ?? string.Empty,
            Roles = principal.GetUserRoles()
        };

        return TypedResults.Ok(user);
    }

    private static async Task<IResult> UploadProfilePhotoAsync([BindRequired, AllowedExtensions("*.jpg", "*.jpeg", "*.png")] IFormFile file, IUserService userService, HttpContext httpContext)
    {
        var result = await userService.UploadProfilePhotoAsync(file, httpContext.User, httpContext.RequestAborted);

        var response = httpContext.CreateResponse(result);
        return response;
    }

    private static async Task<IResult> GetProfilePhotoAsync(IUserService userService, HttpContext httpContext)
    {
        var result = await userService.GetProfilePhotoAsync(httpContext.User, httpContext.RequestAborted);

        var response = httpContext.CreateResponse(result);
        return response;
    }

    private static async Task<IResult> DeleteProfilePhotoAsync(IUserService userService, HttpContext httpContext)
    {
        var result = await userService.DeleteProfilePhotoAsync(httpContext.User, httpContext.RequestAborted);

        var response = httpContext.CreateResponse(result);
        return response;
    }
}