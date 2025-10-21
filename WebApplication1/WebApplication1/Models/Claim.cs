// Import data annotation attributes used for validation and display formatting
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    // Represents a claim submitted by a lecturer
    public class Claim
    {
        // Unique identifier for the claim (primary key)
        public int ClaimId { get; set; }

        // Lecturer's full name (required field with custom error message)
        [Required(ErrorMessage = "Lecturer name is required")]
        [Display(Name = "Lecturer Name")] // Label used in UI forms
        public string LecturerName { get; set; }

        // Number of hours worked (required, must be between 1 and 200)
        [Required(ErrorMessage = "Hours worked is required")]
        [Range(1, 200, ErrorMessage = "Hours worked must be between 1 and 200")]
        [Display(Name = "Hours Worked")]
        public decimal HoursWorked { get; set; }

        // Hourly pay rate (required, must be between 50 and 500)
        [Required(ErrorMessage = "Hourly rate is required")]
        [Range(50, 500, ErrorMessage = "Hourly rate must be between 50 and 500")]
        [Display(Name = "Hourly Rate (R)")]
        public decimal HourlyRate { get; set; }

        // Computed property that calculates the total amount (HoursWorked × HourlyRate), rounded to 2 decimal places
        [Display(Name = "Total Amount")]
        public decimal TotalAmount => Math.Round(HoursWorked * HourlyRate, 2);

        // Optional notes field with a maximum length of 500 characters
        [Display(Name = "Additional Notes")]
        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }

        // Current status of the claim (e.g., Pending, Verified, Approved, Rejected)
        public string Status { get; set; } = "Pending";

        // Date and time when the claim was submitted (defaults to current time)
        public DateTime SubmissionDate { get; set; } = DateTime.Now;

        // Date and time when the claim was approved (nullable)
        public DateTime? ApprovalDate { get; set; }

        // Name of the person who approved the claim (optional)
        public string? ApprovedBy { get; set; }

        // List of associated documents uploaded with the claim
        public List<Document> Documents { get; set; } = new List<Document>();
    }
}