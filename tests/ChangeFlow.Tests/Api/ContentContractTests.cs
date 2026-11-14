using System.Net;
using ChangeFlow.Tests.Fixtures;
using Xunit;
namespace ChangeFlow.Tests.Api;
public sealed class ContentContractTests
{
    private const string Base="/api/change-requests";
    [Fact]
    public async Task CreateReadReplaceUsesObservedEtagAndClearsOptionalFields()
    {
        using var f=new SqliteFixture();var key=Guid.NewGuid().ToString();
        var created=await f.Send("POST",Base,"{\"title\":\"draft\",\"description\":\"description\",\"reason\":\"reason\"}",key:key);
        Assert.Equal(HttpStatusCode.Created,created.StatusCode);
        var receipt=await SqliteFixture.Json(created);var id=receipt.GetProperty("requestId").GetString()!;
        Assert.Equal(2,receipt.EnumerateObject().Count());Assert.NotNull(created.Headers.Location);
        var view=await f.Send("GET",Base+"/"+id);Assert.Equal(HttpStatusCode.OK,view.StatusCode);
        Assert.Equal("Draft",(await SqliteFixture.Json(view)).GetProperty("status").GetString());
        var version=long.Parse(view.Headers.ETag!.Tag.Trim('"'));
        var updated=await f.Send("PUT",Base+"/"+id,"{\"title\":\"edited\"}",key:"edit",version:version);
        Assert.Equal(HttpStatusCode.OK,updated.StatusCode);
        Assert.Equal(2L,f.Scalar("SELECT Version FROM ChangeRequest"));
        Assert.Equal(DBNull.Value,f.Scalar("SELECT Description FROM ChangeRequest"));
        Assert.Equal(DBNull.Value,f.Scalar("SELECT Reason FROM ChangeRequest"));
        Assert.Equal("edited",f.Scalar("SELECT Title FROM ChangeRequest"));
        Assert.Equal(2L,f.Scalar("SELECT COUNT(*) FROM OperationResult"));
        var replay=await f.Send("POST",Base,"{\"title\":\"draft\",\"description\":\"description\",\"reason\":\"reason\"}",key:key);
        Assert.Equal(HttpStatusCode.Created,replay.StatusCode);Assert.Equal(created.Headers.Location,replay.Headers.Location);
    }
    [Theory]
    [InlineData("{}")]
    [InlineData("{\"title\":null}")]
    [InlineData("{\"title\":\"\"}")]
    [InlineData("{\"title\":\" \\t\\n\"}")]
    public async Task InvalidTitleDoesNotClearExistingFields(string json)
    {
        using var f=new SqliteFixture();var id=f.Seed();
        var result=await f.Send("PUT",Base+"/"+id,json,key:"invalid-title",version:1);
        Assert.Equal(HttpStatusCode.BadRequest,result.StatusCode);
        Assert.Equal("old description",f.Scalar("SELECT Description FROM ChangeRequest"));
        Assert.Equal(1L,f.Scalar("SELECT Version FROM ChangeRequest"));
    }
    [Theory]
    [InlineData("{\"title\":\"a\",\"title\":\"b\"}")]
    [InlineData("{\"title\":\"a\",\"requesterId\":\"forged\"}")]
    [InlineData("{\"title\":\"a\",\"status\":\"Approved\"}")]
    [InlineData("{\"title\":1}")]
    [InlineData("{\"title\":\"\\ud800\"}")]
    public async Task InvalidTransportNeverReservesKey(string json)
    {
        using var f=new SqliteFixture();var result=await f.Send("POST",Base,json,key:"not-reserved");
        Assert.Equal(HttpStatusCode.BadRequest,result.StatusCode);
        Assert.Equal(0L,f.Scalar("SELECT COUNT(*) FROM ChangeRequest"));Assert.Equal(0L,f.Scalar("SELECT COUNT(*) FROM OperationResult"));
    }
    [Fact]
    public async Task NonOwnerIsHiddenAndMissingHeadersAreRejected()
    {
        using var f=new SqliteFixture();var id=f.Seed(owner:"dave");
        Assert.Equal(HttpStatusCode.NotFound,(await f.Send("PUT",Base+"/"+id,"{\"title\":\"bad\"}",key:"hidden",version:1)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,(await f.Send("POST",Base,"{\"title\":\"a\"}")).StatusCode);
        Assert.Equal((HttpStatusCode)428,(await f.Send("PUT",Base+"/"+id,"{\"title\":\"a\"}",key:"no-version")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,(await f.Send("GET",Base+"/"+id,user:"invalid-token")).StatusCode);
        Assert.Equal("original",f.Scalar("SELECT Title FROM ChangeRequest"));
    }
}
