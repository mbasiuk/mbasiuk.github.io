using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAntiforgery();
builder.Services.Configure<LoginOptions>(builder.Configuration.GetSection(LoginOptions.DefaultSection));

var app = builder.Build();
#if NET8_0_OR_GREATER
app.UseAntiforgery();
#endif
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
}).AddEndpointFilter<AuthorizeSuperUserFilter>();

app.MapPost("home_page", () => Results.Ok());

app.MapPost("tool/visits", (HttpContext context) =>
{
    return Visit.GetSummary();
    /*
    return new[] {
        new { page = "hr", total = 10, unique = 5, n = 2 },
        new { page = "index", total = 15, unique = 10, n = 8 }
    };*/
}).AddEndpointFilter<AuthorizeSuperUserFilter>();

app.MapPost("tool/search", ([FromForm] CompanySearch company, HttpContext context) =>
{
    var result = company.Save();
    app.Logger.LogInformation("/tool/search save result: {0}", result);
    return Results.Redirect("/tool");
}).AddEndpointFilter<AuthorizeSuperUserFilter>()
#if NET8_0_OR_GREATER
.DisableAntiforgery()
#endif
;

app.MapPost("/tool/search/recent", (HttpContext context) =>
{
    return Results.Ok(CompanySearch.Recent());
}).AddEndpointFilter<AuthorizeSuperUserFilter>()
#if NET8_0_OR_GREATER
.DisableAntiforgery()
#endif
;

app.MapPost("login", ([FromForm] LoginRecord login, HttpContext context, IOptions<LoginOptions> options) =>
{
    LoginOptions loginOptions = options.Value;
    if (loginOptions.IsValid(login))
    {
        var expires = DateTimeOffset.UtcNow.AddMonths(6);
        context.Response.Cookies.Delete("auth");
        context.Response.Cookies.Append("auth", loginOptions.Secret, new CookieOptions
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
})
#if NET8_0_OR_GREATER
.DisableAntiforgery()
#endif
;

app.Use(async (context, next) => {
    var session = new Session(context);
    var visit = new Visit(context);
    visit.Track();
    await next.Invoke();
});

app.Run();

record LoginRecord(string User, string Password);
