namespace EnrollMate.Shared.Models;

public class School
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string Suburb { get; set; } = string.Empty;

    public string Postcode { get; set; } = string.Empty;

    public string ContactEmail { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string PrincipalName { get; set; } = string.Empty;

    /// <summary>Year levels this school serves (e.g. [7,8,9,10,11,12]).</summary>
    public List<int> YearLevels { get; set; } = [];

    /// <summary>Suburbs considered in-zone for this school.</summary>
    public List<string> ZoneSuburbs { get; set; } = [];

    public int Capacity { get; set; }

    public int EnrolledCount { get; set; }

    public int WaitlistedCount { get; set; }

    public bool HasAvailablePlaces => EnrolledCount < Capacity;

    public int AvailablePlaces => Math.Max(0, Capacity - EnrolledCount);

    public bool ServesYearLevel(int yearLevel) => YearLevels.Contains(yearLevel);

    public bool IsInZone(string suburb) =>
        ZoneSuburbs.Contains(suburb, StringComparer.OrdinalIgnoreCase);
}
