# Prog6212-POE-Part2
Developer Notes

Developed by Thuto Hlatshwayo (Student No: ST10450572)
Module: PROG6212 – Programming (POE Part 2)
Institution: iie msa
Date:22 October 2025

Lecturer Claim Management System (PROG6212 Part 2)
 Overview

The Lecturer Claim Management System is a full-stack ASP.NET Core MVC web application developed as part of the PROG6212 Practical Assignment (Part 2).
It allows lecturers to submit and track claims for hours worked, while programme coordinators and academic managers can verify and approve these claims.

This system focuses on data persistence, error handling, security, unit testing, and a clean user interface — in accordance with the Part 2 checklist and marking rubric.

🚀 Features Implemented
👩‍🏫 Lecturer Functionality

Submit a Claim:
Lecturers can submit new claims by entering:

Lecturer name

Hours worked

Hourly rate

Optional notes

Supporting documents (PDF, DOCX, XLSX)

Document Upload Security:

Files are encrypted before saving (no direct access from outside).

Only valid file types and sizes are accepted.

Files are linked directly to the corresponding claim.

Clear error messages are displayed if an invalid file is uploaded.

Track My Claims:
Lecturers can view all their claims and see real-time status updates:

Pending, Verified, Approved, or Rejected.

Each claim displays key info such as total amount, submission date, and attached documents.

 Programme Coordinator Functionality

Coordinator View:
Displays all claims currently marked as Pending.

Verify or Reject Claims:
Coordinators can verify claims (which moves them to the manager approval stage) or reject them with a single click.

View Supporting Documents:
Coordinators can securely view and download encrypted files linked to claims.

 Academic Manager Functionality

Manager View:
Displays all Verified claims awaiting manager approval.

Approve or Reject Claims:
Managers can approve claims (marking them as finalized) or reject them if invalid.

Secure Document Access:
Managers can download attached documents via the same encrypted file system.

 Data Persistence

All data is stored in a local JSON file (claims_data.json) via the custom DataService class.

The data includes claims, documents, and metadata such as next available IDs.

This simulates a database layer and persists between sessions.

 Security & File Encryption

File uploads are stored under wwwroot/uploads, but encrypted using a FileProtector service.

Only authorized users (via the app’s UI) can decrypt and download files.

Input validation ensures only .pdf, .docx, and .xlsx files under 5MB are accepted.

 Error Handling

Error handling has been implemented throughout the application with meaningful user feedback:

Controllers use try-catch blocks to safely handle logic errors.

Model validation errors are displayed using Bootstrap alerts.

All exceptions are logged to the console for debugging.

Safe fallback messages prevent the app from crashing due to runtime issues (e.g., file not found, invalid input, etc.).

 Unit Testing

A full xUnit test suite is included under the WebApplication1.Tests project.
All major features are tested to ensure data integrity and error tolerance.

Implemented Tests:

#	Test Name	Description
1	AddClaim_ShouldIncreaseClaimCount	Ensures claims are added successfully.
2	UpdateClaimStatus_ShouldChangeStatusAndApprovedBy	Verifies claim status updates correctly.
3	GetClaimsByStatus_ShouldReturnOnlyMatchingClaims	Returns only claims with a given status.
4	AddDocumentToClaim_ShouldAttachDocumentSuccessfully	Ensures documents are attached to claims.
5	TotalAmount_ShouldBeCalculatedCorrectly	Verifies total = hours × rate.
6	UpdateClaimStatus_ShouldHandleInvalidIdGracefully	Checks graceful handling of invalid claim IDs.
7	AddClaim_ShouldThrowWhenLecturerNameMissing	Confirms the app doesn’t crash on invalid input.

 All 7 tests pass successfully using the built-in .NET Test Explorer.

🧠 Architecture Overview
WebApplication1/
│
├── Controllers/
│   ├── LecturerController.cs       → Lecturer claim submission, upload & tracking
│   ├── AdminController.cs          → Coordinator & Manager verification logic
│   └── HomeController.cs           → Landing pages
│
├── Models/
│   ├── Claim.cs                    → Claim entity model
│   ├── Document.cs                 → Document metadata model
│   ├── DataService.cs              → Handles data persistence and logic
│   └── IFileProtector.cs           → Interface for encryption/decryption services
│
├── Views/
│   ├── Home/Index.cshtml           → Modern, animated home page
│   ├── Lecturer/SubmitClaim.cshtml → Claim submission form
│   ├── Lecturer/TrackClaims.cshtml → View and track submitted claims
│   ├── Admin/CoordinatorView.cshtml→ View pending claims for coordinators
│   └── Admin/ManagerView.cshtml    → View verified claims for managers
│
├── wwwroot/
│   ├── css/site.css                → Custom styling
│   ├── uploads/                    → Secure encrypted uploads
│   └── bootstrap + icons assets
│
└── WebApplication1.Tests/
    ├── DataServiceTests.cs
    ├── DataServiceInvalidIdTests.cs
    ├── DataServiceValidationTests.cs
    └── FakeEnvironment.cs

 User Interface & Design

Built using Bootstrap 5 for responsive, mobile-friendly design.

Includes modern color gradients, soft shadows, and hover animations.

Home Page:

Gradient background hero section.

Animated welcome message.

“Submit a Claim” and “Track My Claims” CTA buttons.

Lecturer View:

Clean form layout with validation messages.

Upload progress and file previews.

Instant feedback after submission.

Admin Views:

Organized card/table layout for pending/verified claims.

Inline “Verify,” “Approve,” and “Reject” buttons.

Downloadable document links.

 Error Handling Examples
Component	Error	Handling
File Upload	Invalid type or too large	Validation message in red Bootstrap alert
DataService	JSON load/save error	Console log + default empty dataset
Controller	Exception in data ops	try-catch block with TempData["ErrorMessage"]
Missing claim ID	Lookup failure	Returns NotFound() gracefully
 Tools & Frameworks Used

