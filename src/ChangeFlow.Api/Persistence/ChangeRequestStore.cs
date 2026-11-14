using ChangeFlow.Core;
using Microsoft.Data.Sqlite;
namespace ChangeFlow.Api.Persistence;
public static class ChangeRequestStore
{
    public const string Columns="Id,Title,Description,Reason,RequesterId,Status,Version,ReviewBaselineTitle,ReviewBaselineDescription,ReviewBaselineReason";
    public static ChangeRequest Read(SqliteDataReader r)=>new(r.GetString(0),r.GetString(1),r.IsDBNull(2)?null:r.GetString(2),r.IsDBNull(3)?null:r.GetString(3),r.GetString(4),Enum.Parse<RequestStatus>(r.GetString(5)),r.GetInt64(6),r.IsDBNull(7)?null:r.GetString(7),r.IsDBNull(8)?null:r.GetString(8),r.IsDBNull(9)?null:r.GetString(9));
    public static ChangeRequest? Find(SqliteConnection c,SqliteTransaction tx,string id)
    {
        using var cmd=c.CreateCommand();cmd.Transaction=tx;cmd.CommandText="SELECT "+Columns+" FROM ChangeRequest WHERE Id=$id";
        cmd.Parameters.AddWithValue("$id",id);using var reader=cmd.ExecuteReader();return reader.Read()?Read(reader):null;
    }
    private static void Bind(SqliteCommand cmd,ChangeRequest r)
    {
        cmd.Parameters.AddWithValue("$id",r.Id);cmd.Parameters.AddWithValue("$title",r.Title);
        cmd.Parameters.AddWithValue("$description",(object?)r.Description??DBNull.Value);
        cmd.Parameters.AddWithValue("$reason",(object?)r.Reason??DBNull.Value);
        cmd.Parameters.AddWithValue("$owner",r.RequesterId);cmd.Parameters.AddWithValue("$status",r.Status.ToString());
        cmd.Parameters.AddWithValue("$version",r.Version);cmd.Parameters.AddWithValue("$bt",(object?)r.ReviewBaselineTitle??DBNull.Value);
        cmd.Parameters.AddWithValue("$bd",(object?)r.ReviewBaselineDescription??DBNull.Value);
        cmd.Parameters.AddWithValue("$br",(object?)r.ReviewBaselineReason??DBNull.Value);
    }
    public static void Insert(SqliteConnection c,SqliteTransaction tx,ChangeRequest r)
    {
        using var cmd=c.CreateCommand();cmd.Transaction=tx;cmd.CommandText="INSERT INTO ChangeRequest("+Columns+") VALUES($id,$title,$description,$reason,$owner,$status,$version,$bt,$bd,$br)";Bind(cmd,r);cmd.ExecuteNonQuery();
    }
    public static void Delete(SqliteConnection c,SqliteTransaction tx,string id,long expectedVersion)
    {
        using var cmd=c.CreateCommand();cmd.Transaction=tx;
        cmd.CommandText="DELETE FROM ChangeRequest WHERE Id=$id AND Version=$version AND Status='Draft'";
        cmd.Parameters.AddWithValue("$id",id);cmd.Parameters.AddWithValue("$version",expectedVersion);
        if(cmd.ExecuteNonQuery()!=1)throw new RuleViolation("VERSION_CONFLICT");
    }
    public static void Update(SqliteConnection c,SqliteTransaction tx,ChangeRequest r,long expectedVersion)
    {
        using var cmd=c.CreateCommand();cmd.Transaction=tx;
        cmd.CommandText="UPDATE ChangeRequest SET Title=$title,Description=$description,Reason=$reason,Status=$status,Version=$version,ReviewBaselineTitle=$bt,ReviewBaselineDescription=$bd,ReviewBaselineReason=$br WHERE Id=$id AND Version=$expected";
        Bind(cmd,r);cmd.Parameters.AddWithValue("$expected",expectedVersion);
        if(cmd.ExecuteNonQuery()!=1)throw new RuleViolation("VERSION_CONFLICT");
    }
}
