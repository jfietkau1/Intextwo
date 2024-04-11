using Intextwo.Data;
using Intextwo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Build.Experimental;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

services.AddAuthentication().AddGoogle(googleOptions =>
{
    googleOptions.ClientId = configuration["Authentication:Google:ClientId"];
    googleOptions.ClientSecret = configuration["Authentication:Google:ClientSecret"];
});

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();


//tryring this out
builder.Services.AddDbContext<LegoDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});


builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings.
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 12; // Passwords must have at least 12 characters
    options.Password.RequiredUniqueChars = 1;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 3;
    options.Lockout.AllowedForNewUsers = true;

    options.User.RequireUniqueEmail = true;

});



builder.Services.AddControllersWithViews();

builder.Services.AddScoped<ILegoRepository, EFLegoRepository>();

builder.Services.AddRazorPages();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddScoped<Cart>(sp => SessionCart.GetCart(sp));
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.Use(async (context, next) => {
        context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; img-src 'self' data: https://m.media-amazon.com https://www.lego.com https://images.brickset.com https://www.brickeconomy.com https://cdn.builder.io; script-src 'self' 'unsafe-inline' https://code.jquery.com https://cdn.jsdelivr.net https://maxcdn.bootstrapcdn.com; font-src 'self' https://fonts.gstatic.com https://fonts.googleapis.com; style-src 'self' https://maxcdn.bootstrapcdn.com https://cdn.jsdelivr.net 'unsafe-inline';"); // Adjust these directives according to your application's requirements
        await next();
    });

    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession(); 

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();


//app.MapControllerRoute("pagination", "CustProductList/{pageNum}", new {controller = "Home", action = "Index"});
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();


using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await SeedRolesAsync(userManager, roleManager);
    await SeedAdminUserAsync(userManager, roleManager); 

}

async Task SeedRolesAsync(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
{
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        var adminRole = new IdentityRole("Admin");
        await roleManager.CreateAsync(adminRole);
    }
    if (!await roleManager.RoleExistsAsync("User"))
    {
        var userRole = new IdentityRole("User");
        await roleManager.CreateAsync(userRole);
    }
}



async Task SeedAdminUserAsync(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
{
    string adminEmail = "admin@example.com"; // Use a secure way to store and retrieve this
    string adminPassword = "SecurePassword123!"; // Use a secure way to store and retrieve this

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true // Confirm email to bypass email verification
        };

        var createUserResult = await userManager.CreateAsync(adminUser, adminPassword);
        if (createUserResult.Succeeded)
        {
            // Check if the admin role exists
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                var adminRole = new IdentityRole("Admin");
                await roleManager.CreateAsync(adminRole);
            }

            // Add the admin user to the admin role
            var addToRoleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
            if (!addToRoleResult.Succeeded)
            {
                // Handle the case where the admin user could not be added to the admin role
                throw new InvalidOperationException("Failed to add user to Admin role.");
            }
        }
        else
        {
            // Handle the case where the admin user could not be created
            throw new InvalidOperationException("Failed to create the Admin user.");
        }
    }
}




app.Run();


