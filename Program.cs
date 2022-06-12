using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using weatherBot.Data;
using weatherBot.Data.Models;
using weatherBot.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();
builder.Services.AddSingleton<ParseWeatherService>();
builder.Services.AddHostedService<ParseWeatherService>();

new Thread(delegate() { BotStart(); }).Start();


var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");    
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();

void BotStart()
{
    Bot Bot;
    var optionBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
    optionBuilder.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));   

    var context = new ApplicationDbContext(optionBuilder.Options);
    Bot = new Bot(context);
    Bot.Start();
}