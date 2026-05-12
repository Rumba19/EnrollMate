using EnrollMate.Data.Interfaces;
using EnrollMate.Shared.Enums;
using EnrollMate.Shared.Models;

namespace EnrollMate.Api.Endpoints;

public static class ApplicationsEndpoints
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/applications", SubmitApplication);
        app.MapGet("/applications", GetAllApplications);
        app.MapGet("/applications/{id}", GetApplicationById);
        app.MapPost("/applications/{id}/documents", UploadDocument);
    }

    private static async Task<IResult> SubmitApplication(
        SubmitApplicationRequest request,
        IApplicationRepository applications,
        ISchoolRepository schools)
    {
        foreach (var schoolId in request.RequestedSchoolIds)
        {
            var school = await schools.GetByIdAsync(schoolId);
            if (school is null)
                return Results.BadRequest($"School '{schoolId}' not found.");
        }

        var applicationId = Guid.NewGuid().ToString();

        var application = new Application
        {
            Id = applicationId,
            ParentFirstName = request.ParentFirstName,
            ParentLastName = request.ParentLastName,
            ParentEmail = request.ParentEmail,
            ParentPhone = request.ParentPhone,
            Student = new Student
            {
                FirstName = request.StudentFirstName,
                LastName = request.StudentLastName,
                DateOfBirth = request.StudentDateOfBirth,
                YearLevel = request.StudentYearLevel,
                PreviousSchool = request.StudentPreviousSchool,
                Address = request.StudentAddress,
                Suburb = request.StudentSuburb,
                Postcode = request.StudentPostcode,
                SpecialRequirements = request.StudentSpecialRequirements
            },
            RequestedSchoolIds = request.RequestedSchoolIds,
            IntakeYear = request.IntakeYear,
            Status = ApplicationStatus.Submitted
        };

        await applications.AddAsync(application);

        return Results.Created($"/applications/{applicationId}", new
        {
            application.Id,
            application.Status,
            application.Student.FullName,
            application.ParentEmail,
            application.RequestedSchoolIds,
            application.IntakeYear
        });
    }

    private static async Task<IResult> GetAllApplications(IApplicationRepository applications)
    {
        var all = await applications.GetAllAsync();

        var result = all.Select(a => new
        {
            a.Id,
            Student = a.Student.FullName,
            a.Status,
            a.IntakeYear,
            a.SubmittedAt,
            a.LastProcessedAt,
            RequestedSchools = a.RequestedSchoolIds.Count,
            MissingDocuments = a.MissingDocuments.Select(d => d.Name),
            AgentActionsCount = a.AgentLog.Count
        });

        return Results.Ok(result);
    }

    private static async Task<IResult> GetApplicationById(string id, IApplicationRepository applications)
    {
        var application = await applications.GetByIdAsync(id);
        if (application is null)
            return Results.NotFound($"Application '{id}' not found.");

        return Results.Ok(new
        {
            application.Id,
            Student = new
            {
                application.Student.FullName,
                application.Student.YearLevel,
                application.Student.DateOfBirth,
                application.Student.Suburb,
                application.Student.Postcode,
                application.Student.HasSpecialRequirements,
                application.Student.SpecialRequirements
            },
            Parent = new
            {
                application.ParentFullName,
                application.ParentEmail,
                application.ParentPhone
            },
            application.Status,
            application.RequestedSchoolIds,
            application.ConfirmedSchoolIds,
            application.WaitlistedSchoolIds,
            application.IntakeYear,
            application.SubmittedAt,
            application.LastProcessedAt,
            application.ResolvedAt,
            Documents = application.Documents.Select(d => new
            {
                d.Id,
                d.Name,
                d.Type,
                d.IsUploaded,
                d.IsRequired,
                d.UploadedAt
            }),
            application.AllDocumentsUploaded,
            application.FollowUpCount,
            application.EscalationReason,
            application.AgentNotes,
            AgentLog = application.AgentLog.Select(a => new
            {
                a.OccurredAt,
                a.Type,
                a.Description,
                a.Succeeded
            })
        });
    }

    private static async Task<IResult> UploadDocument(
        string id,
        UploadDocumentRequest request,
        IApplicationRepository applications,
        IDocumentRepository documents)
    {
        var application = await applications.GetByIdAsync(id);
        if (application is null)
            return Results.NotFound($"Application '{id}' not found.");

        if (!Enum.TryParse<DocumentType>(request.DocumentType, out var docType))
            return Results.BadRequest($"Unknown document type '{request.DocumentType}'. Valid values: {string.Join(", ", Enum.GetNames<DocumentType>())}.");

        var existing = application.Documents.FirstOrDefault(d => d.Type == docType && !d.IsUploaded);

        if (existing is not null)
        {
            var storagePath = $"mock/{id}/{request.FileName}";
            await documents.MarkUploadedAsync(existing.Id, request.FileName, storagePath);

            return Results.Ok(new
            {
                DocumentId = existing.Id,
                existing.Name,
                FileName = request.FileName,
                StoragePath = storagePath,
                UploadedAt = DateTime.UtcNow,
                Message = "Document uploaded. If all required documents are now present, re-run the agent to continue processing."
            });
        }

        var newDoc = new Document
        {
            ApplicationId = id,
            Type = docType,
            Name = request.DocumentType,
            IsRequired = false,
            IsUploaded = true,
            FileName = request.FileName,
            StoragePath = $"mock/{id}/{request.FileName}",
            UploadedAt = DateTime.UtcNow
        };

        await documents.AddAsync(newDoc);
        application.Documents.Add(newDoc);
        await applications.UpdateAsync(application);

        return Results.Ok(new
        {
            DocumentId = newDoc.Id,
            newDoc.Name,
            FileName = request.FileName,
            newDoc.StoragePath,
            newDoc.UploadedAt
        });
    }
}

public record SubmitApplicationRequest(
    string ParentFirstName,
    string ParentLastName,
    string ParentEmail,
    string ParentPhone,
    string StudentFirstName,
    string StudentLastName,
    DateOnly StudentDateOfBirth,
    int StudentYearLevel,
    string StudentAddress,
    string StudentSuburb,
    string StudentPostcode,
    List<string> RequestedSchoolIds,
    int IntakeYear,
    string? StudentPreviousSchool = null,
    string? StudentSpecialRequirements = null
);

public record UploadDocumentRequest(
    string DocumentType,
    string FileName
);
