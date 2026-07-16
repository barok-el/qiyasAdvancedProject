using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Domain.Entities;

public class AssessmentConfiguration : IEntityTypeConfiguration<Assessment>
{
    public void Configure(EntityTypeBuilder<Assessment> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Title)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(a => a.IsCompleted)
               .IsRequired();

        builder.HasOne(a => a.Student)
               .WithMany(s => s.Assessments)
               .HasForeignKey(a => a.StudentId);
    }
}
