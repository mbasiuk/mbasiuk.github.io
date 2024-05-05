using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAntiforgery();
builder.Services.Configure<LoginOptions>(builder.Configuration.GetSection(LoginOptions.DefaultSection));

var app = builder.Build();

app.UseAntiforgery();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/hr", () =>
{
    return Results.File("hr.html", contentType: "text/html");
});

app.MapGet("/resume", () =>
{
    return Results.File(@"cv/cv.html", contentType: "text/html");
});

app.MapGet("login", () =>
{
    return Results.File("login.html", contentType: "text/html");
});

app.MapGet("tool", (HttpContext context) =>
{
    return Results.File("tool.html", contentType: "text/html");
}).AddEndpointFilter<CookieAuthFilter>();

app.MapPost("login", ([FromForm] LoginRecord login, HttpContext context, IOptions<LoginOptions> options) =>
{
    LoginOptions loginOptions = options.Value;
    if (loginOptions.IsValid(login))
    {
        var expires = DateTimeOffset.UtcNow.AddMonths(6);
        var value = loginOptions.Hash(login);
        context.Response.Cookies.Delete("auth");
        context.Response.Cookies.Append("auth", value, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            Expires = expires
        });
        context.Response.Redirect("/tool");
    }
    else
    {
        context.Response.Redirect("/login");
    }
}).DisableAntiforgery();

app.Run();

record LoginRecord(string user, string password);
