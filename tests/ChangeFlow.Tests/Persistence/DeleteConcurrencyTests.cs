using System.Net;
using System.Net.Http.Headers;
using ChangeFlow.Tests.Fixtures;
using Xunit;
namespace ChangeFlow.Tests.Persistence;
public sealed class DeleteConcurrencyTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DeleteAndSubmitNeverBothSucceed(bool deleteWins)
    {
        using var f=new SqliteFixture();var id=f.Seed();var url=$"/api/change-requests/{id}";
        Task<HttpResponseMessage> Delete()=>f.Send("DELETE",url,key:"delete",version:1);
        Task<HttpResponseMessage> Submit()=>f.Send("POST",url+"/submit",key:"submit",version:1);
        var(a,b)=await MutationFaults.Race(f,deleteWins?Delete:Submit,deleteWins?Submit:Delete,TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK,a.StatusCode);Assert.NotEqual(HttpStatusCode.OK,b.StatusCode);
        Assert.Equal(deleteWins?0L:1L,f.Scalar("SELECT COUNT(*) FROM ChangeRequest"));
        if(!deleteWins){Assert.Equal(2L,f.Scalar("SELECT Version FROM ChangeRequest"));Assert.Equal("PendingReview",f.Scalar("SELECT Status FROM ChangeRequest"));}
        Assert.Equal(2L,f.Scalar("SELECT COUNT(*) FROM OperationResult"));
    }
    [Fact]
    public async Task SameKeyDeleteAndReplayAfterRestartDoNotResurrectEntity()
    {
        using var f=new SqliteFixture();var id=f.Seed();var url=$"/api/change-requests/{id}";
        Task<HttpResponseMessage> Delete()=>f.Send("DELETE",url,key:"same-delete",version:1);
        var(a,b)=await MutationFaults.Race(f,Delete,Delete,TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK,a.StatusCode);Assert.Equal(a.StatusCode,b.StatusCode);
        var body=await a.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Equal(body,await b.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        f.Client.Dispose();await f.Host.DisposeAsync();
        using var restart=new ChangeFlowFactory(f.Database.Path);using var client=restart.CreateClient(new(){BaseAddress=new Uri("https://localhost")});
        using var request=new HttpRequestMessage(HttpMethod.Delete,url);
        request.Headers.Authorization=new AuthenticationHeaderValue("Bearer",f.Tokens["alice"]);request.Headers.Add("Idempotency-Key","same-delete");request.Headers.TryAddWithoutValidation("If-Match","\"1\"");
        var replay=await client.SendAsync(request,TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK,replay.StatusCode);Assert.Equal(body,await replay.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.Equal(0L,f.Scalar("SELECT COUNT(*) FROM ChangeRequest"));Assert.Equal(1L,f.Scalar("SELECT COUNT(*) FROM OperationResult"));
    }
}
