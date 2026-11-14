using Microsoft.Data.Sqlite;

namespace ChangeFlow.Api.Persistence;
public static class SchemaInitializer
{
    public static string Initialize(SqliteConnectionFactory factory)
    {
        using var c=factory.Open();
        using(var command=c.CreateCommand()) {
            command.CommandText="PRAGMA journal_mode=DELETE;";
            if(!string.Equals(command.ExecuteScalar()?.ToString(),"delete",StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Invalid SQLite journal mode.");
        }
        foreach(var setting in new[]{("synchronous","3"),("foreign_keys","1"),("journal_mode","delete")}) {
            using var verify=c.CreateCommand(); verify.CommandText="PRAGMA "+setting.Item1;
            if(verify.ExecuteScalar()?.ToString()!=setting.Item2) throw new InvalidOperationException("Invalid SQLite configuration.");
        }
        using var tx=c.BeginTransaction(System.Data.IsolationLevel.Serializable,deferred:false);
        using var cmd=c.CreateCommand(); cmd.Transaction=tx;
        using var stream=typeof(SchemaInitializer).Assembly.GetManifestResourceStream("ChangeFlow.Api.Persistence.Schema.sql")
            ?? throw new InvalidOperationException("Schema resource missing.");
        using var reader=new StreamReader(stream); cmd.CommandText=reader.ReadToEnd(); cmd.ExecuteNonQuery(); tx.Commit();
        using var version=c.CreateCommand(); version.CommandText="SELECT sqlite_version()"; return (string)version.ExecuteScalar()!;
    }
}
