namespace MyMusic.Application.Tests.Features.Verwaltung.Admin.Commands.Delete;

public class DeleteUserCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_EigenerAccount_WirftConflictExceptionOhneZuLoeschen()
    {
        // arrange
        var adminUserId = Guid.NewGuid();

        var recordRepository = Substitute.For<IRepository<RecordEntity>>();

        var labelRepository = Substitute.For<IRepository<LabelEntity>>();

        var artistRepository = Substitute.For<IRepository<ArtistEntity>>();

        var genreRepository = Substitute.For<IRepository<GenreEntity>>();

        var keycloakAdminClient = Substitute.For<IKeycloakAdminClient>();

        var currentUserService = Substitute.For<ICurrentUserService>();

        currentUserService.UserId.Returns(adminUserId);

        var handler = new DeleteUserCommandHandler(
            recordRepository,
            labelRepository,
            artistRepository,
            genreRepository,
            keycloakAdminClient,
            currentUserService,
            new ExceptionManager());

        // act
        var act = () => handler.HandleAsync(new DeleteUserCommand(adminUserId), CancellationToken.None);

        // assert
        await Assert.ThrowsAsync<ConflictException>(act);

        await keycloakAdminClient.DidNotReceive().DeleteUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_FremderAccount_LoeschtAppDatenVorKeycloakAccount()
    {
        // arrange
        var targetUserId = Guid.NewGuid();

        var record = RecordEntity.Create(
            1, null, RecordFormat.Album, "Album", 2000, RecordCondition.Nm, null, targetUserId);

        var label = LabelEntity.Create("Label", 1, null, targetUserId);

        var artist = ArtistEntity.Create("Artist", targetUserId);

        var genre = GenreEntity.Create("Rock", targetUserId);

        var recordRepository = Substitute.For<IRepository<RecordEntity>>();

        recordRepository.GetPagedAsync(
                Arg.Any<Expression<Func<RecordEntity, bool>>>(),
                Arg.Any<Func<IQueryable<RecordEntity>, IOrderedQueryable<RecordEntity>>>(),
                1,
                int.MaxValue,
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<RecordEntity>)new List<RecordEntity> { record }, TotalCount: 1));

        var labelRepository = Substitute.For<IRepository<LabelEntity>>();

        labelRepository.GetPagedAsync(
                Arg.Any<Expression<Func<LabelEntity, bool>>>(),
                Arg.Any<Func<IQueryable<LabelEntity>, IOrderedQueryable<LabelEntity>>>(),
                1,
                int.MaxValue,
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<LabelEntity>)new List<LabelEntity> { label }, TotalCount: 1));

        var artistRepository = Substitute.For<IRepository<ArtistEntity>>();

        artistRepository.GetPagedAsync(
                Arg.Any<Expression<Func<ArtistEntity, bool>>>(),
                Arg.Any<Func<IQueryable<ArtistEntity>, IOrderedQueryable<ArtistEntity>>>(),
                1,
                int.MaxValue,
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<ArtistEntity>)new List<ArtistEntity> { artist }, TotalCount: 1));

        var genreRepository = Substitute.For<IRepository<GenreEntity>>();

        genreRepository.GetPagedAsync(
                Arg.Any<Expression<Func<GenreEntity, bool>>>(),
                Arg.Any<Func<IQueryable<GenreEntity>, IOrderedQueryable<GenreEntity>>>(),
                1,
                int.MaxValue,
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<GenreEntity>)new List<GenreEntity> { genre }, TotalCount: 1));

        var keycloakAdminClient = Substitute.For<IKeycloakAdminClient>();

        var currentUserService = Substitute.For<ICurrentUserService>();

        currentUserService.UserId.Returns(Guid.NewGuid());

        var handler = new DeleteUserCommandHandler(
            recordRepository,
            labelRepository,
            artistRepository,
            genreRepository,
            keycloakAdminClient,
            currentUserService,
            new ExceptionManager());

        // act
        var result = await handler.HandleAsync(new DeleteUserCommand(targetUserId), CancellationToken.None);

        // assert
        Assert.True(result);

        Received.InOrder(() =>
        {
            recordRepository.Remove(record);

            recordRepository.SaveChangesAsync(Arg.Any<CancellationToken>());

            labelRepository.Remove(label);

            labelRepository.SaveChangesAsync(Arg.Any<CancellationToken>());

            artistRepository.Remove(artist);

            artistRepository.SaveChangesAsync(Arg.Any<CancellationToken>());

            genreRepository.Remove(genre);

            genreRepository.SaveChangesAsync(Arg.Any<CancellationToken>());

            keycloakAdminClient.DeleteUserAsync(targetUserId, Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task HandleAsync_KeycloakLoeschungSchlaegtFehl_AppDatenBleibenGeloescht()
    {
        // arrange
        var targetUserId = Guid.NewGuid();

        var recordRepository = Substitute.For<IRepository<RecordEntity>>();

        recordRepository.GetPagedAsync(
                Arg.Any<Expression<Func<RecordEntity, bool>>>(),
                Arg.Any<Func<IQueryable<RecordEntity>, IOrderedQueryable<RecordEntity>>>(),
                1,
                int.MaxValue,
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<RecordEntity>)new List<RecordEntity>(), TotalCount: 0));

        var labelRepository = Substitute.For<IRepository<LabelEntity>>();

        labelRepository.GetPagedAsync(
                Arg.Any<Expression<Func<LabelEntity, bool>>>(),
                Arg.Any<Func<IQueryable<LabelEntity>, IOrderedQueryable<LabelEntity>>>(),
                1,
                int.MaxValue,
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<LabelEntity>)new List<LabelEntity>(), TotalCount: 0));

        var artistRepository = Substitute.For<IRepository<ArtistEntity>>();

        artistRepository.GetPagedAsync(
                Arg.Any<Expression<Func<ArtistEntity, bool>>>(),
                Arg.Any<Func<IQueryable<ArtistEntity>, IOrderedQueryable<ArtistEntity>>>(),
                1,
                int.MaxValue,
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<ArtistEntity>)new List<ArtistEntity>(), TotalCount: 0));

        var genreRepository = Substitute.For<IRepository<GenreEntity>>();

        genreRepository.GetPagedAsync(
                Arg.Any<Expression<Func<GenreEntity, bool>>>(),
                Arg.Any<Func<IQueryable<GenreEntity>, IOrderedQueryable<GenreEntity>>>(),
                1,
                int.MaxValue,
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<GenreEntity>)new List<GenreEntity>(), TotalCount: 0));

        var keycloakAdminClient = Substitute.For<IKeycloakAdminClient>();

        keycloakAdminClient.DeleteUserAsync(targetUserId, Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new HttpRequestException("Keycloak nicht erreichbar.")));

        var currentUserService = Substitute.For<ICurrentUserService>();

        currentUserService.UserId.Returns(Guid.NewGuid());

        var handler = new DeleteUserCommandHandler(
            recordRepository,
            labelRepository,
            artistRepository,
            genreRepository,
            keycloakAdminClient,
            currentUserService,
            new ExceptionManager());

        // act
        var act = () => handler.HandleAsync(new DeleteUserCommand(targetUserId), CancellationToken.None);

        // assert: die Exception läuft unbehandelt weiter (500 über GlobalExceptionHandler), die App-Daten
        // wurden aber bereits vorher entfernt
        await Assert.ThrowsAsync<HttpRequestException>(act);

        await recordRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

        await genreRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
