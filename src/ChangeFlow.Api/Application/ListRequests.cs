using System.Text;
using ChangeFlow.Api.Endpoints;
using ChangeFlow.Api.Persistence;
using ChangeFlow.Core;
namespace ChangeFlow.Api.Application;
public sealed record RequestPage(IReadOnlyList<ChangeRequestView> Items,string? NextCursor);
public sealed class ListRequests(ChangeRequestQueries queries)
{
    public RequestPage Execute(ActorContext actor,int limit,string? cursor)
    {
        string? after=null;
        if(cursor is not null) {
            try {
                if(cursor.Length is <1 or >128||cursor.Any(ch=>!char.IsAsciiLetterOrDigit(ch)&&ch!='-'&&ch!='_'))throw new FormatException();
                var padded=cursor.Replace('-','+').Replace('_','/');padded+=new string('=',(4-padded.Length%4)%4);
                after=Encoding.UTF8.GetString(Convert.FromBase64String(padded));
                if(!Guid.TryParseExact(after,"D",out var id)||after!=id.ToString())throw new FormatException();
            } catch(FormatException){throw new InputFailure(400,"INVALID_CURSOR");}
        }
        var rows=queries.List(actor,limit+1,after);
        var items=rows.Take(limit).Select(ChangeRequestView.From).ToArray();
        var next=rows.Count>limit?Convert.ToBase64String(Encoding.UTF8.GetBytes(items[^1].Id)).TrimEnd('=').Replace('+','-').Replace('/','_'):null;
        return new(items,next);
    }
}
