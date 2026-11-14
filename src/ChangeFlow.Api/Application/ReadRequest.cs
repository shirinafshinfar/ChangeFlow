using ChangeFlow.Core;
using ChangeFlow.Api.Persistence;
namespace ChangeFlow.Api.Application;
public sealed record ChangeRequestView(string Id,string Title,string? Description,string? Reason,string RequesterId,string Status,long Version)
{
    public static ChangeRequestView From(ChangeRequest r)=>new(r.Id,r.Title,r.Description,r.Reason,r.RequesterId,r.Status.ToString(),r.Version);
}
public sealed class ReadRequest(ChangeRequestQueries queries)
{
    public ChangeRequestView? Execute(ActorContext actor,string id)=>queries.Find(actor,id) is { } r?ChangeRequestView.From(r):null;
}
