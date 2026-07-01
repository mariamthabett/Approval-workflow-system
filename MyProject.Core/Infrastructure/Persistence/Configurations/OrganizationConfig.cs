using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyProject.Core.Domain.Organization;

namespace MyProject.Core.Infrastructure.Persistence.Configurations;

public sealed class RoleConfig : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> b)
    {
        b.ToTable("Roles");
        b.HasKey(r => r.Id);
        b.Property(r => r.Code).HasMaxLength(64).IsRequired();
        b.Property(r => r.Name).HasMaxLength(128).IsRequired();
        b.HasIndex(r => r.Code).IsUnique();
    }
}

public sealed class DepartmentConfig : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> b)
    {
        b.ToTable("Departments");
        b.HasKey(d => d.Id);
        b.Property(d => d.Name).HasMaxLength(128).IsRequired();

        // Optional self/manager references — Restrict to avoid SQL Server multiple-cascade-path errors.
        b.HasOne<Employee>().WithMany().HasForeignKey(d => d.ManagerEmployeeId)
            .OnDelete(DeleteBehavior.Restrict).IsRequired(false);
        b.HasOne<Department>().WithMany().HasForeignKey(d => d.ParentDepartmentId)
            .OnDelete(DeleteBehavior.Restrict).IsRequired(false);
    }
}

public sealed class EmployeeConfig : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> b)
    {
        b.ToTable("Employees");
        b.HasKey(e => e.Id);
        b.Property(e => e.FullName).HasMaxLength(200).IsRequired();
        b.Property(e => e.Email).HasMaxLength(256).IsRequired();
        b.HasIndex(e => e.Email).IsUnique();

        b.HasOne(e => e.Department).WithMany().HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasMany(e => e.Roles).WithOne().HasForeignKey(er => er.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Metadata.FindNavigation(nameof(Employee.Roles))!.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class EmployeeRoleConfig : IEntityTypeConfiguration<EmployeeRole>
{
    public void Configure(EntityTypeBuilder<EmployeeRole> b)
    {
        b.ToTable("EmployeeRoles");
        b.HasKey(er => new { er.EmployeeId, er.RoleId });
        b.HasOne(er => er.Role).WithMany().HasForeignKey(er => er.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
