namespace MyMusic.Infrastructure.Persistence.Configurations;

public sealed class GenreConfiguration : IEntityTypeConfiguration<Genre>
{
    public void Configure(EntityTypeBuilder<Genre> builder)
    {
        builder.ToTable("genre");

        builder.HasKey(genre => genre.Id);

        builder.Property(genre => genre.Id)
            .HasColumnName("id");

        builder.Property(genre => genre.Name)
            .HasColumnName("genre")
            .HasMaxLength(Genre.MaxNameLength)
            .IsRequired();

        builder.Property(genre => genre.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.HasIndex(genre => new { genre.UserId, genre.Name })
            .IsUnique();
    }
}
