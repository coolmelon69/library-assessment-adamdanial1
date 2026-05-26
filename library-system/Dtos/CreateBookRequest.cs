using System.ComponentModel.DataAnnotations;

namespace library_system.Dtos
{
    public class CreateBookRequest
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string Author { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string ISBN { get; set; } = string.Empty;

        [Range(1, 9999)]
        public int PublishedYear { get; set; }

        [Range(0, int.MaxValue)]
        public int TotalCopies { get; set; }
    }
}
