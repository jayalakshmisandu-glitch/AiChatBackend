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

// ✅ CORS (local + Vercel)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "https://ai-chat-frontend-mocha.vercel.app"
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

        // 🔥 REQUIRED for cross-site cookies
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.IsEssential = true; // ✅ IMPORTANT

        options.LoginPath = "/api/auth/login";

        // 🔥 Prevent redirect → return 401 instead
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        };

        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = 403;
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();


// 🔥 ✅ CRITICAL FIX FOR RENDER (DO NOT MISS)
var forwardedOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};

// 🔥 REQUIRED so ASP.NET trusts Render proxy
forwardedOptions.KnownNetworks.Clear();
forwardedOptions.KnownProxies.Clear();

app.UseForwardedHeaders(forwardedOptions);


// ✅ Production config
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ❌ Do NOT enable this on Render
// app.UseHttpsRedirection();

app.UseRouting();

// ✅ Apply CORS BEFORE auth
app.UseCors("AllowFrontend");

// ✅ Auth middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Swagger (dev only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Run app
//app.Run();

//// ✅ Bind to Render port
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");