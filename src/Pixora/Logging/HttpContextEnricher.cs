using Pixora.Authentication.Extensions;
using Serilog.Core;
using Serilog.Events;
using TinyHelpers.Extensions;

namespace Pixora.Logging;

public class HttpContextEnricher(IHttpContextAccessor httpContextAccessor) : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = httpContextAccessor.HttpContext;

        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("LevelNumber", (int)logEvent.Level));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("IsHttp", httpContext is not null));

        if (httpContext is null)
        {
            return;
        }

        var userName = httpContext.User.Identity?.Name;
        var email = httpContext.User.GetEmail();

        if (userName.HasValue())
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserName", userName));
        }

        if (email.HasValue())
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Email", email));
        }
    }
}