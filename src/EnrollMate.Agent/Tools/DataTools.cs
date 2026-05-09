using System.ComponentModel;
using System.Text.Json;
using EnrollMate.Data.Interfaces;
using Microsoft.SemanticKernel;

namespace EnrollMate.Agent.Tools;

public class DataTools(
    IApplicationRepository applications,
    ICourseRepository courses,
    IDocumentRepository documents)
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
            application.RequestedCourseIds,
            application.Status,
            application.Semester,
            application.Year,
            application.FollowUpCount,
            application.FollowUpsExhausted
        });
    }

    [KernelFunction]
    [Description("Checks whether a student is eligible for a course — validates year level, prerequisites, and zone.")]
    public async Task<string> CheckEligibility(string applicationId, string courseId)
    {
        var application = await applications.GetByIdAsync(applicationId);
        if (application is null)
            return $"Application '{applicationId}' not found.";

        var course = await courses.GetByIdAsync(courseId);
        if (course is null)
            return $"Course '{courseId}' not found.";

        var reasons = new List<string>();

        if (!course.EligibleYearLevels.Contains(application.Student.YearLevel))
            reasons.Add($"Student is Year {application.Student.YearLevel} but course requires: {string.Join(", ", course.EligibleYearLevels)}.");

        if (course.HasPrerequisites)
        {
            var unmet = course.PrerequisiteCourseIds
                .Except(application.ConfirmedCourseIds)
                .ToList();

            if (unmet.Count > 0)
                reasons.Add($"Missing prerequisites: {string.Join(", ", unmet)}.");
        }

        if (reasons.Count > 0)
            return $"Not eligible: {string.Join(" ", reasons)}";

        return $"Eligible. Course '{course.Name}' accepts Year {application.Student.YearLevel}.";
    }

    [KernelFunction]
    [Description("Returns the list of required documents for a given year level.")]
    public Task<string> GetRequiredDocuments(int yearLevel)
    {
        // Standard required documents for all year levels
        var required = new List<string>
        {
            "Birth Certificate",
            "Immunisation Record",
            "Proof of Address"
        };

        // Year 7 entry requires transcript from primary school
        if (yearLevel == 7)
            required.Add("Previous School Transcript");

        // Years 10–12 always require transcript
        if (yearLevel >= 10)
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
    [Description("Returns availability and capacity info for a course.")]
    public async Task<string> GetCourseAvailability(string courseId)
    {
        var course = await courses.GetByIdAsync(courseId);
        if (course is null)
            return $"Course '{courseId}' not found.";

        return JsonSerializer.Serialize(new
        {
            course.Id,
            course.Name,
            course.Code,
            course.Capacity,
            course.EnrolledCount,
            course.AvailablePlaces,
            course.HasAvailablePlaces,
            course.WaitlistedCount,
            course.Schedule,
            course.TeacherName,
            course.Room
        });
    }
}
