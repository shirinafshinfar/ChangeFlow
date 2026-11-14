using System.Net;
using ChangeFlow.Tests.Fixtures;
using Xunit;
namespace ChangeFlow.Tests.Api;
public sealed class ResubmitContractTests
{
    [Fact]
    public async Task FullCycleUsesLatestBaselineAndDurableRejectionRequiresNewKey()
    {
        using var f=new SqliteFixture();var id=f.Seed("ChangesRequested");var url=$"/api/change-requests/{id}";
        var rejected=await f.Send("POST",url+"/submit",key:"unchanged",version:1);
        Assert.Equal(HttpStatusCode.Conflict,rejected.StatusCode);Assert.Equal("CHANGES_REQUIRED",(await SqliteFixture.Json(rejected)).GetProperty("code").GetString());
        Assert.Equal(1L,f.Scalar("SELECT Version FROM ChangeRequest"));
        Assert.Equal(HttpStatusCode.OK,(await f.Send("PUT",url,"{\"title\":\"original\"}",key:"clear",version:1)).StatusCode);
        Assert.Equal("ChangesRequested",f.Scalar("SELECT Status FROM ChangeRequest"));Assert.Equal(DBNull.Value,f.Scalar("SELECT Description FROM ChangeRequest"));
        Assert.Equal(HttpStatusCode.Conflict,(await f.Send("POST",url+"/submit",key:"unchanged",version:1)).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await f.Send("POST",url+"/submit",key:"corrected",version:2)).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await f.Send("POST",url+"/request-changes",user:"bob",key:"return",version:3)).StatusCode);
        Assert.Equal(4L,f.Scalar("SELECT Version FROM ChangeRequest"));
        Assert.Equal("original",f.Scalar("SELECT ReviewBaselineTitle FROM ChangeRequest"));
        Assert.Equal(DBNull.Value,f.Scalar("SELECT ReviewBaselineDescription FROM ChangeRequest"));
        Assert.Equal(HttpStatusCode.OK,(await f.Send("PUT",url,"{\"title\":\"original\",\"description\":\"\"}",key:"same",version:4)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict,(await f.Send("POST",url+"/submit",key:"again",version:5)).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await f.Send("PUT",url,"{\"title\":\"different\"}",key:"editagain",version:5)).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await f.Send("POST",url+"/submit",key:"submitagain",version:6)).StatusCode);
        Assert.Equal(7L,f.Scalar("SELECT Version FROM ChangeRequest"));Assert.Equal("PendingReview",f.Scalar("SELECT Status FROM ChangeRequest"));
    }
}
