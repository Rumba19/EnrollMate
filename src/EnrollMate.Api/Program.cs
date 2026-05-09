using EnrollMate.Agent;
using EnrollMate.Agent.Tools;
using EnrollMate.Api.Endpoints;
using EnrollMate.Data.Interfaces;
using EnrollMate.Data.Mock;
using Microsoft.SemanticKernel;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));

// ── In-memory seed data ───────────────────────────────────────────────────────
var courses = SeedData.Courses();
var applications = SeedData.Applications(courses);
var documents = SeedData.Documents(applications);

builder.Services.AddSingleton(courses);
builder.Services.AddSingleton(applications);
builder.Services.AddSingleton(documents);

builder.Services.AddSingleton<ICourseRepository, MockCourseRepository>();
builder.Services.AddSingleton<IApplicationRepository, MockApplicationRepository>();
builder.Services.AddSingleton<IDocumentRepository, MockDocumentRepository>();

// ── Semantic Kernel ───────────────────────────────────────────────────────────
var kernelBuilder = builder.Services.AddKernel();
kernelBuilder.Plugins.AddFromType<DataTools>("DataTools");
kernelBuilder.Plugins.AddFromType<ActionTools>("ActionTools");

// LLM connector — add AWS Bedrock here when AWS credentials are configured:
// var modelId = builder.Configuration["Aws:BedrockModelId"]!;
// kernelBuilder.AddBedrockChatCompletionService(modelId);

// ── Agent ─────────────────────────────────────────────────────────────────────
builder.Services.AddScoped<AgentOrchestrator>();

// ── App pipeline ──────────────────────────────────────────────────────────────
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

ApplicationsEndpoints.Map(app);
AgentEndpoints.Map(app);
NotificationsEndpoints.Map(app);

app.Run();
