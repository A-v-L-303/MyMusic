namespace MyMusic.Infrastructure.Tests.Persistence.Repositories;

public class RepositoryTests
{
    [Fact]
    public async Task GetByIdAsync_DelegiertAnDbSetFindAsync()
    {
        var entity = new TestEntity { Id = Guid.NewGuid() };

        var dbSet = Substitute.For<DbSet<TestEntity>>();

        dbSet.FindAsync(Arg.Any<object[]>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromResult<TestEntity?>(entity));

        var repository = new Repository<TestEntity, Guid>(CreateContext(dbSet));

        var result = await repository.GetByIdAsync(entity.Id, CancellationToken.None);

        Assert.Same(entity, result);
    }

    [Fact]
    public async Task GetByIdAsync_GibtNullZurueckWennKeineEntitaetGefundenWird()
    {
        var dbSet = Substitute.For<DbSet<TestEntity>>();

        dbSet.FindAsync(Arg.Any<object[]>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromResult<TestEntity?>(null));

        var repository = new Repository<TestEntity, Guid>(CreateContext(dbSet));

        var result = await repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_DelegiertAnDbSetAddAsync()
    {
        var entity = new TestEntity { Id = Guid.NewGuid() };

        var dbSet = Substitute.For<DbSet<TestEntity>>();

        var repository = new Repository<TestEntity, Guid>(CreateContext(dbSet));

        await repository.AddAsync(entity, CancellationToken.None);

        await dbSet.Received(1).AddAsync(entity, Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Update_DelegiertAnDbSetUpdate()
    {
        var entity = new TestEntity { Id = Guid.NewGuid() };

        var dbSet = Substitute.For<DbSet<TestEntity>>();

        var repository = new Repository<TestEntity, Guid>(CreateContext(dbSet));

        repository.Update(entity);

        dbSet.Received(1).Update(entity);
    }

    [Fact]
    public void Remove_DelegiertAnDbSetRemove()
    {
        var entity = new TestEntity { Id = Guid.NewGuid() };

        var dbSet = Substitute.For<DbSet<TestEntity>>();

        var repository = new Repository<TestEntity, Guid>(CreateContext(dbSet));

        repository.Remove(entity);

        dbSet.Received(1).Remove(entity);
    }

    [Fact]
    public async Task SaveChangesAsync_DelegiertAnContextSaveChangesAsync()
    {
        var dbSet = Substitute.For<DbSet<TestEntity>>();

        var context = CreateContext(dbSet);

        context.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(3));

        var repository = new Repository<TestEntity, Guid>(context);

        var result = await repository.SaveChangesAsync(CancellationToken.None);

        Assert.Equal(3, result);
    }

    private static MyMusicDbContext CreateContext(DbSet<TestEntity> dbSet)
    {
        var options = new DbContextOptionsBuilder<MyMusicDbContext>().Options;

        var context = Substitute.For<MyMusicDbContext>(options);

        context.Set<TestEntity>().Returns(dbSet);

        return context;
    }
}
