namespace EnrollMate.Shared.Models;

public class Course
{
    /// <summary>Unique identifier for the course.</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Course code (e.g. "MATH101", "ENG201").</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Full course name (e.g. "Mathematics", "English Literature").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Short description of the course.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Which year levels are eligible for this course.</summary>
    public List<int> EligibleYearLevels { get; set; } = [];

    /// <summary>Course codes that must be completed before enrolling (prerequisites).</summary>
    public List<string> PrerequisiteCourseIds { get; set; } = [];

    /// <summary>Semester this offering runs (1 or 2).</summary>
    public int Semester { get; set; }

    /// <summary>Maximum number of students allowed in this course.</summary>
    public int Capacity { get; set; }

    /// <summary>Number of students currently enrolled.</summary>
    public int EnrolledCount { get; set; }

    /// <summary>Number of students on the waitlist.</summary>
    public int WaitlistedCount { get; set; }

    /// <summary>Whether the course still has available places.</summary>
    public bool HasAvailablePlaces => EnrolledCount < Capacity;

    /// <summary>Available places remaining.</summary>
    public int AvailablePlaces => Math.Max(0, Capacity - EnrolledCount);

    /// <summary>Teacher assigned to this course.</summary>
    public string TeacherName { get; set; } = string.Empty;

    /// <summary>Room number or location.</summary>
    public string Room { get; set; } = string.Empty;

    /// <summary>Schedule description (e.g. "Tuesdays and Thursdays 9:00am").</summary>
    public string Schedule { get; set; } = string.Empty;

    /// <summary>Whether this course requires a prerequisite check.</summary>
    public bool HasPrerequisites => PrerequisiteCourseIds.Count > 0;
}