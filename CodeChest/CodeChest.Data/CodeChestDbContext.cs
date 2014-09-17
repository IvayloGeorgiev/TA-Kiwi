﻿namespace CodeChest.Data
{        
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.ModelConfiguration.Conventions;    
    using System.Linq;    

    using Microsoft.AspNet.Identity.EntityFramework;

    using CodeChest.Data.Migrations;
    using CodeChest.Models;    

    public class CodeChestDbContext : IdentityDbContext<User>
    {
        public CodeChestDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<CodeChestDbContext, Configuration>());
        }

        public static CodeChestDbContext Create()
        {
            return new CodeChestDbContext();
        }

        public IDbSet<CodeSnipet> CodeSnipets { get; set; }

        public IDbSet<Rating> Ratings { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();

        }
    }
}