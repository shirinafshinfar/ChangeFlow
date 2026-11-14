using ChangeFlow.Core;
using Microsoft.Data.Sqlite;
namespace ChangeFlow.Api.Persistence;
public static class OperationResultStore
{
    public static (byte[] Fingerprint,OperationResult Result)? Find(SqliteConnection c,SqliteTransaction tx,ActorContext actor,MutationRequest r)
    {
        using var cmd=c.CreateCommand(); cmd.Transaction=tx;
        cmd.CommandText="SELECT Fingerprint,HttpStatus,ResponseJson,ResultCode,TargetId,Location FROM OperationResult WHERE ActorId=$a AND OperationKind=$o AND IdempotencyKey=$k";
        cmd.Parameters.AddWithValue("$a",actor.UserId); cmd.Parameters.AddWithValue("$o",r.Operation); cmd.Parameters.AddWithValue("$k",r.Key);
        using var reader=cmd.ExecuteReader();
        if(!reader.Read())return null;
        return ((byte[])reader[0],new(reader.GetInt32(1),reader.GetString(2),reader.GetString(3),reader.IsDBNull(4)?null:reader.GetString(4),reader.IsDBNull(5)?null:reader.GetString(5)));
    }
    public static void Insert(SqliteConnection c,SqliteTransaction tx,ActorContext actor,MutationRequest r,byte[] fingerprint,OperationResult result)
    {
        using var cmd=c.CreateCommand(); cmd.Transaction=tx;
        cmd.CommandText="INSERT INTO OperationResult VALUES($a,$o,$k,$f,$t,$s,$code,$json,$loc)";
        cmd.Parameters.AddWithValue("$a",actor.UserId); cmd.Parameters.AddWithValue("$o",r.Operation);
        cmd.Parameters.AddWithValue("$k",r.Key); cmd.Parameters.AddWithValue("$f",fingerprint);
        cmd.Parameters.AddWithValue("$t",(object?)result.TargetId??r.Target??(object)DBNull.Value);
        cmd.Parameters.AddWithValue("$s",result.Status); cmd.Parameters.AddWithValue("$code",result.Code);
        cmd.Parameters.AddWithValue("$json",result.Json); cmd.Parameters.AddWithValue("$loc",(object?)result.Location??DBNull.Value);
        cmd.ExecuteNonQuery();
    }
}
