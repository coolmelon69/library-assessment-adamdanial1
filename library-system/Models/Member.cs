using System.ComponentModel.DataAnnotations;
using library_system.Security;

namespace library_system.Models
{
    public class Member
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string SsoSubject { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(254)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string Role { get; set; } = AppRoles.User;

        public DateTime JoinedDate { get; set; }

        public ICollection<Loan> Loans { get; set; } = new List<Loan>();
    }
}
