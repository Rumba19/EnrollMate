using EnrollMate.Data.Interfaces;
using EnrollMate.Shared.Enums;
using EnrollMate.Shared.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace EnrollMate.Agent;

// Plugins (DataTools, ActionTools) are registered on the Kernel via DI in Program.cs
public class AgentOrchestrator(Kernel kernel, IApplicationRepository applications)
{
    private readonly string _systemPrompt = LoadSystemPrompt();

    public async Task<AgentResult> RunAsync(string applicationId)
    {
        var application = await applications.GetByIdAsync(applicationId);
        if (application is null)
            return new AgentResult(false, $"Application '{applicationId}' not found.");

        if (application.Status is ApplicationStatus.Confirmed
            or ApplicationStatus.Rejected
            or ApplicationStatus.EscalatedToStaff)
        {
            return new AgentResult(false, $"Application '{applicationId}' is already in a terminal state: {application.Status}.");
        }

        await applications.UpdateStatusAsync(applicationId, ApplicationStatus.Processing);

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(_systemPrompt);
        chatHistory.AddUserMessage(BuildUserMessage(application));

        var settings = new PromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        var chat = kernel.GetRequiredService<IChatCompletionService>();
        var response = await chat.GetChatMessageContentAsync(chatHistory, settings, kernel);

        var updated = await applications.GetByIdAsync(applicationId);

        return new AgentResult(
            Succeeded: true,
            Summary: response.Content ?? "Agent completed.",
            FinalStatus: updated?.Status ?? application.Status,
            ActionsCount: updated?.AgentLog.Count ?? 0
        );
    }

    private static string BuildUserMessage(Application application) =>
        $"""
        Process the following enrollment application.

        Application ID : {application.Id}
        Student        : {application.Student.FullName}, Year {application.Student.YearLevel}
        Parent         : {application.ParentFullName} <{application.ParentEmail}>
        Requested courses: {string.Join(", ", application.RequestedCourseIds)}
        Semester/Year  : Semester {application.Semester}, {application.Year}
        Special requirements: {(application.Student.HasSpecialRequirements ? application.Student.SpecialRequirements : "None")}

        Work through the full decision flow now.
        """;

    private static string LoadSystemPrompt()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Prompts", "SystemPrompt.txt");
        return File.Exists(path)
            ? File.ReadAllText(path)
            : FallbackSystemPrompt;
    }

    // Fallback in case the file is not copied to output (e.g. during unit tests)
    private const string FallbackSystemPrompt =
        "You are EnrollMate, an AI agent that processes student enrollment applications. " +
        "Use your tools to check eligibility, verify documents, enroll students, and communicate with parents.";
}

public record AgentResult(
    bool Succeeded,
    string Summary,
    ApplicationStatus FinalStatus = ApplicationStatus.Processing,
    int ActionsCount = 0
);
