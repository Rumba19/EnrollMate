using EnrollMate.Data.Interfaces;
using EnrollMate.Shared.Enums;
using EnrollMate.Shared.Models;

namespace EnrollMate.Data.Mock;

public class MockApplicationRepository : IApplicationRepository
{
    private readonly List<Application> _applications;

    public MockApplicationRepository(List<Application> applications)
    {
        _applications = applications;
    }

    public Task<Application?> GetByIdAsync(string applicationId)
    {
        var result = _applications.FirstOrDefault(a => a.Id == applicationId);
        return Task.FromResult(result);
    }

    public Task<IEnumerable<Application>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<Application>>(_applications);
    }

    public Task<IEnumerable<Application>> GetPendingAsync()
    {
        var pending = _applications.Where(a =>
            a.Status == ApplicationStatus.Submitted ||
            a.Status == ApplicationStatus.Processing ||
            a.Status == ApplicationStatus.UnderReview);

        return Task.FromResult<IEnumerable<Application>>(pending);
    }

    public Task AddAsync(Application application)
    {
        _applications.Add(application);
        return Task.CompletedTask;
    }

    public Task UpdateStatusAsync(string applicationId, ApplicationStatus status)
    {
        var application = _applications.FirstOrDefault(a => a.Id == applicationId);
        if (application is not null)
            application.Status = status;

        return Task.CompletedTask;
    }

    public Task UpdateAsync(Application application)
    {
        var index = _applications.FindIndex(a => a.Id == application.Id);
        if (index >= 0)
            _applications[index] = application;

        return Task.CompletedTask;
    }
}
