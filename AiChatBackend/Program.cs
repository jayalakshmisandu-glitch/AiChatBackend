using AiChatBackend.DAL;
using AiChatBackend.Sevices;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddSingleton<GeminiService>();
builder.Services.AddSingleton<AuthService>();
builder.Services.AddScoped<ChatService>();

builder.Services.AddControllersWithViews();

// ✅ CORS (allow frontend — update later with your real frontend URL)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ✅ Authentication
builder.Services.AddAuthentication("cookie")
    .AddCookie("cookie", options =>
    {
        options.Cookie.Name = "auth_cookie";
        options.Cookie.HttpOnly = true;

        // 🔐 Secure for production
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

        options.LoginPath = "/api/auth/login";
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// ✅ Production config
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ❌ Disable HTTPS redirection (Render handles HTTPS)
// app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("AllowFrontend");

// ✅ Auth middleware order (correct)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ✅ Swagger only in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 🔥 CRITICAL: Bind to Render port
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");