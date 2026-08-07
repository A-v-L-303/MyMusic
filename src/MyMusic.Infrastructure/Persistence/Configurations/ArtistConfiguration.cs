namespace MyMusic.Infrastructure.Persistence.Configurations;

public sealed class ArtistConfiguration : IEntityTypeConfiguration<Artist>
{
    public void Configure(EntityTypeBuilder<Artist> builder)
    {
        builder.ToTable("artist");

        builder.HasKey(artist => artist.Id);

        builder.Property(artist => artist.Id)
            .HasColumnName("id");

        builder.Property(artist => artist.Name)
            .HasColumnName("artist_name")
            .HasMaxLength(Artist.MaxNameLength)
            .IsRequired();

        builder.Property(artist => artist.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.HasIndex(artist => new { artist.UserId, artist.Name })
            .IsUnique();
    }
}
