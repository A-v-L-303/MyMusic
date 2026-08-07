namespace MyMusic.Infrastructure.Persistence.Configurations;

public sealed class LabelConfiguration : IEntityTypeConfiguration<Label>
{
    public void Configure(EntityTypeBuilder<Label> builder)
    {
        builder.ToTable("label");

        builder.HasKey(label => label.Id);

        builder.Property(label => label.Id)
            .HasColumnName("id");

        builder.Property(label => label.Name)
            .HasColumnName("label_name")
            .HasMaxLength(Label.MaxNameLength)
            .IsRequired();

        builder.Property(label => label.CountryId)
            .HasColumnName("country_id")
            .IsRequired();

        builder.Property(label => label.Information)
            .HasColumnName("information")
            .HasMaxLength(Label.MaxInformationLength);

        builder.Property(label => label.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.HasIndex(label => new { label.UserId, label.Name })
            .IsUnique();

        builder.HasOne<Country>()
            .WithMany()
            .HasForeignKey(label => label.CountryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
