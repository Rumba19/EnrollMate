using System.ComponentModel;
using System.Text.Json;
using EnrollMate.Data.Interfaces;
using Microsoft.SemanticKernel;

namespace EnrollMate.Agent.Tools;

public class DataTools(
    IApplicationRepository applications,
    ISchoolRepository schools)
{
    [KernelFunction]
    [Description("Gets the full enrollment application record by ID.")]
    public async Task<string> GetApplicationById(string applicationId)
    {
        var application = await applications.GetByIdAsync(applicationId);
        if (application is null)
            return $"No application found with ID '{applicationId}'.";

        return JsonSerializer.Serialize(new
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
                application.ParentEmail
            },
            application.RequestedSchoolIds,
            application.Status,
            application.IntakeYear,
            application.FollowUpCount,
            application.FollowUpsExhausted
        });
    }

    [KernelFunction]
    [Description("Checks whether a student is eligible to enroll at a school — validates year level and zone.")]
    public async Task<string> CheckEligibility(string applicationId, string schoolId)
    {
        var application = await applications.GetByIdAsync(applicationId);
        if (application is null)
            return $"Application '{applicationId}' not found.";

        var school = await schools.GetByIdAsync(schoolId);
        if (school is null)
            return $"School '{schoolId}' not found.";

        var reasons = new List<string>();

        if (!school.ServesYearLevel(application.Student.YearLevel))
            reasons.Add($"Student is Year {application.Student.YearLevel} but {school.Name} serves: {string.Join(", ", school.YearLevels)}.");

        if (!school.IsInZone(application.Student.Suburb))
            reasons.Add($"Student's suburb '{application.Student.Suburb}' is not in the zone for {school.Name}. Zone suburbs: {string.Join(", ", school.ZoneSuburbs)}.");

        if (reasons.Count > 0)
            return $"Not eligible: {string.Join(" ", reasons)}";

        return $"Eligible. {school.Name} serves Year {application.Student.YearLevel} and '{application.Student.Suburb}' is in zone.";
    }

    [KernelFunction]
    [Description("Returns the list of required documents for a given year level.")]
    public Task<string> GetRequiredDocuments(int yearLevel)
    {
        var required = new List<string>
        {
            "Birth Certificate",
            "Immunisation Record",
            "Proof of Address"
        };

        if (yearLevel >= 7)
            required.Add("Previous School Transcript");

        return Task.FromResult(
            $"Required documents for Year {yearLevel}: {string.Join(", ", required)}.");
    }

    [KernelFunction]
    [Description("Returns uploaded and missing documents for an application.")]
    public async Task<string> GetUploadedDocuments(string applicationId)
    {
        var application = await applications.GetByIdAsync(applicationId);
        if (application is null)
            return $"Application '{applicationId}' not found.";

        var uploaded = application.Documents.Where(d => d.IsUploaded).Select(d => d.Name).ToList();
        var missing = application.MissingDocuments.Select(d => d.Name).ToList();

        return JsonSerializer.Serialize(new
        {
            AllUploaded = application.AllDocumentsUploaded,
            Uploaded = uploaded,
            Missing = missing
        });
    }

    [KernelFunction]
    [Description("Returns availability and capacity info for a school.")]
    public async Task<string> GetSchoolAvailability(string schoolId)
    {
        var school = await schools.GetByIdAsync(schoolId);
        if (school is null)
            return $"School '{schoolId}' not found.";

        return JsonSerializer.Serialize(new
        {
            school.Id,
            school.Name,
            school.Address,
            school.Suburb,
            school.YearLevels,
            school.Capacity,
            school.EnrolledCount,
            school.AvailablePlaces,
            school.HasAvailablePlaces,
            school.WaitlistedCount,
            school.PrincipalName,
            school.ContactEmail
        });
    }
}
