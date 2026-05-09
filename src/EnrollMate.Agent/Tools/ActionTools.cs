using System.ComponentModel;
using EnrollMate.Data.Interfaces;
using EnrollMate.Shared.Enums;
using EnrollMate.Shared.Models;
using Microsoft.SemanticKernel;

namespace EnrollMate.Agent.Tools;

public class ActionTools(
    IApplicationRepository applications,
    ICourseRepository courses)
{
    [KernelFunction]
    [Description("Sends a personalised email to the parent via AWS SES. Returns success or failure.")]
    public async Task<string> SendEmailViaSES(string applicationId, string to, string subject, string body)
    {
        var application = await applications.GetByIdAsync(applicationId);
        if (application is null)
            return $"Application '{applicationId}' not found — email not sent.";

        // Mock: log the action instead of calling real SES
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
    [Description("Confirms enrollment for a student in a course. Updates course enrolled count and application confirmed courses.")]
    public async Task<string> EnrollStudent(string applicationId, string courseId)
    {
        var application = await applications.GetByIdAsync(applicationId);
        if (application is null)
            return $"Application '{applicationId}' not found.";

        var course = await courses.GetByIdAsync(courseId);
        if (course is null)
            return $"Course '{courseId}' not found.";

        if (!course.HasAvailablePlaces)
            return $"Course '{course.Name}' is full — cannot enroll. Use waitlist instead.";

        if (!application.ConfirmedCourseIds.Contains(courseId))
            application.ConfirmedCourseIds.Add(courseId);

        await courses.IncrementEnrolledCountAsync(courseId);

        var action = new AgentAction
        {
            ApplicationId = applicationId,
            Type = AgentActionType.ConfirmedEnrollment,
            Description = $"Student enrolled in {course.Name} ({course.Code}).",
            ToolName = "EnrollStudent",
            ToolResult = "Enrolled"
        };

        application.AgentLog.Add(action);
        await applications.UpdateAsync(application);

        return $"Student enrolled in '{course.Name}'. Available places remaining: {course.AvailablePlaces - 1}.";
    }

    [KernelFunction]
    [Description("Places a student on the waitlist for a full course.")]
    public async Task<string> PlaceOnWaitlist(string applicationId, string courseId)
    {
        var application = await applications.GetByIdAsync(applicationId);
        if (application is null)
            return $"Application '{applicationId}' not found.";

        var course = await courses.GetByIdAsync(courseId);
        if (course is null)
            return $"Course '{courseId}' not found.";

        if (!application.WaitlistedCourseIds.Contains(courseId))
            application.WaitlistedCourseIds.Add(courseId);

        await courses.IncrementWaitlistedCountAsync(courseId);

        var action = new AgentAction
        {
            ApplicationId = applicationId,
            Type = AgentActionType.PlacedOnWaitlist,
            Description = $"Student placed on waitlist for {course.Name} ({course.Code}).",
            ToolName = "PlaceOnWaitlist",
            ToolResult = $"WaitlistPosition:{course.WaitlistedCount + 1}"
        };

        application.AgentLog.Add(action);
        await applications.UpdateAsync(application);

        return $"Student placed on waitlist for '{course.Name}'. Waitlist position: {course.WaitlistedCount + 1}.";
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

        // Mock: real implementation would publish to SNS
        return $"Staff notification created for application '{applicationId}'. Summary: {summary}";
    }

    [KernelFunction]
    [Description("Schedules a follow-up action for an application after a delay. Used when waiting for parent to upload documents.")]
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

        // Mock: real implementation would send a delayed message to SQS
        return $"Follow-up scheduled in {delayHours} hours. This is attempt {application.FollowUpCount} of {application.MaxFollowUps}.";
    }
}
