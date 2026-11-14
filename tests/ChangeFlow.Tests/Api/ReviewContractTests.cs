using System.Net;
using ChangeFlow.Tests.Fixtures;
using Xunit;
namespace ChangeFlow.Tests.Api;
public sealed class ReviewContractTests
{
    [Theory]
    [InlineData("approve","Approved")]
    [InlineData("request-changes","ChangesRequested")]
    public async Task IndependentReviewerDecidesOnCurrentVersion(string command,string status)
    {
        using var f=new SqliteFixture();var id=f.Seed("PendingReview");
        var response=await f.Send("POST",$"/api/change-requests/{id}/{command}",user:"bob",key:"decision",version:1);
        Assert.Equal(HttpStatusCode.OK,response.StatusCode);
        var receipt=await SqliteFixture.Json(response);Assert.Equal(2,receipt.EnumerateObject().Count());
        Assert.Equal(status,f.Scalar("SELECT Status FROM ChangeRequest"));Assert.Equal(2L,f.Scalar("SELECT Version FROM ChangeRequest"));
        if(command=="request-changes")Assert.Equal("original",f.Scalar("SELECT ReviewBaselineTitle FROM ChangeRequest"));
        var replay=await f.Send("POST",$"/api/change-requests/{id}/{command}",user:"bob",key:"decision",version:1);
        Assert.Equal(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken),await replay.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.DoesNotContain("title",await replay.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }
    [Theory]
    [InlineData("approve")]
    [InlineData("request-changes")]
    public async Task BothSelfReviewCommandsAreForbiddenForDualRole(string command)
    {
        using var f=new SqliteFixture();var id=f.Seed("PendingReview","carol");
        Assert.Equal(HttpStatusCode.Forbidden,(await f.Send("POST",$"/api/change-requests/{id}/{command}",user:"carol",key:"self",version:1)).StatusCode);
        Assert.Equal("PendingReview",f.Scalar("SELECT Status FROM ChangeRequest"));Assert.Equal(1L,f.Scalar("SELECT Version FROM ChangeRequest"));
    }
    [Theory]
    [InlineData("Draft")]
    [InlineData("ChangesRequested")]
    [InlineData("Approved")]
    public async Task ReviewerCannotReviewInvisibleOtherState(string state)
    {
        using var f=new SqliteFixture();var id=f.Seed(state);
        Assert.Equal(HttpStatusCode.NotFound,(await f.Send("POST",$"/api/change-requests/{id}/approve",user:"bob",key:"state",version:1)).StatusCode);
        Assert.Equal(state,f.Scalar("SELECT Status FROM ChangeRequest"));
    }
}
