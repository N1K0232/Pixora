using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OperationResults;
using Pixora.Authentication.Extensions;
using Pixora.BusinessLayer.Services.Interfaces;
using Pixora.DataAccessLayer;
using Pixora.Shared.Models;
using Pixora.Shared.Models.Requests;
using Entities = Pixora.DataAccessLayer.Entities;

namespace Pixora.BusinessLayer.Services;

public class PostService(IApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor) : IPostService
{
    public async Task<Result<Post>> CreateAsync(SavePostRequest request, CancellationToken cancellationToken)
    {
        var user = httpContextAccessor.HttpContext!.User;
        var post = new Entities.Post
        {
            UserId = user.GetId(),
            Title = request.Title,
            Content = request.Content,
            Visibility = request.Visibility
        };

        await dbContext.CreateAsync(post, cancellationToken);
        await dbContext.SaveAsync(cancellationToken);

        var createdPost = new Post(post.Id, user.Identity?.Name ?? string.Empty, request.Title, request.Content, post.CreatedAt, post.LastModifiedAt, post.IsEdited);
        return createdPost;
    }

    public async Task<Result<Post>> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var dbPost = await dbContext.GetData<Entities.Post>().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (dbPost is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, "Post not found", $"No post found with id {id}");
        }

        if (!dbPost.IsPublished)
        {
            return Result.Fail(FailureReasons.Forbidden, "Content not available", "Content not available");
        }

        var post = new Post(id, dbPost.User.UserName!, dbPost.Title, dbPost.Content, dbPost.CreatedAt, dbPost.LastModifiedAt, dbPost.IsEdited);
        return post;
    }

    public async Task<Result<IEnumerable<Post>>> GetListAsync(CancellationToken cancellationToken)
    {
        var posts = await dbContext.GetData<Entities.Post>()
            .Where(p => p.IsPublished)
            .Include(p => p.User)
            .Select(p => new Post(p.Id, p.User.UserName!, p.Title, p.Content, p.CreatedAt, p.LastModifiedAt, p.IsEdited))
            .ToListAsync(cancellationToken);

        return posts;
    }

    public async Task<Result> UpdateAsync(Guid id, SavePostRequest request, CancellationToken cancellationToken)
    {
        var post = await dbContext.GetData<Entities.Post>(true).FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (post is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, "Post not found", $"No post found with id {id}");
        }

        post.Title = request.Title;
        post.Content = request.Content;

        post.Visibility = request.Visibility;
        post.IsEdited = true;

        await dbContext.SaveAsync(cancellationToken);
        return Result.Ok();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var post = await dbContext.GetAsync<Entities.Post>(id, cancellationToken);
        if (post is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, "Post not found", $"No post found with id {id}");
        }

        await dbContext.DeleteAsync(post, cancellationToken);
        await dbContext.SaveAsync(cancellationToken);

        return Result.Ok();
    }
}