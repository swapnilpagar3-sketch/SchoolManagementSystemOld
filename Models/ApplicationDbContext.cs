using Microsoft.EntityFrameworkCore;
using System;
//using System.Data.Entity;

namespace EntityFrameworkCore.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base(options)
        {
                
        }
        public DbSet<User> Users { get; set; }

        public DbSet<Student> Students { get; set; }

        public DbSet<Class> Classes { get; set; }

        public DbSet<Subject> Subjects { get; set; }

        public DbSet<Attendance> Attendance { get; set; }

        public DbSet<Exam> Exams { get; set; }

        public DbSet<StudentExamMarks> StudentExamMarks { get; set; }

        public DbSet<Fee> Fees { get; set; }

        //public DbSet<Notification> Notifications { get; set; }

        //public DbSet<UserLoginResult> UserLoginResults { get; set; }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<UserLoginResult>()
        //                .HasNoKey();

        //    base.OnModelCreating(modelBuilder);
        //}

        public DbSet<LoginResult> LoginResults { get; set; }

        public DbSet<Teacher> Teachers { get; set; }

        public DbSet<Product> Products { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<LoginResult>().HasNoKey();

            modelBuilder.Entity<Teacher>(entity =>
            {
                modelBuilder.Entity<Teacher>()
                    .ToTable("masTeacher");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Skills)
                    .IsRequired()
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.Salary)
                    .IsRequired()
                    .HasColumnType("money");

                //entity.Property(e => e.CreatedOn)
                //    .HasColumnType("date")
                //    .HasDefaultValueSql("(getdate())");
            });
        }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<LoginResult>().HasNoKey();
        //}
    }
}
