using GreenSwamp.Data;
using GreenSwamp.Middleware;
using GreenSwamp.Pages;
using GreenSwamp.Services;
using Microsoft.EntityFrameworkCore;
using static System.Formats.Asn1.AsnWriter;



    
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/account/login";
    options.LogoutPath = "/account/logout";
    options.AccessDeniedPath = "/account/login";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
});

builder.Services.AddHttpContextAccessor();


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<ISubscribeService, SubscribeService>();


builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();


using(var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated();

    
    dbContext.Database.ExecuteSqlRaw(@"
        CREATE VIEW IF NOT EXISTS trending_ponds AS
        SELECT t.tag_id, t.tag_name, COUNT(pt.post_id) AS recent_posts
        FROM tags t
        JOIN post_tags pt ON t.tag_id = pt.tag_id
        JOIN posts p ON pt.post_id = p.post_id
        WHERE p.created_at > datetime('now', '-7 day')
        GROUP BY t.tag_id
        ORDER BY recent_posts DESC
        LIMIT 10
    ");
}


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseStaticFiles();

app.UseStatusCodePagesWithReExecute("/404");

app.UseAuthentication();

app.UseAuthorization();

app.UseMiddleware<RequestLoggingMiddleware>();

app.MapControllers();

app.MapRazorPages();

app.Run();
