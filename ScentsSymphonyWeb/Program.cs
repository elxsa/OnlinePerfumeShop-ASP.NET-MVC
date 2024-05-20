using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ScentsSymphonyWeb.Data;
using ScentsSymphonyWeb.Models;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();


// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

// Ad?ugare servicii pentru sesiuni
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configurare Stripe
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Use Developer Exception Page to show detailed errors
    app.UseMigrationsEndPoint(); // Specific to Entity Framework Core migrations
}
else
{
    app.UseExceptionHandler("/Home/Error"); // Use a generic error handler page
    app.UseHsts(); // Enforce HTTPS
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.Use(async (context, next) =>
{
    context.Items["PublishableKey"] = builder.Configuration["Stripe:PublishableKey"];
    await next.Invoke();
});


app.UseAuthentication();
app.UseAuthorization();

// Ad?ugare middleware pentru sesiuni
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();



StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

app.Run();
