using Client.Core.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Client.Core.Data
{
    public class SiteContext : DbContext
    {
        public SiteContext(DbContextOptions<SiteContext> options)
          : base(options)
        { }

        public DbSet<User> User { get; set; }
        public DbSet<UserMapping> UserMapping { get; set; }
    }
}
