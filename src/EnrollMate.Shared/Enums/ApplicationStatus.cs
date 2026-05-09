namespace EnrollMate.Shared.Enums;

public enum ApplicationStatus
{
    /// <summary>Application received, not yet processed by agent.</summary>
    Submitted,

    /// <summary>Agent is currently processing this application.</summary>
    Processing,

    /// <summary>Agent is waiting for parent to upload missing documents.</summary>
    PendingDocuments,

    /// <summary>All documents received, agent re-evaluating.</summary>
    UnderReview,

    /// <summary>Enrollment confirmed, welcome email sent.</summary>
    Confirmed,

    /// <summary>Course full, student placed on waitlist.</summary>
    Waitlisted,

    /// <summary>Application rejected — does not meet eligibility rules.</summary>
    Rejected,

    /// <summary>Edge case flagged — staff must review manually.</summary>
    EscalatedToStaff,

    /// <summary>Parent did not respond after max follow-up attempts.</summary>
    Abandoned
}