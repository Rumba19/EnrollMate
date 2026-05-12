using EnrollMate.Shared.Enums;
using EnrollMate.Shared.Models;

namespace EnrollMate.Data.Mock;

public static class SeedData
{
    public static List<School> Schools() =>
    [
        new School
        {
            Id = "school-greenfield-secondary",
            Name = "Greenfield Secondary College",
            Description = "A leading secondary school serving Years 7–12 in the Greenfield district.",
            Address = "1 College Drive",
            Suburb = "Greenfield",
            Postcode = "3056",
            ContactEmail = "admin@greenfieldsecondary.edu",
            PhoneNumber = "03 9000 1111",
            PrincipalName = "Mr. Alan Brooks",
            YearLevels = [7, 8, 9, 10, 11, 12],
            ZoneSuburbs = ["Greenfield", "Maplewood", "Riverside"],
            Capacity = 120,
            EnrolledCount = 95
        },
        new School
        {
            Id = "school-riverside-academy",
            Name = "Riverside Academy",
            Description = "A selective secondary school for Years 7–12 with a focus on STEM.",
            Address = "45 Academy Road",
            Suburb = "Riverside",
            Postcode = "3057",
            ContactEmail = "enrol@riversideacademy.edu",
            PhoneNumber = "03 9000 2222",
            PrincipalName = "Ms. Patricia Wong",
            YearLevels = [7, 8, 9, 10, 11, 12],
            ZoneSuburbs = ["Riverside", "Greenfield", "Northside"],
            Capacity = 80,
            EnrolledCount = 80  // full — students will be waitlisted
        },
        new School
        {
            Id = "school-northside-primary",
            Name = "Northside Primary School",
            Description = "A welcoming primary school serving Years K–6 in the Northside area.",
            Address = "88 School Lane",
            Suburb = "Northside",
            Postcode = "3058",
            ContactEmail = "office@northsideprimary.edu",
            PhoneNumber = "03 9000 3333",
            PrincipalName = "Mrs. Sandra Hill",
            YearLevels = [1, 2, 3, 4, 5, 6],
            ZoneSuburbs = ["Northside", "Maplewood"],
            Capacity = 200,
            EnrolledCount = 160
        }
    ];

    public static List<Application> Applications()
    {
        // ── Sofia Nguyen — main demo scenario ──────────────────────────────────
        // Year 10 student applying to Greenfield Secondary and Riverside Academy.
        // Riverside is full — she'll be waitlisted there.
        // Missing: immunisation record and proof of address.
        var sofiaId = "app-sofia-nguyen";
        var sofia = new Application
        {
            Id = sofiaId,
            ParentFirstName = "Linh",
            ParentLastName = "Nguyen",
            ParentEmail = "linh.nguyen@email.com",
            ParentPhone = "0412 345 678",
            Student = new Student
            {
                Id = "student-sofia",
                FirstName = "Sofia",
                LastName = "Nguyen",
                DateOfBirth = new DateOnly(2010, 3, 14),
                YearLevel = 10,
                PreviousSchool = "Riverside Middle School",
                Address = "42 Maple Street",
                Suburb = "Greenfield",
                Postcode = "3056"
            },
            RequestedSchoolIds = ["school-greenfield-secondary", "school-riverside-academy"],
            IntakeYear = 2026,
            Status = ApplicationStatus.Submitted,
            SubmittedAt = DateTime.UtcNow.AddHours(-2),
            Documents =
            [
                new Document
                {
                    ApplicationId = sofiaId,
                    Type = DocumentType.BirthCertificate,
                    Name = "Birth Certificate",
                    IsRequired = true,
                    IsUploaded = true,
                    FileName = "birth_certificate.pdf",
                    StoragePath = "mock/sofia/birth_certificate.pdf",
                    UploadedAt = DateTime.UtcNow.AddHours(-2)
                },
                new Document
                {
                    ApplicationId = sofiaId,
                    Type = DocumentType.PreviousSchoolTranscript,
                    Name = "Previous School Transcript",
                    IsRequired = true,
                    IsUploaded = true,
                    FileName = "transcript.pdf",
                    StoragePath = "mock/sofia/transcript.pdf",
                    UploadedAt = DateTime.UtcNow.AddHours(-2)
                },
                new Document
                {
                    ApplicationId = sofiaId,
                    Type = DocumentType.ImmunisationRecord,
                    Name = "Immunisation Record",
                    IsRequired = true,
                    IsUploaded = false
                },
                new Document
                {
                    ApplicationId = sofiaId,
                    Type = DocumentType.ProofOfAddress,
                    Name = "Proof of Address",
                    IsRequired = true,
                    IsUploaded = false
                }
            ]
        };

        // ── James Obi — already confirmed overnight ────────────────────────────
        // Applied to Greenfield Secondary — all docs present, enrolled.
        var jamesId = "app-james-obi";
        var james = new Application
        {
            Id = jamesId,
            ParentFirstName = "Emeka",
            ParentLastName = "Obi",
            ParentEmail = "emeka.obi@email.com",
            ParentPhone = "0423 456 789",
            Student = new Student
            {
                Id = "student-james",
                FirstName = "James",
                LastName = "Obi",
                DateOfBirth = new DateOnly(2010, 7, 22),
                YearLevel = 10,
                PreviousSchool = "Northside Primary",
                Address = "88 Park Road",
                Suburb = "Greenfield",
                Postcode = "3056"
            },
            RequestedSchoolIds = ["school-greenfield-secondary"],
            ConfirmedSchoolIds = ["school-greenfield-secondary"],
            IntakeYear = 2026,
            Status = ApplicationStatus.Confirmed,
            SubmittedAt = DateTime.UtcNow.AddHours(-10),
            LastProcessedAt = DateTime.UtcNow.AddHours(-8),
            ResolvedAt = DateTime.UtcNow.AddHours(-8),
            Documents =
            [
                new Document
                {
                    ApplicationId = jamesId,
                    Type = DocumentType.BirthCertificate,
                    Name = "Birth Certificate",
                    IsRequired = true,
                    IsUploaded = true,
                    FileName = "birth_certificate.pdf",
                    StoragePath = "mock/james/birth_certificate.pdf",
                    UploadedAt = DateTime.UtcNow.AddHours(-10)
                },
                new Document
                {
                    ApplicationId = jamesId,
                    Type = DocumentType.ImmunisationRecord,
                    Name = "Immunisation Record",
                    IsRequired = true,
                    IsUploaded = true,
                    FileName = "immunisation.pdf",
                    StoragePath = "mock/james/immunisation.pdf",
                    UploadedAt = DateTime.UtcNow.AddHours(-10)
                },
                new Document
                {
                    ApplicationId = jamesId,
                    Type = DocumentType.ProofOfAddress,
                    Name = "Proof of Address",
                    IsRequired = true,
                    IsUploaded = true,
                    FileName = "proof_address.pdf",
                    StoragePath = "mock/james/proof_address.pdf",
                    UploadedAt = DateTime.UtcNow.AddHours(-10)
                }
            ],
            AgentLog =
            [
                new AgentAction
                {
                    ApplicationId = jamesId,
                    Type = AgentActionType.ReadApplication,
                    Description = "Read application for James Obi.",
                    ToolName = "GetApplicationById",
                    OccurredAt = DateTime.UtcNow.AddHours(-8)
                },
                new AgentAction
                {
                    ApplicationId = jamesId,
                    Type = AgentActionType.CheckedEligibility,
                    Description = "Eligibility confirmed — Year 10, in zone for Greenfield Secondary College.",
                    ToolName = "CheckEligibility",
                    ToolResult = "Eligible",
                    OccurredAt = DateTime.UtcNow.AddHours(-8)
                },
                new AgentAction
                {
                    ApplicationId = jamesId,
                    Type = AgentActionType.CheckedDocuments,
                    Description = "All required documents uploaded.",
                    ToolName = "GetUploadedDocuments",
                    ToolResult = "AllUploaded",
                    OccurredAt = DateTime.UtcNow.AddHours(-8)
                },
                new AgentAction
                {
                    ApplicationId = jamesId,
                    Type = AgentActionType.ConfirmedEnrollment,
                    Description = "Enrollment confirmed at Greenfield Secondary College.",
                    ToolName = "EnrollStudent",
                    ToolResult = "Enrolled",
                    StatusChangedTo = nameof(ApplicationStatus.Confirmed),
                    OccurredAt = DateTime.UtcNow.AddHours(-8)
                },
                new AgentAction
                {
                    ApplicationId = jamesId,
                    Type = AgentActionType.SentEmail,
                    Description = "Welcome email sent to emeka.obi@email.com.",
                    ToolName = "SendEmailViaSES",
                    ToolResult = "Sent",
                    OccurredAt = DateTime.UtcNow.AddHours(-8)
                }
            ]
        };

        // ── Mia Torres — escalated to staff (special needs flagged) ────────────
        var miaId = "app-mia-torres";
        var mia = new Application
        {
            Id = miaId,
            ParentFirstName = "Carmen",
            ParentLastName = "Torres",
            ParentEmail = "carmen.torres@email.com",
            ParentPhone = "0434 567 890",
            Student = new Student
            {
                Id = "student-mia",
                FirstName = "Mia",
                LastName = "Torres",
                DateOfBirth = new DateOnly(2010, 11, 5),
                YearLevel = 10,
                PreviousSchool = "Westlake Primary",
                Address = "15 Oak Avenue",
                Suburb = "Greenfield",
                Postcode = "3056",
                SpecialRequirements = "Student has an auditory processing disorder and requires front-row seating and written instructions."
            },
            RequestedSchoolIds = ["school-greenfield-secondary"],
            IntakeYear = 2026,
            Status = ApplicationStatus.EscalatedToStaff,
            SubmittedAt = DateTime.UtcNow.AddHours(-6),
            LastProcessedAt = DateTime.UtcNow.AddHours(-5),
            EscalationReason = "Student has special requirements that need staff review before enrollment can be confirmed.",
            AgentNotes = "All documents present and eligibility checks passed. Escalated due to special needs flag — staff to confirm appropriate support is in place.",
            Documents =
            [
                new Document
                {
                    ApplicationId = miaId,
                    Type = DocumentType.BirthCertificate,
                    Name = "Birth Certificate",
                    IsRequired = true,
                    IsUploaded = true,
                    FileName = "birth_certificate.pdf",
                    StoragePath = "mock/mia/birth_certificate.pdf",
                    UploadedAt = DateTime.UtcNow.AddHours(-6)
                },
                new Document
                {
                    ApplicationId = miaId,
                    Type = DocumentType.SpecialNeedsAssessment,
                    Name = "Special Needs Assessment",
                    IsRequired = true,
                    IsUploaded = true,
                    FileName = "assessment.pdf",
                    StoragePath = "mock/mia/assessment.pdf",
                    UploadedAt = DateTime.UtcNow.AddHours(-6)
                }
            ],
            AgentLog =
            [
                new AgentAction
                {
                    ApplicationId = miaId,
                    Type = AgentActionType.CheckedEligibility,
                    Description = "Eligibility confirmed — Year 10, in zone for Greenfield Secondary College.",
                    ToolName = "CheckEligibility",
                    ToolResult = "Eligible",
                    OccurredAt = DateTime.UtcNow.AddHours(-5)
                },
                new AgentAction
                {
                    ApplicationId = miaId,
                    Type = AgentActionType.EscalatedToStaff,
                    Description = "Escalated — special requirements require staff review.",
                    ToolName = "CreateStaffNotification",
                    ToolResult = "NotificationSent",
                    StatusChangedTo = nameof(ApplicationStatus.EscalatedToStaff),
                    OccurredAt = DateTime.UtcNow.AddHours(-5)
                }
            ]
        };

        return [sofia, james, mia];
    }

    public static List<Document> Documents(List<Application> applications) =>
        applications.SelectMany(a => a.Documents).ToList();
}
