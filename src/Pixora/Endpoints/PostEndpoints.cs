using MinimalHelpers.FluentValidation;
using MinimalHelpers.Routing;
using OperationResults.AspNetCore.Http;
using Pixora.BusinessLayer.Services.Interfaces;
using Pixora.Shared.Models;
using Pixora.Shared.Models.Requests;

namespace Pixora.Endpoints;

public class PostEndpoints : IEndpointRouteHandlerBuilder
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var postApiGroup = endpoints.MapGroup("/api/posts").WithTags("Posts");

        postApiGroup.MapPost(string.Empty, CreateAsync)
            .RequireAuthorization()
            .WithValidation<SavePostRequest>()
            .Produces<Post>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("CreatePost");

        postApiGroup.MapGet("{id:guid}", GetAsync)
            .AllowAnonymous()
            .Produces<Post>()
            .Produces(StatusCodes.Status404NotFound)
            .WithName("GetPost");

        postApiGroup.MapGet(string.Empty, GetListAsync)
            .AllowAnonymous()
            .Produces<IEnumerable<Post>>()
            .WithName("GetPosts");

        postApiGroup.MapPut("{id:guid}", UpdateAsync)
            .RequireAuthorization()
            .WithValidation<SavePostRequest>()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("UpdatePost");

        postApiGroup.MapDelete("{id:guid}", DeleteAsync)
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("DeletePost");
    }

    private static async Task<IResult> CreateAsync(SavePostRequest request, IPostService postService, HttpContext httpContext)
    {
        var result = await postService.CreateAsync(request, httpContext.RequestAborted);

        var response = httpContext.CreateResponse(result, "GetPost", new { result.Content?.Id });
        return response;
    }

    private static async Task<IResult> GetAsync(Guid id, IPostService postService, HttpContext httpContext)
    {
        var result = await postService.GetAsync(id, httpContext.RequestAborted);

        var response = httpContext.CreateResponse(result);
        return response;
    }

    private static async Task<IResult> GetListAsync(IPostService postService, HttpContext httpContext)
    {
        var result = await postService.GetListAsync(httpContext.RequestAborted);

        var response = httpContext.CreateResponse(result);
        return response;
    }

    private static async Task<IResult> UpdateAsync(Guid id, SavePostRequest request, IPostService postService, HttpContext httpContext)
    {
        var result = await postService.UpdateAsync(id, request, httpContext.RequestAborted);

        var response = httpContext.CreateResponse(result);
        return response;
    }

    private static async Task<IResult> DeleteAsync(Guid id, IPostService postService, HttpContext httpContext)
    {
        var result = await postService.DeleteAsync(id, httpContext.RequestAborted);

        var response = httpContext.CreateResponse(result);
        return response;
    }
}