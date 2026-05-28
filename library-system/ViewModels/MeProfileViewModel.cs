using library_system.Dtos;

namespace library_system.ViewModels
{
    public class MeProfileViewModel
    {
        public MemberProfileResponse? Member { get; set; }

        public IReadOnlyList<LoanResponse> ActiveLoans { get; set; } = Array.Empty<LoanResponse>();

        public int ActiveLoanLimit { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
