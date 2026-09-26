using Microsoft.AspNetCore.Http;
using PayNexa.Common.Security;

namespace PayNexa.AspNetCore.Security;

internal sealed class HttpContextCurrentActor(IHttpContextAccessor httpContextAccessor) : ICurrentActor
{
    public const string AnonymousId = "anonymous";
    public const string SubjectClaim = "sub";

    public string Id => httpContextAccessor.HttpContext is { } context
        ? context.User.FindFirst(SubjectClaim)?.Value ?? context.User.Identity?.Name ?? AnonymousId
        : SystemActor.SystemId;
}
