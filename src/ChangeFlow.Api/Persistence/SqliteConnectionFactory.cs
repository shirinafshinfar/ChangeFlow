using Microsoft.Data.Sqlite;

namespace ChangeFlow.Api.Persistence;

public sealed class SqliteConnectionFactory
{
    public string Path { get; }
    public SqliteConnectionFactory(IConfiguration configuration)
    {
        Path = System.IO.Path.GetFullPath(configuration["ChangeFlow:DatabasePath"] ?? ".local/changeflow.db");
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path)!);
    }
    public SqliteConnection Open()
    {
        var c = new SqliteConnection(new SqliteConnectionStringBuilder {
            DataSource=Path, Pooling=true, DefaultTimeout=5, ForeignKeys=true
        }.ToString());
        try {
            c.Open();
            using var cmd=c.CreateCommand();
            cmd.CommandText="PRAGMA synchronous=EXTRA; PRAGMA foreign_keys=ON;";
            cmd.ExecuteNonQuery();
            return c;
        } catch { c.Dispose(); throw; }
    }
}
