namespace MyMusic.Infrastructure.Persistence.Configurations;

public sealed class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("country");

        builder.HasKey(country => country.Id);

        builder.Property(country => country.Id)
            .HasColumnName("id");

        builder.Property(country => country.Name)
            .HasColumnName("country_name")
            .HasMaxLength(Country.MaxNameLength)
            .IsRequired();

        builder.Property(country => country.Code)
            .HasColumnName("country_code")
            .HasMaxLength(Country.MaxCodeLength)
            .IsRequired();

        builder.HasIndex(country => country.Code)
            .IsUnique();
    }
}
