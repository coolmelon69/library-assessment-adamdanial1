using library_system.Dtos;

namespace library_system.ViewModels
{
    public class MeLoansViewModel
    {
        public MemberProfileResponse? Member { get; set; }

        public IReadOnlyList<LoanResponse> Loans { get; set; } = Array.Empty<LoanResponse>();

        public string? ErrorMessage { get; set; }
    }
}
