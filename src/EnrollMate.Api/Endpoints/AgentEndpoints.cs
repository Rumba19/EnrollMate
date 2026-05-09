using EnrollMate.Agent;
using EnrollMate.Data.Interfaces;

namespace EnrollMate.Api.Endpoints;

public static class AgentEndpoints
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/agent/log", GetAgentLog);
        app.MapPost("/agent/run/{id}", RunAgent);
    }

    private static async Task<IResult> GetAgentLog(IApplicationRepository applications)
    {
        var all = await applications.GetAllAsync();

        var log = all
            .SelectMany(a => a.AgentLog.Select(action => new
            {
                action.OccurredAt,
                ApplicationId = a.Id,
                Student = a.Student.FullName,
                action.Type,
                action.Description,
                action.ToolName,
                action.ToolResult,
                action.Succeeded,
                action.StatusChangedTo
            }))
            .OrderByDescending(e => e.OccurredAt);

        return Results.Ok(log);
    }

    private static async Task<IResult> RunAgent(
        string id,
        AgentOrchestrator orchestrator,
        IApplicationRepository applications)
    {
        var application = await applications.GetByIdAsync(id);
        if (application is null)
            return Results.NotFound($"Application '{id}' not found.");

        try
        {
            var result = await orchestrator.RunAsync(id);

            if (!result.Succeeded)
                return Results.BadRequest(new { result.Summary });

            return Results.Ok(new
            {
                ApplicationId = id,
                result.FinalStatus,
                result.ActionsCount,
                result.Summary
            });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("IChatCompletionService"))
        {
            return Results.Problem(
                title: "LLM not configured",
                detail: "No chat completion service is registered. Add an AWS Bedrock connector in Program.cs to run the agent.",
                statusCode: 503);
        }
    }
}
