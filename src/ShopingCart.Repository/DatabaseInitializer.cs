using Dapper;
using Microsoft.Data.Sqlite;

namespace ShopingCart.Repository;

public static class DatabaseInitializer
{
    public static void Initialize(string connectionString)
    {
        var settings = new SqliteConnectionStringBuilder(connectionString);
        var directory = Path.GetDirectoryName(Path.GetFullPath(settings.DataSource));
        Directory.CreateDirectory(directory!);

        using var connection = new SqliteConnection(connectionString);
        connection.Open();
        connection.Execute("PRAGMA journal_mode = WAL;");
        using var transaction = connection.BeginTransaction();
        connection.Execute(ReadSql("schema.sql"), transaction: transaction);
        connection.Execute(ReadSql("seed.sql"), transaction: transaction);
        transaction.Commit();
    }

    private static string ReadSql(string filename)
    {
        var assembly = typeof(DatabaseInitializer).Assembly;
        var resource = assembly.GetManifestResourceNames().Single(name => name.EndsWith(filename));
        using var reader = new StreamReader(assembly.GetManifestResourceStream(resource)!);
        return reader.ReadToEnd();
    }
}
