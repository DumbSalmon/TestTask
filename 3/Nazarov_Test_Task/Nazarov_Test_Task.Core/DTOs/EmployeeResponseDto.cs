namespace Nazarov_Test_Task.Core.DTOs;

public class EmployeeResponseDto
{
    public int Id { get; set; }

    public int DepartmentId { get; set; }

    public int? ChiefId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Salary { get; set; }
}