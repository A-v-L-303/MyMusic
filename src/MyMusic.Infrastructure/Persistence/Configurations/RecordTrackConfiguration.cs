namespace MyMusic.Infrastructure.Persistence.Configurations;

public sealed class RecordTrackConfiguration : IEntityTypeConfiguration<RecordTrack>
{
    public void Configure(EntityTypeBuilder<RecordTrack> builder)
    {
        builder.ToTable("record_track");

        builder.HasKey(track => track.Id);

        builder.Property(track => track.Id)
            .HasColumnName("id");

        builder.Property(track => track.RecordId)
            .HasColumnName("record_id")
            .IsRequired();

        builder.Property(track => track.ArtistId)
            .HasColumnName("artist_id")
            .IsRequired();

        builder.Property(track => track.GenreId)
            .HasColumnName("genre_id")
            .IsRequired();

        builder.Property(track => track.TrackName)
            .HasColumnName("track_name")
            .HasMaxLength(RecordTrack.MaxTrackNameLength)
            .IsRequired();

        builder.Property(track => track.RecordSide)
            .HasColumnName("record_side")
            .HasMaxLength(RecordTrack.MaxRecordSideLength)
            .HasDefaultValue("0")
            .IsRequired();

        builder.Property(track => track.TrackNumber)
            .HasColumnName("track_number")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(track => track.Information)
            .HasColumnName("information")
            .HasMaxLength(RecordTrack.MaxInformationLength);

        builder.Property(track => track.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.HasIndex(track => new { track.RecordId, track.RecordSide, track.TrackNumber })
            .IsUnique();

        builder.HasOne<Record>()
            .WithMany()
            .HasForeignKey(track => track.RecordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Artist>()
            .WithMany()
            .HasForeignKey(track => track.ArtistId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Genre>()
            .WithMany()
            .HasForeignKey(track => track.GenreId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
