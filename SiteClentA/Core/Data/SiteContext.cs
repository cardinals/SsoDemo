using Microsoft.EntityFrameworkCore;
using SiteClient.Core.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace SiteClient.Core.Data
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
