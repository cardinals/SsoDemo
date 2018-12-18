using Microsoft.EntityFrameworkCore;
using SiteServer.Core.Entity;
namespace SiteServer.Core.Data
{
    public class SiteContext : DbContext
    {
        public SiteContext(DbContextOptions<SiteContext> options)
            : base(options)
        { }

        public DbSet<User> User { get; set; }
        public DbSet<UserMapping> UserMapping { get; set; }
        public DbSet<SiteConfig> SiteConfig { get; set; }
        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<UserMapping>().HasKey(x => new { x.SsoUserId, x.UserId, x.SourceSiteConfigId, x.TargetSiteConfigId });
        //    base.OnModelCreating(modelBuilder);
        //}
    }
}
