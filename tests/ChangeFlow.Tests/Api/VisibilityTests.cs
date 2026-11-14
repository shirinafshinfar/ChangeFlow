using System.Net;
using ChangeFlow.Core;
using ChangeFlow.Tests.Fixtures;
using Xunit;
namespace ChangeFlow.Tests.Api;
public sealed class VisibilityTests
{
    [Theory]
    [InlineData("alice")]
    [InlineData("bob")]
    [InlineData("carol")]
    [InlineData("dave")]
    public async Task SingleAndListUseSameRoleAndOwnershipPredicate(string actor)
    {
        using var f=new SqliteFixture();var expected=new HashSet<string>();
        foreach(var owner in new[]{"alice","bob","carol","dave"})
        foreach(var status in Enum.GetNames<RequestStatus>()){
            var id=f.Seed(status,owner);var visible=(actor is "alice" or "carol" or "dave"&&actor==owner)||
                (actor is "bob" or "carol"&&owner!=actor&&status=="PendingReview");
            var response=await f.Send("GET",$"/api/change-requests/{id}",user:actor);
            Assert.Equal(visible?HttpStatusCode.OK:HttpStatusCode.NotFound,response.StatusCode);
            if(visible)expected.Add(id);
        }
        var list=await f.Send("GET","/api/change-requests?limit=100",user:actor);
        Assert.Equal(HttpStatusCode.OK,list.StatusCode);
        var json=await SqliteFixture.Json(list);var actual=json.GetProperty("items").EnumerateArray().Select(i=>i.GetProperty("id").GetString()!).ToArray();
        Assert.Equal(expected.Order(StringComparer.Ordinal),actual);
        Assert.Equal(System.Text.Json.JsonValueKind.Null,json.GetProperty("nextCursor").ValueKind);
    }
    [Fact]
    public async Task HiddenAndMissingHaveIdenticalPublicBodyAndHeaders()
    {
        using var f=new SqliteFixture();var hidden=f.Seed();
        var a=await f.Send("GET",$"/api/change-requests/{hidden}",user:"dave");
        var b=await f.Send("GET",$"/api/change-requests/{Guid.NewGuid()}",user:"dave");
        Assert.Equal(HttpStatusCode.NotFound,a.StatusCode);Assert.Equal(a.StatusCode,b.StatusCode);
        Assert.Equal(await a.Content.ReadAsStringAsync(TestContext.Current.CancellationToken),await b.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.Equal(a.Headers.ToString(),b.Headers.ToString());Assert.Equal(a.Content.Headers.ToString(),b.Content.Headers.ToString());
    }
    [Fact]
    public async Task PaginationFiltersBeforeLimitHasNoTotalAndRejectsInvalidCursors()
    {
        using var f=new SqliteFixture();
        for(var i=0;i<7;i++)f.Seed("Draft");
        var visible=Enumerable.Range(0,3).Select(_=>f.Seed("PendingReview")).Order(StringComparer.Ordinal).ToArray();
        var found=new List<string>();string? cursor=null;
        do {
            var response=await f.Send("GET","/api/change-requests?limit=1"+(cursor is null?"":"&cursor="+cursor),user:"bob");
            Assert.Equal(HttpStatusCode.OK,response.StatusCode);var json=await SqliteFixture.Json(response);
            Assert.Equal(2,json.EnumerateObject().Count());var items=json.GetProperty("items").EnumerateArray().ToArray();
            Assert.Single(items);Assert.Equal(7,items[0].EnumerateObject().Count());found.Add(items[0].GetProperty("id").GetString()!);
            cursor=json.GetProperty("nextCursor").GetString();
            if(found.Count>3)throw new InvalidOperationException("cursor did not advance");
        } while(cursor is not null);
        Assert.Equal(visible,found);
        foreach(var query in new[]{"limit=0","limit=101","limit=no","cursor=!","cursor=YWJj","limit=1&limit=2"})
            Assert.Equal(HttpStatusCode.BadRequest,(await f.Send("GET","/api/change-requests?"+query,user:"bob")).StatusCode);
        using(var c=f.Database.Open())using(var cmd=c.CreateCommand()){cmd.CommandText="DELETE FROM ChangeRequest";cmd.ExecuteNonQuery();}
        var empty=await SqliteFixture.Json(await f.Send("GET","/api/change-requests",user:"bob"));
        Assert.Empty(empty.GetProperty("items").EnumerateArray());Assert.Null(empty.GetProperty("nextCursor").GetString());
        f.Revoke("bob",Roles.Reviewer);
        Assert.Equal(HttpStatusCode.Forbidden,(await f.Send("GET","/api/change-requests",user:"bob")).StatusCode);
    }
}
