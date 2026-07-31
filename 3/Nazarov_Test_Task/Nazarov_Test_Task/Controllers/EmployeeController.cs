using Microsoft.AspNetCore.Mvc;
using Nazarov_Test_Task.Core.DTOs;
using Nazarov_Test_Task.Core.Services;


namespace Nazarov_Test_Task.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly EmployeeService _employeeService;

    public EmployeesController( EmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeResponseDto>>> GetAll()
    {
        return Ok(await _employeeService.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var employee = await _employeeService.GetByIdAsync(id);

        return new JsonResult(employee)
        {
            StatusCode = StatusCodes.Status200OK
        };
    }

    [HttpPost]
public async Task<ActionResult<EmployeeResponseDto>> Upsert(EmployeeUpsertDto request)
{
        var departmentExists = await _employeeService.DepartmentExistsAsync(request.DepartmentId);

        if (!departmentExists)
    {
        return BadRequest("Отдел с указанным DepartmentId не найден.");
    }

    if (request.ChiefId is null)
    {
    var managersCount = await _employeeService.GetManagersCountAsync(
        request.DepartmentId,
        request.Id);

    if (managersCount >= 1)
        {
        return BadRequest("В отделе уже есть руководитель.");
        }
    }
    if (request.ChiefId.HasValue)
    {
            var chief = await _employeeService.GetChiefAsync(request.ChiefId.Value);

            if (chief is null)
        {
            return BadRequest("Руководитель с указанным ChiefId не найден.");
        }

        if (request.Id.HasValue && request.Id.Value == request.ChiefId.Value)
        {
            return BadRequest("Сотрудник не может быть руководителем самого себя.");
        }
    }

    if (!request.Id.HasValue)
{
    var response = await _employeeService.CreateAsync(request);

    return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
}

    var employee = await _employeeService.GetEmployeeAsync(request.Id.Value);

    if (employee is null)
    {
        return NotFound("Сотрудник с указанным Id не найден.");
    }

    return Ok(await _employeeService.UpdateAsync(employee, request));
}

[HttpDelete("{id:int}")]
public async Task<IActionResult> Delete(int id)
{
        var employee = await _employeeService.GetEmployeeAsync(id);

        if (employee is null)
    {
        return NotFound("Сотрудник с указанным Id не найден.");
    }

    if (employee.ChiefId is null)
    {
        return BadRequest("Нельзя удалить руководителя.");
    }

    await _employeeService.DeleteAsync(employee);

    return Content(string.Empty);
}

    [HttpPost("salary-update")]
    public async Task<ActionResult<IEnumerable<UpdateSalaryForDepartmentResultDto>>> UpdateSalaryForDepartment(
        UpdateSalaryForDepartmentDto request)
    {
        return Ok(await _employeeService.UpdateSalaryForDepartmentAsync(request));
    }
}

