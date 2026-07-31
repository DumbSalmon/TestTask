using Microsoft.EntityFrameworkCore;
using Nazarov_Test_Task.Core.Data;
using Nazarov_Test_Task.Core.DTOs;
using Nazarov_Test_Task.Core.Models;
using System.Data;

namespace Nazarov_Test_Task.Core.Services;

public class EmployeeService
{
    private readonly AppDbContext _context;

    public EmployeeService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<EmployeeResponseDto>> GetAllAsync()
    {
        return await _context.Employees
            .AsNoTracking()
            .OrderBy(employee => employee.Id)
            .Select(employee => new EmployeeResponseDto
            {
                Id = employee.Id,
                DepartmentId = employee.DepartmentId,
                ChiefId = employee.ChiefId,
                Name = employee.Name,
                Salary = employee.Salary
            })
            .ToListAsync();
    }

    public async Task<EmployeeResponseDto?> GetByIdAsync(int id)
    {
        return await _context.Employees
            .AsNoTracking()
            .Where(employee => employee.Id == id)
            .Select(employee => new EmployeeResponseDto
            {
                Id = employee.Id,
                DepartmentId = employee.DepartmentId,
                ChiefId = employee.ChiefId,
                Name = employee.Name,
                Salary = employee.Salary
            })
            .FirstOrDefaultAsync();
    }

    public Task<bool> DepartmentExistsAsync(int departmentId)
    {
        return _context.Departments.AnyAsync(department => department.Id == departmentId);
    }

    public Task<Employee?> GetEmployeeAsync(int id)
    {
        return _context.Employees.FirstOrDefaultAsync(employee => employee.Id == id);
    }

    public Task<Employee?> GetChiefAsync(int id)
    {
        return _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(employee => employee.Id == id);
    }

    public Task<int> GetManagersCountAsync(int departmentId, int? excludedEmployeeId)
    {
        return _context.Employees.CountAsync(employee =>
            employee.DepartmentId == departmentId &&
            employee.ChiefId == null &&
            (!excludedEmployeeId.HasValue || employee.Id != excludedEmployeeId.Value));
    }

    public async Task<EmployeeResponseDto> CreateAsync(EmployeeUpsertDto request)
    {
        var employee = new Employee
        {
            DepartmentId = request.DepartmentId,
            ChiefId = request.ChiefId,
            Name = request.Name,
            Salary = request.Salary
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        return ToResponseDto(employee);
    }

    public async Task<EmployeeResponseDto> UpdateAsync(Employee employee, EmployeeUpsertDto request)
    {
        employee.DepartmentId = request.DepartmentId;
        employee.ChiefId = request.ChiefId;
        employee.Name = request.Name;
        employee.Salary = request.Salary;

        await _context.SaveChangesAsync();

        return ToResponseDto(employee);
    }

    public async Task DeleteAsync(Employee employee)
    {
        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();
    }

    public async Task<List<UpdateSalaryForDepartmentResultDto>> UpdateSalaryForDepartmentAsync(
    UpdateSalaryForDepartmentDto request)
    {
        return await _context.Database
            .SqlQuery<UpdateSalaryForDepartmentResultDto>($"""
            SELECT
                id AS "Id",
                "ID Отдела" AS "DepartmentId",
                "ID Руководителя" AS "ChiefId",
                "Сотрудник" AS "Name",
                "Старая ЗП" AS "OldSalary",
                "Обновленная ЗП" AS "NewSalary"
            FROM public.updatesalaryfordepartment({request.DepartmentId}, {request.Percent})
            """)
            .ToListAsync();
    }

    private static EmployeeResponseDto ToResponseDto(Employee employee)
    {
        return new EmployeeResponseDto
        {
            Id = employee.Id,
            DepartmentId = employee.DepartmentId,
            ChiefId = employee.ChiefId,
            Name = employee.Name,
            Salary = employee.Salary
        };
    }
}