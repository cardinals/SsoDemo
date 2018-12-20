using Microsoft.EntityFrameworkCore;
using Server.Core.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Core.Data
{
    public class SiteContext : DbContext
    {
        public SiteContext(DbContextOptions<SiteContext> options)
            : base(options)
        { }

        public DbSet<User> User { get; set; }
        public DbSet<SiteConfig> SiteConfig { get; set; }
        public DbSet<GenericAttribute> GenericAttribute { get; set; }
    }
}
