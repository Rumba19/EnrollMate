using EnrollMate.Data.Interfaces;
using EnrollMate.Shared.Enums;

namespace EnrollMate.Api.Endpoints;

public static class NotificationsEndpoints
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/notifications", GetNotifications);
    }

    private static async Task<IResult> GetNotifications(IApplicationRepository applications)
    {
        var all = await applications.GetAllAsync();

        var notifications = all
            .Where(a => a.Status == ApplicationStatus.EscalatedToStaff)
            .Select(a => new
            {
                a.Id,
                Student = a.Student.FullName,
                a.Student.YearLevel,
                Parent = new { a.ParentFullName, a.ParentEmail },
                a.EscalationReason,
                a.AgentNotes,
                a.SubmittedAt,
                a.LastProcessedAt,
                AgentActionsCount = a.AgentLog.Count
            })
            .OrderByDescending(n => n.LastProcessedAt);

        return Results.Ok(notifications);
    }
}
