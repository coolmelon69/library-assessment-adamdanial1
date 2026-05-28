using System.Security.Claims;
using library_system.Data;
using library_system.Models;
using library_system.Services.Loans;
using library_system.Services.Members;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace library_system.Tests;

public class LibraryServiceTests
{
    [Fact]
    public async Task BorrowBookAsync_rejects_when_no_copies_are_available()
    {
        await using var context = CreateContext();
        await context.Database.EnsureCreatedAsync();

        var book = new Book
        {
            Title = "Clean Code",
            Author = "Robert C. Martin",
            ISBN = "9780132350884",
            PublishedYear = 2008,
            TotalCopies = 1
        };
        var existingMember = new Member
        {
            SsoSubject = "existing-member",
            FullName = "Existing Member",
            Email = "existing@example.test",
            JoinedDate = DateTime.UtcNow
        };

        context.AddRange(book, existingMember);
        await context.SaveChangesAsync();

        context.Loans.Add(new Loan
        {
            BookId = book.Id,
            MemberId = existingMember.Id,
            BorrowedDate = DateTime.UtcNow.AddDays(-1)
        });
        await context.SaveChangesAsync();

        var service = CreateLoanService(context);
        var result = await service.BorrowBookAsync(book.Id, CreateUser("borrower"));

        Assert.Equal(BorrowBookStatus.NoCopiesAvailable, result.Status);
        Assert.Null(result.Loan);
        Assert.Equal(1, await context.Loans.CountAsync(loan => loan.BookId == book.Id));
    }

    [Fact]
    public async Task BorrowBookAsync_rejects_when_member_already_has_three_active_loans()
    {
        await using var context = CreateContext();
        await context.Database.EnsureCreatedAsync();

        var member = new Member
        {
            SsoSubject = "loan-limit-member",
            FullName = "Loan Limit Member",
            Email = "limit@example.test",
            JoinedDate = DateTime.UtcNow
        };
        var targetBook = new Book
        {
            Title = "Domain-Driven Design",
            Author = "Eric Evans",
            ISBN = "9780321125217",
            PublishedYear = 2003,
            TotalCopies = 2
        };

        context.Members.Add(member);
        context.Books.Add(targetBook);

        for (var index = 0; index < 3; index++)
        {
            var borrowedBook = new Book
            {
                Title = $"Borrowed Book {index}",
                Author = "Test Author",
                ISBN = $"limit-{index}",
                PublishedYear = 2024,
                TotalCopies = 1
            };

            context.Books.Add(borrowedBook);
            context.Loans.Add(new Loan
            {
                Book = borrowedBook,
                Member = member,
                BorrowedDate = DateTime.UtcNow.AddDays(-index - 1)
            });
        }

        await context.SaveChangesAsync();

        var service = CreateLoanService(context);
        var result = await service.BorrowBookAsync(targetBook.Id, CreateUser(member.SsoSubject));

        Assert.Equal(BorrowBookStatus.ActiveLoanLimitReached, result.Status);
        Assert.Null(result.Loan);
        Assert.False(await context.Loans.AnyAsync(loan => loan.BookId == targetBook.Id));
    }

    [Fact]
    public async Task GetOrProvisionMemberAsync_creates_member_from_google_claims()
    {
        await using var context = CreateContext();
        await context.Database.EnsureCreatedAsync();

        var service = CreateMemberProvisioningService(context);
        var result = await service.GetOrProvisionMemberAsync(CreateUser("new-member", "New Member", "new@example.test"));

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Member);
        Assert.Equal("New Member", result.Member.FullName);
        Assert.Equal("new@example.test", result.Member.Email);
        Assert.Equal(1, await context.Members.CountAsync());
    }

    [Fact]
    public async Task GetOrProvisionMemberAsync_reuses_existing_member_for_same_google_subject()
    {
        await using var context = CreateContext();
        await context.Database.EnsureCreatedAsync();

        var member = new Member
        {
            SsoSubject = "existing-google-sub",
            FullName = "Existing Member",
            Email = "existing@example.test",
            JoinedDate = DateTime.UtcNow.AddDays(-10)
        };
        context.Members.Add(member);
        await context.SaveChangesAsync();

        var service = CreateMemberProvisioningService(context);
        var result = await service.GetOrProvisionMemberAsync(CreateUser(member.SsoSubject, "Changed Name", "changed@example.test"));

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Member);
        Assert.Equal(member.Id, result.Member.Id);
        Assert.Equal("Existing Member", result.Member.FullName);
        Assert.Equal(1, await context.Members.CountAsync());
    }

    [Fact]
    public async Task ReturnLoanAsync_rejects_loan_owned_by_another_member()
    {
        await using var context = CreateContext();
        await context.Database.EnsureCreatedAsync();

        var owner = new Member
        {
            SsoSubject = "owner-sub",
            FullName = "Owner",
            Email = "owner@example.test",
            JoinedDate = DateTime.UtcNow
        };
        var otherMember = new Member
        {
            SsoSubject = "other-sub",
            FullName = "Other Member",
            Email = "other@example.test",
            JoinedDate = DateTime.UtcNow
        };
        var book = new Book
        {
            Title = "Refactoring",
            Author = "Martin Fowler",
            ISBN = "9780134757599",
            PublishedYear = 2018,
            TotalCopies = 1
        };
        var loan = new Loan
        {
            Book = book,
            Member = owner,
            BorrowedDate = DateTime.UtcNow.AddDays(-2)
        };

        context.AddRange(owner, otherMember, book, loan);
        await context.SaveChangesAsync();

        var service = CreateLoanService(context);
        var result = await service.ReturnLoanAsync(loan.Id, CreateUser(otherMember.SsoSubject));

        Assert.Equal(ReturnLoanStatus.Forbidden, result.Status);
        Assert.Null(result.Loan);
        Assert.Null(await context.Loans.Where(existingLoan => existingLoan.Id == loan.Id)
            .Select(existingLoan => existingLoan.ReturnedDate)
            .SingleAsync());
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static MemberProvisioningService CreateMemberProvisioningService(ApplicationDbContext context)
    {
        return new MemberProvisioningService(
            context,
            NullLogger<MemberProvisioningService>.Instance);
    }

    private static LoanService CreateLoanService(ApplicationDbContext context)
    {
        return new LoanService(
            context,
            CreateMemberProvisioningService(context),
            NullLogger<LoanService>.Instance);
    }

    private static ClaimsPrincipal CreateUser(
        string subject,
        string name = "Test Member",
        string email = "member@example.test")
    {
        var claims = new[]
        {
            new Claim("sub", subject),
            new Claim("name", name),
            new Claim("email", email)
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestGoogle"));
    }
}
