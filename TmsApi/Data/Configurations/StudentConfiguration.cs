using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Entities;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.RegistrationNumber)
               .IsRequired()
               .HasMaxLength(20);

        builder.Property(s => s.Name)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(s => s.GPA)
               .HasColumnType("decimal(3,2)");

        builder.HasMany(s => s.Enrollments)
               .WithOne(e => e.Student)
               .HasForeignKey(e => e.StudentId);

        builder.HasMany(s => s.Assessments)
               .WithOne(a => a.Student)
               .HasForeignKey(a => a.StudentId);
    }
}
