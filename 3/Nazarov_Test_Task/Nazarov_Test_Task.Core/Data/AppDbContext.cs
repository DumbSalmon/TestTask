using Microsoft.EntityFrameworkCore;
using Nazarov_Test_Task.Core.Models;

namespace Nazarov_Test_Task.Core.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Employee> Employees => Set<Employee>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Department>(entity =>
        {
            entity.ToTable("department", "public");
            entity.HasKey(department => department.Id);
            entity.Property(department => department.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(department => department.Name).HasColumnName("name").HasMaxLength(100);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("employee", "public");
            entity.HasKey(employee => employee.Id);
            entity.Property(employee => employee.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(employee => employee.DepartmentId).HasColumnName("department_id");
            entity.Property(employee => employee.ChiefId).HasColumnName("chief_id");
            entity.Property(employee => employee.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(employee => employee.Salary).HasColumnName("salary").IsRequired();

            entity.HasOne(employee => employee.Department)
                .WithMany(department => department.Employees)
                .HasForeignKey(employee => employee.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(employee => employee.Chief)
                .WithMany(chief => chief.Subordinates)
                .HasForeignKey(employee => employee.ChiefId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}