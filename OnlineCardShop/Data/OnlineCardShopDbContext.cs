﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnlineCardShop.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineCardShop.Data
{
    public class OnlineCardShopDbContext : IdentityDbContext
    {
        public OnlineCardShopDbContext(DbContextOptions<OnlineCardShopDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Card> Cards { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Condition> Conditions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder
                .Entity<Card>()
                .HasOne(c => c.Category)
                .WithMany(c => c.Cards)
                .HasForeignKey(c => c.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .Entity<Card>()
                .HasOne(c => c.User)
                .WithMany(u => u.OwnedCards)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);



            base.OnModelCreating(builder);
        }
    }
}
