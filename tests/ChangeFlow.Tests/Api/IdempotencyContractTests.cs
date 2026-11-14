using System.Net;
using ChangeFlow.Tests.Fixtures;
using Xunit;
namespace ChangeFlow.Tests.Api;
public sealed class IdempotencyContractTests
{
    [Fact]
    public async Task FingerprintIncludesCanonicalContentTargetAndVersionWithIndependentScopes()
    {
        using var f=new SqliteFixture();var id=f.Seed();var other=f.Seed();
        var url=$"/api/change-requests/{id}";
        Assert.Equal(HttpStatusCode.OK,(await f.Send("PUT",url,"{\"title\":\"x\"}",key:"k",version:1)).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await f.Send("PUT",url,"{\"reason\":\"\",\"title\":\"x\",\"description\":null}",key:"k",version:1)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict,(await f.Send("PUT",url,"{\"title\":\"y\"}",key:"k",version:1)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict,(await f.Send("PUT",url,"{\"title\":\"x\"}",key:"k",version:2)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict,(await f.Send("PUT",$"/api/change-requests/{other}","{\"title\":\"x\"}",key:"k",version:1)).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await f.Send("POST",url+"/submit",key:"k",version:2)).StatusCode);
        Assert.Equal(HttpStatusCode.Created,(await f.Send("POST","/api/change-requests","{\"title\":\"dave\"}",user:"dave",key:"k")).StatusCode);
        Assert.Equal(3L,f.Scalar("SELECT COUNT(*) FROM OperationResult"));
    }
    [Theory]
    [InlineData("validation",400)]
    [InlineData("missing",404)]
    [InlineData("stale",412)]
    [InlineData("state",409)]
    [InlineData("self",403)]
    public async Task TerminalErrorsAreDurableBeforeLiveResourceChecks(string kind,int expected)
    {
        using var f=new SqliteFixture();var id=kind=="missing"?Guid.NewGuid().ToString():f.Seed(kind=="state"?"Approved":"PendingReview",owner:kind=="self"?"carol":"alice");
        Task<HttpResponseMessage> Attempt()=>kind=="validation"
            ?f.Send("PUT",$"/api/change-requests/{id}","{\"title\":\"\"}",key:"terminal",version:1)
            :f.Send("POST",$"/api/change-requests/{id}/"+(kind=="self"?"approve":"submit"),user:kind=="self"?"carol":"alice",key:"terminal",version:kind=="stale"?9:1);
        var first=await Attempt();Assert.Equal(expected,(int)first.StatusCode);
        using(var c=f.Database.Open())using(var cmd=c.CreateCommand()){cmd.CommandText="DELETE FROM ChangeRequest";cmd.ExecuteNonQuery();}
        var replay=await Attempt();Assert.Equal(first.StatusCode,replay.StatusCode);
        Assert.Equal(await first.Content.ReadAsStringAsync(TestContext.Current.CancellationToken),await replay.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1L,f.Scalar("SELECT COUNT(*) FROM OperationResult"));
    }
    [Theory]
    [InlineData(null)]
    [InlineData("contains space")]
    [InlineData("é")]
    public async Task InvalidAdmissionDoesNotReserveKey(string? key)
    {
        using var f=new SqliteFixture();
        Assert.Equal(HttpStatusCode.BadRequest,(await f.Send("POST","/api/change-requests","{\"title\":\"x\"}",key:key)).StatusCode);
        Assert.Equal(0L,f.Scalar("SELECT COUNT(*) FROM OperationResult"));
    }
}
