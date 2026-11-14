using System.Net;
using ChangeFlow.Tests.Fixtures;
using Xunit;
namespace ChangeFlow.Tests.Api;
public sealed class PendingReviewEditTests
{
    [Fact]
    public async Task EditDuringReviewInvalidatesOldApprovalAndApprovedIsImmutable()
    {
        using var f=new SqliteFixture();var id=f.Seed("PendingReview");
        var old=await f.Send("GET","/api/change-requests/"+id,user:"bob");Assert.Equal("\"1\"",old.Headers.ETag!.Tag);
        Assert.Equal(HttpStatusCode.OK,(await f.Send("PUT","/api/change-requests/"+id,"{\"title\":\"new\"}",key:"edit",version:1)).StatusCode);
        Assert.Equal("PendingReview",f.Scalar("SELECT Status FROM ChangeRequest"));
        Assert.Equal(HttpStatusCode.PreconditionFailed,(await f.Send("POST",$"/api/change-requests/{id}/approve",user:"bob",key:"stale",version:1)).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await f.Send("POST",$"/api/change-requests/{id}/approve",user:"bob",key:"current",version:2)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict,(await f.Send("PUT","/api/change-requests/"+id,"{\"title\":\"bad\"}",key:"immutable",version:3)).StatusCode);
        Assert.Equal("new",f.Scalar("SELECT Title FROM ChangeRequest"));Assert.Equal(3L,f.Scalar("SELECT Version FROM ChangeRequest"));
    }
}
