using System.Net;
using ChangeFlow.Tests.Fixtures;
using Xunit;
namespace ChangeFlow.Tests.Persistence;
public sealed class ConcurrentMutationTests
{
    [Theory]
    [InlineData("edit","approve")]
    [InlineData("approve","edit")]
    [InlineData("approve","request-changes")]
    [InlineData("request-changes","approve")]
    public async Task IncompatibleOperationsHaveExactlyOneEffect(string winner,string loser)
    {
        using var f=new SqliteFixture();var id=f.Seed("PendingReview");
        Task<HttpResponseMessage> Attempt(string operation,string key)=>operation=="edit"
            ?f.Send("PUT",$"/api/change-requests/{id}","{\"title\":\"changed\"}",key:key,version:1)
            :f.Send("POST",$"/api/change-requests/{id}/{operation}",user:"bob",key:key,version:1);
        var (a,b)=await MutationFaults.Race(f,()=>Attempt(winner,"winner"),()=>Attempt(loser,"loser"),TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK,a.StatusCode);Assert.NotEqual(HttpStatusCode.OK,b.StatusCode);
        Assert.Equal(2L,f.Scalar("SELECT Version FROM ChangeRequest"));
        Assert.Equal(winner=="edit"?"PendingReview":winner=="approve"?"Approved":"ChangesRequested",f.Scalar("SELECT Status FROM ChangeRequest"));
        Assert.Equal(winner=="edit"?"changed":"original",f.Scalar("SELECT Title FROM ChangeRequest"));
        if(winner=="request-changes")Assert.Equal("original",f.Scalar("SELECT ReviewBaselineTitle FROM ChangeRequest"));
        Assert.Equal(2L,f.Scalar("SELECT COUNT(*) FROM OperationResult"));
    }
    [Fact]
    public async Task ConcurrentSameKeyReplaysOneEffect()
    {
        using var f=new SqliteFixture();var id=f.Seed();
        Task<HttpResponseMessage> Attempt()=>f.Send("POST",$"/api/change-requests/{id}/submit",key:"same",version:1);
        var(a,b)=await MutationFaults.Race(f,Attempt,Attempt,TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK,a.StatusCode);Assert.Equal(a.StatusCode,b.StatusCode);
        Assert.Equal(await a.Content.ReadAsStringAsync(TestContext.Current.CancellationToken),await b.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.Equal(2L,f.Scalar("SELECT Version FROM ChangeRequest"));Assert.Equal(1L,f.Scalar("SELECT COUNT(*) FROM OperationResult"));
    }
}
