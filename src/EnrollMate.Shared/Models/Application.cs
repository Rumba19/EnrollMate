using EnrollMate.Shared.Enums;

namespace EnrollMate.Shared.Models;

public class Application
{
    /// <summary>Unique identifier for this application.</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    // -------------------------------------------------------------------------
    // Student & parent
    // -------------------------------------------------------------------------

    /// <summary>The student this application is for.</summary>
    public Student Student { get; set; } = new();

    /// <summary>Parent or guardian first name.</summary>
    public string ParentFirstName { get; set; } = string.Empty;

    /// <summary>Parent or guardian last name.</summary>
    public string ParentLastName { get; set; } = string.Empty;

    /// <summary>Parent full name — convenience property.</summary>
    public string ParentFullName => $"{ParentFirstName} {ParentLastName}";

    /// <summary>Parent email — used for all agent communications.</summary>
    public string ParentEmail { get; set; } = string.Empty;

    /// <summary>Parent phone number.</summary>
    public string ParentPhone { get; set; } = string.Empty;

    // -------------------------------------------------------------------------
    // Courses requested
    // -------------------------------------------------------------------------

    /// <summary>Course IDs the student wants to enroll in.</summary>
    public List<string> RequestedCourseIds { get; set; } = [];

    /// <summary>Course IDs successfully confirmed after agent processing.</summary>
    public List<string> ConfirmedCourseIds { get; set; } = [];

    /// <summary>Course IDs placed on waitlist.</summary>
    public List<string> WaitlistedCourseIds { get; set; } = [];

    // -------------------------------------------------------------------------
    // Documents
    // -------------------------------------------------------------------------

    /// <summary>All documents associated with this application (uploaded and pending).</summary>
    public List<Document> Documents { get; set; } = [];

    /// <summary>Documents that are still missing.</summary>
    public List<Document> MissingDocuments => Documents
        .Where(d => d.IsRequired && !d.IsUploaded)
        .ToList();

    /// <summary>Whether all required documents have been uploaded.</summary>
    public bool AllDocumentsUploaded => !MissingDocuments.Any();

    // -------------------------------------------------------------------------
    // Status & timeline
    // -------------------------------------------------------------------------

    /// <summary>Current status of the application.</summary>
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Submitted;

    /// <summary>When the application was submitted.</summary>
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    /// <summary>When the agent last processed this application.</summary>
    public DateTime? LastProcessedAt { get; set; }

    /// <summary>When the application was confirmed or resolved.</summary>
    public DateTime? ResolvedAt { get; set; }

    /// <summary>Target semester (1 or 2).</summary>
    public int Semester { get; set; }

    /// <summary>Target year (e.g. 2026).</summary>
    public int Year { get; set; } = DateTime.UtcNow.Year;

    // -------------------------------------------------------------------------
    // Agent activity
    // -------------------------------------------------------------------------

    /// <summary>Log of all agent actions taken on this application.</summary>
    public List<AgentAction> AgentLog { get; set; } = [];

    /// <summary>Number of follow-up emails sent so far.</summary>
    public int FollowUpCount { get; set; }

    /// <summary>Maximum follow-up attempts before escalating to staff.</summary>
    public int MaxFollowUps { get; set; } = 3;

    /// <summary>Whether the agent has exhausted follow-up attempts.</summary>
    public bool FollowUpsExhausted => FollowUpCount >= MaxFollowUps;

    /// <summary>Reason for escalation to staff (if applicable).</summary>
    public string? EscalationReason { get; set; }

    /// <summary>Internal notes from the agent — visible to staff on the dashboard.</summary>
    public string? AgentNotes { get; set; }
}