using Consumer.Worker.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Consumer.Worker.Tests.TestSupport;

// Gerçek Sqlite provider'ına karşı test etmek için açık kalan bir ":memory:" bağlantısı kullanılır.
// Bağlantı kapanınca veritabanı da yok olur; her test kendi bağlantısını/context'ini açar ve dispose eder.
public sealed class SqliteInMemoryContextFactory : IDisposable
{
    private readonly SqliteConnection _connection;

    public SqliteInMemoryContextFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    public ConsumerDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ConsumerDbContext>()
            .UseSqlite(_connection)
            .Options;

        return new ConsumerDbContext(options);
    }

    public void Dispose() => _connection.Dispose();
}
