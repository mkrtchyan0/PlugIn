using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PlugIn.Dal.AppContext;
using PlugIn.Dal.Models;
using PlugIn.Extensions;
using PlugIn.JwtTokenProviders;
using PlugIn.Models;
using PlugIn.Repository;

namespace PlugIn
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddPlugInDbContext(builder);

            builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            builder.Services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();

            builder.Services.AddScoped<IProjectRepository, ProjectRepository>();

            builder.Services.AddSingleton<JwtTokenProvider>();

            builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options => 
            options.SignIn.RequireConfirmedEmail = true)
                .AddSignInManager<SignInManager<ApplicationUser>>()
                .AddEntityFrameworkStores<PlugInDbContext>()
                .AddUserManager<UserManager<ApplicationUser>>()
                .AddRoles<IdentityRole<Guid>>()
                .AddRoleManager<RoleManager<IdentityRole<Guid>>>()
                .AddDefaultTokenProviders()
                .AddTokenProvider<DataProtectorTokenProvider<ApplicationUser>>(TokenOptions.DefaultEmailProvider);

            builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
            options.TokenLifespan = TimeSpan.FromHours(12));

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, o =>
                {
                    o.Cookie.MaxAge = TimeSpan.FromDays(3);
                });

            builder.Services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = true;
               
            });
            builder.Services.AddSingleton(new EmailService(
                "smtp.gmail.com",
                587,
                "smarttaxam@gmail.com",
                "qmkcevtbftufhlrm"));

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllerRoute
                (name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MigrateContext<PlugInDbContext>()
                .Run();
        }
    }
}