using System.Net;
using ChangeFlow.Tests.Fixtures;
using Xunit;
namespace ChangeFlow.Tests.Api;
public sealed class DeleteContractTests
{
    [Fact]
    public async Task DeleteUsesVersionAndKeepsReceiptWithoutResource()
    {
        using var f=new SqliteFixture();var id=f.Seed();var url=$"/api/change-requests/{id}";
        Assert.Equal(HttpStatusCode.BadRequest,(await f.Send("DELETE",url,version:1)).StatusCode);
        Assert.Equal((HttpStatusCode)428,(await f.Send("DELETE",url,key:"missing-version")).StatusCode);
        Assert.Equal(HttpStatusCode.PreconditionFailed,(await f.Send("DELETE",url,key:"stale",version:2)).StatusCode);
        var result=await f.Send("DELETE",url,key:"delete",version:1);
        Assert.Equal(HttpStatusCode.OK,result.StatusCode);Assert.Equal("Deleted",(await SqliteFixture.Json(result)).GetProperty("result").GetString());
        Assert.Equal(0L,f.Scalar("SELECT COUNT(*) FROM ChangeRequest"));Assert.Equal(2L,f.Scalar("SELECT COUNT(*) FROM OperationResult"));
        var replay=await f.Send("DELETE",url,key:"delete",version:1);
        Assert.Equal(await result.Content.ReadAsStringAsync(TestContext.Current.CancellationToken),await replay.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.Equal(HttpStatusCode.NotFound,(await f.Send("GET",url)).StatusCode);
    }
    [Theory]
    [InlineData("Draft","dave",404)]
    [InlineData("PendingReview","alice",409)]
    [InlineData("ChangesRequested","alice",409)]
    [InlineData("Approved","alice",409)]
    [InlineData("PendingReview","carol",403)]
    [InlineData("Draft","bob",403)]
    public async Task InvalidDeletionDoesNotMutate(string status,string actor,int expected)
    {
        using var f=new SqliteFixture();var id=f.Seed(status);
        Assert.Equal(expected,(int)(await f.Send("DELETE",$"/api/change-requests/{id}",user:actor,key:"denied",version:1)).StatusCode);
        Assert.Equal(1L,f.Scalar("SELECT COUNT(*) FROM ChangeRequest"));Assert.Equal(1L,f.Scalar("SELECT Version FROM ChangeRequest"));
    }
}
