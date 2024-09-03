using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using PlugIn.Dal.AppContext;
using PlugIn.Dal.Models;
using PlugIn.Extensions;
using PlugIn.JwtTokenProviders;
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

			builder.Services.AddScoped<IProjectRepository, ProjectRepository>();

			builder.Services.AddSingleton<JwtTokenProvider>();

			builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>()
				.AddSignInManager<SignInManager<ApplicationUser>>()
				.AddEntityFrameworkStores<PlugInDbContext>()
				.AddUserManager<UserManager<ApplicationUser>>()
				.AddRoles<IdentityRole<Guid>>()
				.AddRoleManager<RoleManager<IdentityRole<Guid>>>()
				.AddDefaultTokenProviders();

			builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
				.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, o =>
				{
					o.Cookie.MaxAge = TimeSpan.FromDays(3);
				});

			builder.Services.Configure<IdentityOptions>(options =>
			{
				options.Password.RequireDigit = true;
				options.Password.RequireLowercase = true;
				options.Password.RequireNonAlphanumeric = false;
				options.Password.RequireUppercase = true;
				options.Password.RequiredLength = 6;
				options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
				options.Lockout.MaxFailedAccessAttempts = 5;
				options.User.RequireUniqueEmail = true;
			});
			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (!app.Environment.IsDevelopment())
			{
				app.UseExceptionHandler("/Home/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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