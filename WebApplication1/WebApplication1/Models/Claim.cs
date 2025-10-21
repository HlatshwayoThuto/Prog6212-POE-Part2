using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Claim
    {
        public int ClaimId { get; set; }

        [Required(ErrorMessage = "Lecturer name is required")]
        [Display(Name = "Lecturer Name")]
        public string LecturerName { get; set; }

        [Required(ErrorMessage = "Hours worked is required")]
        [Range(1, 200, ErrorMessage = "Hours worked must be between 1 and 200")]
        [Display(Name = "Hours Worked")]
        public decimal HoursWorked { get; set; }

        [Required(ErrorMessage = "Hourly rate is required")]
        [Range(50, 500, ErrorMessage = "Hourly rate must be between 50 and 500")]
        [Display(Name = "Hourly Rate (R)")]
        public decimal HourlyRate { get; set; }

        [Display(Name = "Total Amount")]
        public decimal TotalAmount => Math.Round(HoursWorked * HourlyRate, 2);

        [Display(Name = "Additional Notes")]
        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }

        public string Status { get; set; } = "Pending";
        public DateTime SubmissionDate { get; set; } = DateTime.Now;
        public DateTime? ApprovalDate { get; set; }

        // optional
        public string? ApprovedBy { get; set; }

        public List<Document> Documents { get; set; } = new List<Document>();
    }
}
