using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using MongoBlazor.Components;
using MongoBlazor.Model;
using MongoBlazor.Services;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDB"));


// Authentication Configuration
builder.Services.Configure<AuthenticationSettings>(
    builder.Configuration.GetSection("Authentication"));

builder.Services.AddSingleton<TransactionService>();

// Services
builder.Services.AddSingleton<TransactionService>();
builder.Services.AddSingleton<AuthService>();
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<ProtectedSessionStorage>();

// ✅ THIS NOW WORKS
builder.Services.AddMudServices();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
// Redirect root to login if authentication is enabled
app.Use(async (context, next) =>
{
    var authService = context.RequestServices.GetRequiredService<AuthService>();

    if (authService.IsLoginEnabled && context.Request.Path == "/")
    {
        var sessionService = context.RequestServices.GetRequiredService<SessionService>();
        var isAuthenticated = await sessionService.IsAuthenticatedAsync();

        if (!isAuthenticated)
        {
            context.Response.Redirect("/login");
            return;
        }
    }

    await next();
});
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
