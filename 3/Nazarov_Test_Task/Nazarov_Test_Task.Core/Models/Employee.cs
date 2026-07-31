namespace Nazarov_Test_Task.Core.Models;

public class Employee
{
    public int Id { get; set; }
    public int DepartmentId { get; set; }
    public int? ChiefId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Salary { get; set; }

    public Department Department { get; set; } = null!;
    public Employee? Chief { get; set; }
    public ICollection<Employee> Subordinates { get; set; } = new List<Employee>();
}