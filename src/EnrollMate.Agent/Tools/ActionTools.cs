using System.ComponentModel;
using EnrollMate.Data.Interfaces;
using EnrollMate.Shared.Enums;
using EnrollMate.Shared.Models;
using Microsoft.SemanticKernel;

namespace EnrollMate.Agent.Tools;

public class ActionTools(
    IApplicationRepository applications,
    ISchoolRepository schools)
{
    [KernelFunction]
    [Description("Sends a personalised email to the parent via AWS SES. Returns success or failure.")]
    public async Task<string> SendEmailViaSES(string applicationId, string to, string subject, string body)
    {
        var application = await applications.GetByIdAsync(applicationId);
        if (application is null)
            return $"Application '{applicationId}' not found — email not sent.";

        var action = new AgentAction
        {
            ApplicationId = applicationId,
            Type = AgentActionType.SentEmail,
            Description = $"Email sent to {to} — Subject: {subject}",
            ToolName = "SendEmailViaSES",
            ToolResult = "Sent (mock)",
            Succeeded = true
        };

        application.AgentLog.Add(action);
        await applications.UpdateAsync(application);

        return $"Email sent to {to}.";
    }

    [KernelFunction]
    [Description("Updates the status of an enrollment application.")]
    public async Task<string> UpdateApplicationStatus(string applicationId, string status)
    {
        if (!Enum.TryParse<ApplicationStatus>(status, out var parsed))
            return $"Unknown status '{status}'. Valid values: {string.Join(", ", Enum.GetNames<ApplicationStatus>())}.";

        var application = await applications.GetByIdAsync(applicationId);
        if (application is null)
            return $"Application '{applicationId}' not found.";

        var previous = application.Status.ToString();
        application.Status = parsed;
        application.LastProcessedAt = DateTime.UtcNow;

        if (parsed is ApplicationStatus.Confirmed or ApplicationStatus.Rejected
            or ApplicationStatus.Waitlisted or ApplicationStatus.Abandoned)
        {
            application.ResolvedAt = DateTime.UtcNow;
        }

        await applications.UpdateAsync(application);

        return $"Application '{applicationId}' status updated from {previous} to {parsed}.";
    }

    [KernelFunction]
    [Description("Confirms enrollment for a student at a school. Updates the school's enrolled count and the application's confirmed schools.")]
    public async Task<string> EnrollStudent(string applicationId, string schoolId)
    {
        var application = await applications.GetByIdAsync(applicationId);
        if (application is null)
            return $"Application '{applicationId}' not found.";

        var school = await schools.GetByIdAsync(schoolId);
        if (school is null)
            return $"School '{schoolId}' not found.";

        if (!school.HasAvailablePlaces)
            return $"'{school.Name}' is at full capacity — cannot enroll. Use PlaceOnWaitlist instead.";

        if (!application.ConfirmedSchoolIds.Contains(schoolId))
            application.ConfirmedSchoolIds.Add(schoolId);

        await schools.IncrementEnrolledCountAsync(schoolId);

        var action = new AgentAction
        {
            ApplicationId = applicationId,
            Type = AgentActionType.ConfirmedEnrollment,
            Description = $"Enrollment confirmed at {school.Name}.",
            ToolName = "EnrollStudent",
            ToolResult = "Enrolled"
        };

        application.AgentLog.Add(action);
        await applications.UpdateAsync(application);

        return $"Student enrolled at '{school.Name}'. Available places remaining: {school.AvailablePlaces - 1}.";
    }

    [KernelFunction]
    [Description("Places a student on the waitlist at a school that is currently at full capacity.")]
    public async Task<string> PlaceOnWaitlist(string applicationId, string schoolId)
    {
        var application = await applications.GetByIdAsync(applicationId);
        if (application is null)
            return $"Application '{applicationId}' not found.";

        var school = await schools.GetByIdAsync(schoolId);
        if (school is null)
            return $"School '{schoolId}' not found.";

        if (!application.WaitlistedSchoolIds.Contains(schoolId))
            application.WaitlistedSchoolIds.Add(schoolId);

        await schools.IncrementWaitlistedCountAsync(schoolId);

        var action = new AgentAction
        {
            ApplicationId = applicationId,
            Type = AgentActionType.PlacedOnWaitlist,
            Description = $"Student placed on waitlist at {school.Name}.",
            ToolName = "PlaceOnWaitlist",
            ToolResult = $"WaitlistPosition:{school.WaitlistedCount + 1}"
        };

        application.AgentLog.Add(action);
        await applications.UpdateAsync(application);

        return $"Student placed on waitlist at '{school.Name}'. Waitlist position: {school.WaitlistedCount + 1}.";
    }

    [KernelFunction]
    [Description("Creates a staff notification and escalates the application for manual review.")]
    public async Task<string> CreateStaffNotification(string applicationId, string summary)
    {
        var application = await applications.GetByIdAsync(applicationId);
        if (application is null)
            return $"Application '{applicationId}' not found.";

        application.EscalationReason = summary;
        application.AgentNotes = summary;

        var action = new AgentAction
        {
            ApplicationId = applicationId,
            Type = AgentActionType.EscalatedToStaff,
            Description = $"Case escalated to staff: {summary}",
            ToolName = "CreateStaffNotification",
            ToolResult = "NotificationSent (mock)",
            StatusChangedTo = nameof(ApplicationStatus.EscalatedToStaff)
        };

        application.AgentLog.Add(action);
        await applications.UpdateAsync(application);

        return $"Staff notification created for application '{applicationId}'.";
    }

    [KernelFunction]
    [Description("Schedules a follow-up action for an application after a delay. Used when waiting for the parent to upload missing documents.")]
    public async Task<string> ScheduleFollowUp(string applicationId, int delayHours)
    {
        var application = await applications.GetByIdAsync(applicationId);
        if (application is null)
            return $"Application '{applicationId}' not found.";

        application.FollowUpCount++;

        var action = new AgentAction
        {
            ApplicationId = applicationId,
            Type = AgentActionType.ScheduledFollowUp,
            Description = $"Follow-up scheduled in {delayHours} hours (attempt {application.FollowUpCount} of {application.MaxFollowUps}).",
            ToolName = "ScheduleFollowUp",
            ToolResult = $"Scheduled (mock) — due at {DateTime.UtcNow.AddHours(delayHours):u}"
        };

        application.AgentLog.Add(action);
        await applications.UpdateAsync(application);

        return $"Follow-up scheduled in {delayHours} hours. Attempt {application.FollowUpCount} of {application.MaxFollowUps}.";
    }
}
