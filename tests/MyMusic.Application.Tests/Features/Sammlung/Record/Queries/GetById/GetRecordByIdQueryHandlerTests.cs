namespace MyMusic.Application.Tests.Features.Sammlung.Record.Queries.GetById;

public class GetRecordByIdQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_EigenerRecordMitArtist_GibtResponseMitLabelUndArtistNamenZurueck()
    {
        // arrange
        var userId = Guid.NewGuid();

        var record = RecordEntity.Create(
            1, 2, RecordFormat.Album, "Abbey Road", 1969, RecordCondition.Nm, null, userId);

        var repository = Substitute.For<IRepository<RecordEntity>>();

        repository.GetByIdAsync(record.Id, Arg.Any<CancellationToken>()).Returns(record);

        var labelRepository = Substitute.For<IRepository<LabelEntity>>();

        labelRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(LabelEntity.Create("Apple Records", 1, null, userId));

        var artistRepository = Substitute.For<IRepository<ArtistEntity>>();

        artistRepository.GetByIdAsync(2, Arg.Any<CancellationToken>())
            .Returns(ArtistEntity.Create("The Beatles", userId));

        var handler = new GetRecordByIdQueryHandler(
            repository, labelRepository, artistRepository, new ExceptionManager(), new RecordResponseBuilder());

        // act
        var response = await handler.HandleAsync(new GetRecordByIdQuery(record.Id, userId), CancellationToken.None);

        // assert
        Assert.Equal("Abbey Road", response.AlbumName);
        Assert.Equal("Apple Records", response.LabelName);
        Assert.Equal("The Beatles", response.ArtistName);
    }

    [Fact]
    public async Task HandleAsync_EigenerRecordOhneArtist_ArtistNameIstNull()
    {
        // arrange
        var userId = Guid.NewGuid();

        var record = RecordEntity.Create(
            1, null, RecordFormat.Compilation, "Various Artists", 1999, RecordCondition.Vg, null, userId);

        var repository = Substitute.For<IRepository<RecordEntity>>();

        repository.GetByIdAsync(record.Id, Arg.Any<CancellationToken>()).Returns(record);

        var labelRepository = Substitute.For<IRepository<LabelEntity>>();

        labelRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(LabelEntity.Create("Various Records", 1, null, userId));

        var artistRepository = Substitute.For<IRepository<ArtistEntity>>();

        var handler = new GetRecordByIdQueryHandler(
            repository, labelRepository, artistRepository, new ExceptionManager(), new RecordResponseBuilder());

        // act
        var response = await handler.HandleAsync(new GetRecordByIdQuery(record.Id, userId), CancellationToken.None);

        // assert
        Assert.Null(response.ArtistName);

        await artistRepository.DidNotReceive().GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_UnbekannterRecord_WirftNotFoundException()
    {
        // arrange
        var repository = Substitute.For<IRepository<RecordEntity>>();

        repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((RecordEntity?)null);

        var labelRepository = Substitute.For<IRepository<LabelEntity>>();

        var artistRepository = Substitute.For<IRepository<ArtistEntity>>();

        var handler = new GetRecordByIdQueryHandler(
            repository, labelRepository, artistRepository, new ExceptionManager(), new RecordResponseBuilder());

        // act
        var act = () => handler.HandleAsync(new GetRecordByIdQuery(1, Guid.NewGuid()), CancellationToken.None);

        // assert
        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task HandleAsync_FremderRecord_WirftNotFoundExceptionStattForbidden()
    {
        // arrange
        var fremderRecord = RecordEntity.Create(
            1, null, RecordFormat.Album, "Abbey Road", 1969, RecordCondition.Vg, null, Guid.NewGuid());

        var repository = Substitute.For<IRepository<RecordEntity>>();

        repository.GetByIdAsync(fremderRecord.Id, Arg.Any<CancellationToken>()).Returns(fremderRecord);

        var labelRepository = Substitute.For<IRepository<LabelEntity>>();

        var artistRepository = Substitute.For<IRepository<ArtistEntity>>();

        var handler = new GetRecordByIdQueryHandler(
            repository, labelRepository, artistRepository, new ExceptionManager(), new RecordResponseBuilder());

        var query = new GetRecordByIdQuery(fremderRecord.Id, Guid.NewGuid());

        // act
        var act = () => handler.HandleAsync(query, CancellationToken.None);

        // assert: 404 statt 403 - Existenz einer fremden Ressource wird nicht bestätigt
        await Assert.ThrowsAsync<NotFoundException>(act);
    }
}
