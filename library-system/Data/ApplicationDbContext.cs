using library_system.Models;
using Microsoft.EntityFrameworkCore;

namespace library_system.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Book> Books => Set<Book>();

        public DbSet<Member> Members => Set<Member>();

        public DbSet<Loan> Loans => Set<Loan>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Book>(entity =>
            {
                entity.HasIndex(book => book.ISBN)
                    .IsUnique();

                entity.Property(book => book.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(book => book.Author)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(book => book.ISBN)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.ToTable(table =>
                    table.HasCheckConstraint("CK_Books_TotalCopies_NonNegative", "[TotalCopies] >= 0"));
            });

            modelBuilder.Entity<Member>(entity =>
            {
                entity.HasIndex(member => member.SsoSubject)
                    .IsUnique();

                entity.Property(member => member.SsoSubject)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(member => member.FullName)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(member => member.Email)
                    .IsRequired()
                    .HasMaxLength(254);
            });

            modelBuilder.Entity<Loan>(entity =>
            {
                entity.HasOne(loan => loan.Book)
                    .WithMany(book => book.Loans)
                    .HasForeignKey(loan => loan.BookId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(loan => loan.Member)
                    .WithMany(member => member.Loans)
                    .HasForeignKey(loan => loan.MemberId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(loan => new { loan.BookId, loan.ReturnedDate });
                entity.HasIndex(loan => new { loan.MemberId, loan.ReturnedDate });
            });
        }
    }
}
