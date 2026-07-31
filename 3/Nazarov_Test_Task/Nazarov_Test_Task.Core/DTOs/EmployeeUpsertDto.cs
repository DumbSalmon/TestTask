using System.ComponentModel.DataAnnotations;

namespace Nazarov_Test_Task.Core.DTOs;

public class EmployeeUpsertDto
{
    public int? Id { get; set; }

    [Range(1, int.MaxValue)]
    public int DepartmentId { get; set; }

    public int? ChiefId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Salary { get; set; }
}