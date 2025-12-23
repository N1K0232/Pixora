using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using MinimalHelpers.Routing;
using OperationResults.AspNetCore.Http;
using Pixora.BusinessLayer.Services.Interfaces;
using Pixora.Models;
using Pixora.Shared.Models;

namespace Pixora.Endpoints;

public class ImagesEndpoints : IEndpointRouteHandlerBuilder
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var imagesApiGroup = endpoints.MapGroup("/api/images").WithTags("Images");

        imagesApiGroup.MapPost(string.Empty, SaveAsync)
            .Produces<Image>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization()
            .DisableAntiforgery()
            .WithName("UploadImage");

        imagesApiGroup.MapGet("{id:guid}", GetAsync)
            .Produces<Image>()
            .Produces(StatusCodes.Status404NotFound)
            .AllowAnonymous()
            .WithName("GetImage");

        imagesApiGroup.MapGet(string.Empty, GetListAsync)
            .Produces<Image>()
            .Produces(StatusCodes.Status404NotFound)
            .AllowAnonymous()
            .WithName("GetImages");

        imagesApiGroup.MapGet("{id:guid}/stream", DownloadAsync)
            .Produces(StatusCodes.Status200OK, contentType: MediaTypeNames.Image.Jpeg)
            .Produces(StatusCodes.Status200OK, contentType: MediaTypeNames.Image.Png)
            .Produces(StatusCodes.Status404NotFound)
            .AllowAnonymous()
            .WithName("DownloadImage");

        imagesApiGroup.MapDelete("{id:guid}", DeleteAsync)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization()
            .WithName("DeleteImage");
    }

    private static async Task<IResult> SaveAsync([FromForm] UploadImageRequest request, IImageService imageService, HttpContext httpContext)
    {
        var result = await imageService.SaveAsync(request.File, request.Description, request.Tags, httpContext.RequestAborted);

        var response = httpContext.CreateResponse(result, "GetImage", new { id = result.Content?.Id });
        return response;
    }

    private static async Task<IResult> GetAsync(Guid id, IImageService imageService, HttpContext httpContext)
    {
        var result = await imageService.GetAsync(id, httpContext.RequestAborted);

        var response = httpContext.CreateResponse(result);
        return response;
    }

    private static async Task<IResult> GetListAsync(IImageService imageService, HttpContext httpContext)
    {
        var result = await imageService.GetListAsync(httpContext.RequestAborted);

        var response = httpContext.CreateResponse(result);
        return response;
    }

    private static async Task<IResult> DownloadAsync(Guid id, IImageService imageService, HttpContext httpContext)
    {
        var result = await imageService.DownloadAsync(id, httpContext.RequestAborted);

        var response = httpContext.CreateResponse(result);
        return response;
    }

    private static async Task<IResult> DeleteAsync(Guid id, IImageService imageService, HttpContext httpContext)
    {
        var result = await imageService.DeleteAsync(id, httpContext.RequestAborted);

        var response = httpContext.CreateResponse(result);
        return response;
    }
}