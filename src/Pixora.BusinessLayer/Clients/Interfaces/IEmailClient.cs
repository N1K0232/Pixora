using Pixora.Shared.Models;

namespace Pixora.BusinessLayer.Clients.Interfaces;

public interface IEmailClient
{
    Task SendAsync(EmailMessage emailMessage, CancellationToken cancellationToken = default);
}