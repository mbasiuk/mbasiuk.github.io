
using Microsoft.Extensions.Options;

class AuthorizeSuperUserFilter : IEndpointFilter
{
    readonly SignInOptions signInOptions;

    public AuthorizeSuperUserFilter(IOptions<SignInOptions> options)
    {
        signInOptions = options.Value;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var auth = context.HttpContext.Request.Cookies["auth"];
        if (auth != signInOptions.Secret)
        {
            return Results.Redirect("/signin?returnUrl=" + context.HttpContext.Request.Path);
        }
        return await next(context);
    }
}