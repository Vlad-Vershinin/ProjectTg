using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace Server.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Chat> Chats { get; set; }
    public DbSet<PrivateChat> PrivateChats { get; set; }
    public DbSet<Message> Messages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PrivateChat>()
            .HasOne(pc => pc.User1)
            .WithMany(u => u.PrivateChatsAsUser1)
            .HasForeignKey(pc => pc.User1Id)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PrivateChat>()
            .HasOne(pc => pc.User2)
            .WithMany(u => u.PrivateChatsAsUser2)
            .HasForeignKey(pc => pc.User2Id)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Chat)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ChatId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
