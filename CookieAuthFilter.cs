
using Microsoft.Extensions.Options;

class AuthorizeSuperUserFilter : IEndpointFilter
{
    readonly LoginOptions loginOptions;

    public AuthorizeSuperUserFilter(IOptions<LoginOptions> options)
    {
        loginOptions = options.Value;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var auth = context.HttpContext.Request.Cookies["auth"];
        if (auth != loginOptions.Secret)
        {
            return Results.Redirect("/login");
        }
        return await next(context);
    }
}