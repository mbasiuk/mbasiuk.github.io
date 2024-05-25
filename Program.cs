using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAntiforgery();
builder.Services.Configure<SignInOptions>(builder.Configuration.GetSection(SignInOptions.DefaultSection));

var app = builder.Build();
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

app.MapGet("signin", () =>
{
    return Results.File("signin.html", contentType: "text/html");
});

app.MapGet("tool", (HttpContext context) =>
{
    return Results.File("tool.html", contentType: "text/html");
}).AddEndpointFilter<AuthorizeSuperUserFilter>();

app.MapPost("home_page", () => Results.Ok());

app.MapPost("tool/visits", (HttpContext context) =>
{
    return Visit.GetSummary();
}).AddEndpointFilter<AuthorizeSuperUserFilter>();

app.MapPost("tool/visits/ignore", ([FromBody] Visits visits, HttpContext context) =>
{
    Visit.IgnoreByPage(visits);
}).AddEndpointFilter<AuthorizeSuperUserFilter>();

app.MapPost("tool/search", (HttpContext context) =>
{
    CompanySearch company = new CompanySearch();
    var result = company.Save();
    app.Logger.LogInformation("/tool/search save result: {0}", result);
    return Results.Redirect("/tool");
}).AddEndpointFilter<AuthorizeSuperUserFilter>();

app.MapPost("/tool/search/recent", (HttpContext context) =>
{
    return Results.Ok(CompanySearch.Recent());
}).AddEndpointFilter<AuthorizeSuperUserFilter>();

app.MapPost("signin", (SignInRecord signIn, HttpContext context, IOptions<SignInOptions> options) =>
{
    SignInOptions singInOptions = options.Value;
    if (singInOptions.IsValid(signIn))
    {
        var expires = DateTimeOffset.UtcNow.AddMonths(6);
        context.Response.Cookies.Delete("auth");
        context.Response.Cookies.Append("auth", singInOptions.Secret, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            Expires = expires
        });
        return Results.Ok();
    }
    return Results.NotFound();
});

app.MapGet("/bas-a-create", (HttpContext context) =>
{
    return Results.File("bas-a-create.html", contentType: "text/html");
});

app.MapPost("/bas-a-create", (HttpContext context) =>
{
    var basa = BasA.Create();
    return Results.Created("bas-a.html", basa);
});


app.MapPost("/bas-a/ro", (BasA input, HttpContext context) =>
{
    if (!input.ReadId.HasValue)
    {
        return Results.BadRequest();
    }
    var output = BasA.FindByReadId(input.ReadId.Value);
    if (output == null)
    {
        return Results.NotFound();
    }
    if(output.Id != null)
    {
        return Results.StatusCode(500);
    }
    return Results.Ok(output);
});

app.Use(async (context, next) =>
{
    Session session = null!;
    try
    {
        session = new Session(context);
        var visit = new Visit(context);
        visit.Track();
    }
    catch (SqliteException e)
    {
        if (session != null && e.Message.Contains("FOREIGN KEY constraint failed"))
        {
            session.Clear(context);
        }
    }
    await next.Invoke();
});

app.Run();

record SignInRecord(string User, string Password);
