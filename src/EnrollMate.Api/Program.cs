using EnrollMate.Data.Interfaces;
using EnrollMate.Data.Mock;
using EnrollMate.Shared.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Seed shared in-memory data
var courses = SeedData.Courses();
var applications = SeedData.Applications(courses);
var documents = SeedData.Documents(applications);

// Register as singletons so all repositories share the same lists
builder.Services.AddSingleton(courses);
builder.Services.AddSingleton(applications);
builder.Services.AddSingleton(documents);

builder.Services.AddSingleton<ICourseRepository, MockCourseRepository>();
builder.Services.AddSingleton<IApplicationRepository, MockApplicationRepository>();
builder.Services.AddSingleton<IDocumentRepository, MockDocumentRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();

app.Run();
