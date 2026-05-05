using AiChatBackend.DAL;
using AiChatBackend.Sevices;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddSingleton<GeminiService>();
builder.Services.AddSingleton<AuthService>();
builder.Services.AddScoped<ChatService>();

builder.Services.AddControllersWithViews();

// ✅ CORS (Vercel frontend)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
           
            "https://ai-chat-frontend-mocha.vercel.app",
            "https://ai-chat-frontend-git-main-jayalakshmisandu-glitchs-projects.vercel.app"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

// ✅ Authentication (cookie)
builder.Services.AddAuthentication("cookie")
    .AddCookie("cookie", options =>
    {
        options.Cookie.Name = "auth_cookie";
        options.Cookie.HttpOnly = true;

        // 🔥 REQUIRED for cross-domain cookies
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

        options.LoginPath = "/api/auth/login";

        // 🔥 Prevent redirect → return 401 instead
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// ✅ 🔥 CRITICAL: Fix HTTPS behind Render proxy
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto
});

// ✅ Production config
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ❌ Do NOT enable this on Render
// app.UseHttpsRedirection();

app.UseRouting();

// ✅ Apply CORS
app.UseCors("AllowFrontend");

// ✅ Auth middleware order
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Swagger (dev only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
//app.Run();

//// ✅ Bind to Render port
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");