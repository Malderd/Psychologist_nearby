using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Psychologist_nearby.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Test> Tests { get; set; }
    public DbSet<TestQuestion> TestQuestions { get; set; }
    public DbSet<TestAnswer> TestAnswers { get; set; }
    public DbSet<TestResult> TestResults { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // --- Уникальный индекс на Email ---
        builder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // --- Внешний ключ: User -> Role (Restrict) ---
        builder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- Внешний ключ: Review -> User (SetNull) ---
        builder.Entity<Review>()
            .HasOne(r => r.User)
            .WithMany(u => u.Reviews)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // --- Структуры тестов и результатов (Cascade) ---
        builder.Entity<TestQuestion>()
            .HasOne(q => q.Test)
            .WithMany(t => t.Questions)
            .HasForeignKey(q => q.TestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<TestAnswer>()
            .HasOne(a => a.Question)
            .WithMany(q => q.Answers)
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<TestResult>()
            .HasOne(tr => tr.User)
            .WithMany(u => u.TestResults)
            .HasForeignKey(tr => tr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<TestResult>()
            .HasOne(tr => tr.Test)
            .WithMany(t => t.TestResults)
            .HasForeignKey(tr => tr.TestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}