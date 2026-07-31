using System.ComponentModel.DataAnnotations;

namespace Nazarov_Test_Task.Core.DTOs;

public class UpdateSalaryForDepartmentDto
{
    [Range(1, int.MaxValue)]
    public int DepartmentId { get; set; }

    [Range(1, int.MaxValue)]
    public int Percent { get; set; }
}