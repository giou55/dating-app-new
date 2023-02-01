using api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace api.Data
{
    // in this class we are using IdentityDbContext to create and configure the tables
    
    public class DataContext : IdentityDbContext<AppUser, AppRole, int, IdentityUserClaim<int>, AppUserRole,
        IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        public DataContext(DbContextOptions options) : base(options)
        {
        }

        // creates a table in database called "Likes",
        // it is going to be used as a join table to relate our entities
        // so that we have got our many to many relationship,
        // it'll be a table with two columns,
        // one the SourceUserId with integers
        // and two the TargetUserId also with integers
        public DbSet<UserLike> Likes { get; set; }

        // creates a table in database called "Messages",
        // it is going to be used as a join table to relate our entities
        // so that we have got our many to many relationship,
        public DbSet<Message> Messages { get; set; }

        // we override this method that's inside the DbContext class
        protected override void OnModelCreating(ModelBuilder builder)
        { 
            // we use this method from base DbContext class, 
            // even if in most cases it ins't necessary
            base.OnModelCreating(builder);

            builder.Entity<AppUser>()
                .HasMany(ur => ur.UserRoles)
                .WithOne(u => u.User)
                .HasForeignKey(ur => ur.UserId)
                // we don't want our foreign keys to be allowed to be null
                .IsRequired();

            builder.Entity<AppRole>()
                .HasMany(ur => ur.UserRoles)
                .WithOne(u => u.Role)
                .HasForeignKey(ur => ur.RoleId)
                // we don't want our foreign keys to be allowed to be null
                .IsRequired();

            // we add configuration for Likes table
            builder.Entity<UserLike>()
                // we specify the primary key for Likes table, that is made up of two entities
                .HasKey(k => new {k.SourceUserId, k.TargetUserId});

            builder.Entity<UserLike>()
                // we configure a relationship
                // and we say that a SourceUser can likes many LikedUsers
                .HasOne(s => s.SourceUser)
                .WithMany(l => l.LikedUsers)
                .HasForeignKey(s => s.SourceUserId)
                // if a user deletes his profile, it also will be deleted the corresponding likes
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserLike>()
                // we configure a relationship
                // and we say that a TargetUser can be liked many LikedByUsers
                .HasOne(s => s.TargetUser)
                .WithMany(l => l.LikedByUsers)
                .HasForeignKey(s => s.TargetUserId)
                // if a user deletes his profile, it also will be deleted the corresponding likes
                .OnDelete(DeleteBehavior.Cascade);

            // if we're using SQL server, we have to change one of these:
            // OnDelete(DeleteBehavior.Cascade)
            // with this:
            // OnDelete(DeleteBehavior.NoAction)
            // otherwise we'll get an error

            builder.Entity<Message>()
                .HasOne(u => u.Recipient)
                .WithMany(m => m.MessagesReceived)
                // if a sender user deletes his profile, we want the recipient of the message 
                // should still be able to see that message
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
                .HasOne(u => u.Sender)
                .WithMany(m => m.MessagesSent)
                // if a sender user deletes his profile, we want the recipient of the message 
                // should still be able to see that message
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}