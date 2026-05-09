namespace EnrollMate.Shared.Models;

public class Student
{
    /// <summary>Unique identifier for the student.</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Student's first name.</summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Student's last name.</summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>Full name — convenience property.</summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>Date of birth — used for age and year level eligibility checks.</summary>
    public DateOnly DateOfBirth { get; set; }

    /// <summary>Calculated age in years.</summary>
    public int Age => DateOnly.FromDateTime(DateTime.Today).Year - DateOfBirth.Year;

    /// <summary>Year level the student is enrolling into (e.g. 7, 8, 9, 10, 11, 12).</summary>
    public int YearLevel { get; set; }

    /// <summary>Previous school name — used for transcript verification.</summary>
    public string? PreviousSchool { get; set; }

    /// <summary>Home address — used for zone eligibility checks.</summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>Suburb — used for zone boundary checks.</summary>
    public string Suburb { get; set; } = string.Empty;

    /// <summary>Postcode.</summary>
    public string Postcode { get; set; } = string.Empty;

    /// <summary>Any special needs or requirements flagged on the application.</summary>
    public string? SpecialRequirements { get; set; }

    /// <summary>Whether student has special needs — triggers human review.</summary>
    public bool HasSpecialRequirements => !string.IsNullOrWhiteSpace(SpecialRequirements);
}