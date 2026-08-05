namespace MyMusic.Infrastructure.Tests.Persistence.Repositories;

public class RepositoryTests
{
    [Fact]
    public async Task GetByIdAsync_GibtNullZurueckWennKeineEntitaetGefundenWird()
    {
        // arrange
        var dbSet = Substitute.For<DbSet<TestEntity>>();

        dbSet.FindAsync(Arg.Any<object[]>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromResult<TestEntity?>(null));

        var repository = new Repository<TestEntity>(CreateContext(dbSet));

        // act
        var result = await repository.GetByIdAsync(1, CancellationToken.None);

        // assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_DelegiertAnDbSetAddAsync()
    {
        // arrange
        var entity = new TestEntity { Id = 1 };

        var dbSet = Substitute.For<DbSet<TestEntity>>();

        var repository = new Repository<TestEntity>(CreateContext(dbSet));

        // act
        await repository.AddAsync(entity, CancellationToken.None);

        // assert
        await dbSet.Received(1).AddAsync(entity, Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Update_DelegiertAnDbSetUpdate()
    {
        // arrange
        var entity = new TestEntity { Id = 1 };

        var dbSet = Substitute.For<DbSet<TestEntity>>();

        var repository = new Repository<TestEntity>(CreateContext(dbSet));

        // act
        repository.Update(entity);

        // assert
        dbSet.Received(1).Update(entity);
    }

    [Fact]
    public void Remove_DelegiertAnDbSetRemove()
    {
        // arrange
        var entity = new TestEntity { Id = 1 };

        var dbSet = Substitute.For<DbSet<TestEntity>>();

        var repository = new Repository<TestEntity>(CreateContext(dbSet));

        // act
        repository.Remove(entity);

        // assert
        dbSet.Received(1).Remove(entity);
    }

    [Fact]
    public async Task SaveChangesAsync_DelegiertAnContextSaveChangesAsync()
    {
        // arrange
        var dbSet = Substitute.For<DbSet<TestEntity>>();

        var context = CreateContext(dbSet);

        context.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(3));

        var repository = new Repository<TestEntity>(context);

        // act
        var result = await repository.SaveChangesAsync(CancellationToken.None);

        // assert
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
