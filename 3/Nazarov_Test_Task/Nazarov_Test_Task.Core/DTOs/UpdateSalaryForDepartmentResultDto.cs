namespace Nazarov_Test_Task.Core.DTOs;

public class UpdateSalaryForDepartmentResultDto
{
    public int Id { get; set; }

    public int DepartmentId { get; set; }

    public int? ChiefId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int OldSalary { get; set; }

    public int NewSalary { get; set; }
}