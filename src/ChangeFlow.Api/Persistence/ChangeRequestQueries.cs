using ChangeFlow.Core;
namespace ChangeFlow.Api.Persistence;
public sealed class ChangeRequestQueries(SqliteConnectionFactory factory)
{
    public const string Predicate="((RequesterId=$actor AND ($roles & 1)=1) OR (RequesterId<>$actor AND Status='PendingReview' AND ($roles & 2)=2))";
    public IReadOnlyList<ChangeRequest> List(ActorContext actor,int count,string? after)
    {
        using var c=factory.Open();using var cmd=c.CreateCommand();
        cmd.CommandText="SELECT "+ChangeRequestStore.Columns+" FROM ChangeRequest WHERE "+Predicate+
            " AND ($after IS NULL OR Id>$after) ORDER BY Id LIMIT $count";
        cmd.Parameters.AddWithValue("$actor",actor.UserId);cmd.Parameters.AddWithValue("$roles",(int)actor.Roles);
        cmd.Parameters.AddWithValue("$after",(object?)after??DBNull.Value);cmd.Parameters.AddWithValue("$count",count);
        var rows=new List<ChangeRequest>();using var reader=cmd.ExecuteReader();
        while(reader.Read())rows.Add(ChangeRequestStore.Read(reader));
        return rows;
    }
    public ChangeRequest? Find(ActorContext actor,string id)
    {
        using var c=factory.Open();using var cmd=c.CreateCommand();
        cmd.CommandText="SELECT "+ChangeRequestStore.Columns+" FROM ChangeRequest WHERE Id=$id AND "+Predicate;
        cmd.Parameters.AddWithValue("$id",id);cmd.Parameters.AddWithValue("$actor",actor.UserId);cmd.Parameters.AddWithValue("$roles",(int)actor.Roles);
        using var reader=cmd.ExecuteReader();return reader.Read()?ChangeRequestStore.Read(reader):null;
    }
}
