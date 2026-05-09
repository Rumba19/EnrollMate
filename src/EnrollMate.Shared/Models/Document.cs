namespace EnrollMate.Shared.Models;

public class Document
{
    /// <summary>Unique identifier for this document record.</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>The application this document belongs to.</summary>
    public string ApplicationId { get; set; } = string.Empty;

    /// <summary>Type of document (matches required document types).</summary>
    public DocumentType Type { get; set; }

    /// <summary>Friendly display name (e.g. "Medical Form", "Previous Transcript").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Whether this document has been uploaded by the parent.</summary>
    public bool IsUploaded { get; set; }

    /// <summary>Whether this document is required for the application to proceed.</summary>
    public bool IsRequired { get; set; } = true;

    /// <summary>File name of the uploaded document.</summary>
    public string? FileName { get; set; }

    /// <summary>S3 key or mock path where the file is stored.</summary>
    public string? StoragePath { get; set; }

    /// <summary>When the document was uploaded.</summary>
    public DateTime? UploadedAt { get; set; }

    /// <summary>Any notes from staff about this document.</summary>
    public string? Notes { get; set; }
}

public enum DocumentType
{
    MedicalForm,
    PreviousSchoolTranscript,
    ProofOfAddress,
    BirthCertificate,
    ImmunisationRecord,
    ParentalConsentForm,
    SpecialNeedsAssessment,
    Other
}