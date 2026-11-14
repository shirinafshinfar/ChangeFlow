using System.Net;
using ChangeFlow.Tests.Fixtures;
using Xunit;
namespace ChangeFlow.Tests.Api;
public sealed class ReviewVisibilityTests
{
    [Fact]
    public async Task ReviewerSeesOnlyCurrentPendingVersionAndNoPrivateMetadata()
    {
        using var f=new SqliteFixture();var id=f.Seed("PendingReview",version:7);
        var response=await f.Send("GET","/api/change-requests/"+id,user:"bob");
        Assert.Equal(HttpStatusCode.OK,response.StatusCode);Assert.Equal("\"7\"",response.Headers.ETag!.Tag);
        var view=await SqliteFixture.Json(response);Assert.Equal(7,view.GetProperty("version").GetInt64());
        Assert.Equal(7,view.EnumerateObject().Count());Assert.False(view.TryGetProperty("reviewBaselineTitle",out _));
        var approved=f.Seed("Approved");
        Assert.Equal(HttpStatusCode.NotFound,(await f.Send("GET","/api/change-requests/"+approved,user:"bob")).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await f.Send("GET","/api/change-requests/"+approved)).StatusCode);
    }
}
