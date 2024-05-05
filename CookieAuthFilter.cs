
using Microsoft.Extensions.Options;

class CookieAuthFilter : IEndpointFilter
{
    readonly LoginOptions loginOptions;

    public CookieAuthFilter(IOptions<LoginOptions> options)
    {
        loginOptions = options.Value;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var auth = context.HttpContext.Request.Cookies["auth"];
        if (auth != loginOptions.Hash())
        {
            return Results.Unauthorized();
        }
        return await next(context);
    }
}