var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/hr", () => {
    return Results.File("hr.html", contentType: "text/html");
});

app.MapGet("/resume", () => {
    return Results.File(@"cv/cv.html", contentType: "text/html");
});

app.Run();
