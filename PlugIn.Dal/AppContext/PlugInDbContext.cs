using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PlugIn.Dal.Models;

namespace PlugIn.Dal.AppContext
{
	public class PlugInDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        private readonly IConfiguration _configuration;
        public PlugInDbContext(IConfiguration configuration, DbContextOptions<PlugInDbContext> options) : base (options)
        {
            _configuration = configuration;
        }
        public DbSet<ApplicationProject> AppProject { get; set; }
    }
}
