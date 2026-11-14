using System.Net;
using ChangeFlow.Tests.Fixtures;
using Xunit;
namespace ChangeFlow.Tests.Api;
public sealed class SubmitContractTests
{
    [Fact]
    public async Task SubmitDraftPreservesContentAndRejectsOtherStates()
    {
        using var f=new SqliteFixture();var id=f.Seed();
        var result=await f.Send("POST",$"/api/change-requests/{id}/submit",key:"submit",version:1);
        Assert.Equal(HttpStatusCode.OK,result.StatusCode);
        Assert.Equal("PendingReview",f.Scalar("SELECT Status FROM ChangeRequest"));
        Assert.Equal("original",f.Scalar("SELECT Title FROM ChangeRequest"));Assert.Equal(2L,f.Scalar("SELECT Version FROM ChangeRequest"));
        Assert.Equal(HttpStatusCode.PreconditionFailed,(await f.Send("POST",$"/api/change-requests/{id}/submit",key:"stale",version:1)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict,(await f.Send("POST",$"/api/change-requests/{id}/submit",key:"invalid",version:2)).StatusCode);
        var approved=f.Seed("Approved");
        Assert.Equal(HttpStatusCode.Conflict,(await f.Send("POST",$"/api/change-requests/{approved}/submit",key:"approved",version:1)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,(await f.Send("POST",$"/api/change-requests/{id}/submit",user:"bob",key:"role",version:2)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,(await f.Send("POST",$"/api/change-requests/{id}/submit",user:"dave",key:"owner",version:2)).StatusCode);
    }
}
