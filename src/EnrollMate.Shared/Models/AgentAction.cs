namespace EnrollMate.Shared.Models;

public class AgentAction
{
    /// <summary>Unique identifier for this action.</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>The application this action was taken on.</summary>
    public string ApplicationId { get; set; } = string.Empty;

    /// <summary>When this action occurred.</summary>
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    /// <summary>The type of action the agent took.</summary>
    public AgentActionType Type { get; set; }

    /// <summary>Human-readable description of what the agent did.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>The tool the agent called (e.g. "CheckEligibility", "SendEmailViaSES").</summary>
    public string ToolName { get; set; } = string.Empty;

    /// <summary>Result or output from the tool call.</summary>
    public string? ToolResult { get; set; }

    /// <summary>Whether this action succeeded.</summary>
    public bool Succeeded { get; set; } = true;

    /// <summary>Error message if the action failed.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Status the application moved to after this action (if changed).</summary>
    public string? StatusChangedTo { get; set; }
}

public enum AgentActionType
{
    /// <summary>Agent read application data.</summary>
    ReadApplication,

    /// <summary>Agent checked eligibility rules.</summary>
    CheckedEligibility,

    /// <summary>Agent checked document requirements.</summary>
    CheckedDocuments,

    /// <summary>Agent confirmed enrollment.</summary>
    ConfirmedEnrollment,

    /// <summary>Agent sent an email to the parent.</summary>
    SentEmail,

    /// <summary>Agent scheduled a follow-up.</summary>
    ScheduledFollowUp,

    /// <summary>Agent escalated the case to staff.</summary>
    EscalatedToStaff,

    /// <summary>Agent detected an uploaded document.</summary>
    DetectedDocumentUpload,

    /// <summary>Agent placed student on waitlist.</summary>
    PlacedOnWaitlist,

    /// <summary>Agent rejected the application.</summary>
    RejectedApplication,

    /// <summary>Agent queried knowledge base / policy docs.</summary>
    SearchedKnowledgeBase
}