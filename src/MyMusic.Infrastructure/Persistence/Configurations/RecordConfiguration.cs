namespace MyMusic.Infrastructure.Persistence.Configurations;

public sealed class RecordConfiguration : IEntityTypeConfiguration<Record>
{
    public void Configure(EntityTypeBuilder<Record> builder)
    {
        builder.ToTable("record");

        builder.HasKey(record => record.Id);

        builder.Property(record => record.Id)
            .HasColumnName("id");

        builder.Property(record => record.LabelId)
            .HasColumnName("label_id")
            .IsRequired();

        builder.Property(record => record.ArtistId)
            .HasColumnName("artist_id");

        builder.Property(record => record.AlbumCover)
            .HasColumnName("album_cover");

        builder.Property(record => record.Format)
            .HasColumnName("format")
            .HasColumnType("record_format")
            .IsRequired();

        builder.Property(record => record.AlbumName)
            .HasColumnName("album_name")
            .HasMaxLength(Record.MaxAlbumNameLength)
            .IsRequired();

        builder.Property(record => record.ReleaseYear)
            .HasColumnName("release_year")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(record => record.Condition)
            .HasColumnName("condition")
            .HasColumnType("record_condition")
            .HasDefaultValueSql("'VG'::record_condition")
            .HasSentinel((RecordCondition)(-1))
            .IsRequired();

        builder.Property(record => record.Information)
            .HasColumnName("information")
            .HasMaxLength(Record.MaxInformationLength);

        builder.Property(record => record.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.HasOne<Label>()
            .WithMany()
            .HasForeignKey(record => record.LabelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Artist>()
            .WithMany()
            .HasForeignKey(record => record.ArtistId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
