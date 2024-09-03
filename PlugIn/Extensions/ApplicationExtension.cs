using Microsoft.EntityFrameworkCore;
using PlugIn.Dal.AppContext;

namespace PlugIn.Extensions
{
    public static class ApplicationExtension
    {
        public static IServiceCollection AddPlugInDbContext(this IServiceCollection services, WebApplicationBuilder builder)
        {
            builder.Services.AddDbContext<PlugInDbContext>(options =>
             options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));
            return services;

		}
		public static WebApplication MigrateContext<TContext>(this WebApplication app) where TContext : DbContext
		{
			using var scope = app.Services.CreateScope();
			var context = scope.ServiceProvider.GetRequiredService<TContext>();
			if (!context.Database.EnsureCreated())
				context.Database.Migrate();
			return app;
		}
	}
}
