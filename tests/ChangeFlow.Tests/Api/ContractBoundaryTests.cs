using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ChangeFlow.Tests.Fixtures;
using Xunit;
namespace ChangeFlow.Tests.Api;
public sealed class ContractBoundaryTests
{
    [Fact]
    public async Task ScalarLimitsAndOptionalReplacementRoundTripThroughHttp()
    {
        using var f=new SqliteFixture();
        var title=string.Concat(Enumerable.Repeat(char.ConvertFromUtf32(0x1F642),200));
        var description=new string('d',5000);var reason=new string('r',2000);
        var created=await f.Send("POST","/api/change-requests",JsonSerializer.Serialize(new{title,description,reason}),key:"boundary");
        Assert.Equal(HttpStatusCode.Created,created.StatusCode);var id=(await SqliteFixture.Json(created)).GetProperty("requestId").GetString()!;
        Assert.Equal(title,f.Scalar("SELECT Title FROM ChangeRequest"));
        foreach(var field in new[]{"title","description","reason"}){
            var input=new Dictionary<string,string>{{"title",title},{"description",description},{"reason",reason}};
            input[field]+="x";
            Assert.Equal(HttpStatusCode.BadRequest,(await f.Send("PUT",$"/api/change-requests/{id}",JsonSerializer.Serialize(input),key:"over-"+field,version:1)).StatusCode);
            Assert.Equal(1L,f.Scalar("SELECT Version FROM ChangeRequest"));Assert.Equal(description,f.Scalar("SELECT Description FROM ChangeRequest"));
        }
        Assert.Equal(HttpStatusCode.OK,(await f.Send("PUT",$"/api/change-requests/{id}","{\"title\":\" á \",\"description\":\"new\",\"reason\":\"replacement\"}",key:"replace",version:1)).StatusCode);
        Assert.Equal(" á ",f.Scalar("SELECT Title FROM ChangeRequest"));Assert.Equal("new",f.Scalar("SELECT Description FROM ChangeRequest"));Assert.Equal("replacement",f.Scalar("SELECT Reason FROM ChangeRequest"));
        Assert.Equal(HttpStatusCode.OK,(await f.Send("PUT",$"/api/change-requests/{id}","{\"title\":\"next\",\"description\":null,\"reason\":\"\"}",key:"clear",version:2)).StatusCode);
        Assert.Equal(DBNull.Value,f.Scalar("SELECT Description FROM ChangeRequest"));Assert.Equal(DBNull.Value,f.Scalar("SELECT Reason FROM ChangeRequest"));
    }
    [Theory]
    [InlineData("*")]
    [InlineData("W/\"1\"")]
    [InlineData("\"0\"")]
    [InlineData("\"-1\"")]
    [InlineData("\"9223372036854775808\"")]
    [InlineData("\"1\", \"2\"")]
    public async Task InvalidIfMatchDoesNotReserveKey(string etag)
    {
        using var f=new SqliteFixture();var id=f.Seed();
        using var request=new HttpRequestMessage(HttpMethod.Post,$"/api/change-requests/{id}/submit");
        request.Headers.Authorization=new AuthenticationHeaderValue("Bearer",f.Tokens["alice"]);request.Headers.Add("Idempotency-Key","header");
        request.Headers.TryAddWithoutValidation("If-Match",etag);
        Assert.Equal(HttpStatusCode.BadRequest,(await f.Client.SendAsync(request,TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(1L,f.Scalar("SELECT Version FROM ChangeRequest"));Assert.Equal(0L,f.Scalar("SELECT COUNT(*) FROM OperationResult"));
    }
    [Theory]
    [InlineData("large",413)]
    [InlineData("type",415)]
    [InlineData("command",400)]
    public async Task InvalidTransportDoesNotWriteEntityOrLedger(string kind,int expected)
    {
        using var f=new SqliteFixture();var id=f.Seed();
        using var request=new HttpRequestMessage(HttpMethod.Post,kind=="command"?$"/api/change-requests/{id}/submit":"/api/change-requests");
        request.Headers.Authorization=new AuthenticationHeaderValue("Bearer",f.Tokens["alice"]);request.Headers.Add("Idempotency-Key","transport");
        request.Headers.TryAddWithoutValidation("If-Match","\"1\"");
        request.Content=new StringContent(kind=="large"?new string('x',128*1024+1):"{\"title\":\"extra\"}",Encoding.UTF8,kind=="type"?"text/plain":"application/json");
        Assert.Equal(expected,(int)(await f.Client.SendAsync(request,TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(1L,f.Scalar("SELECT Version FROM ChangeRequest"));Assert.Equal(0L,f.Scalar("SELECT COUNT(*) FROM OperationResult"));
    }
    [Fact]
    public async Task VersionOverflowReturnsSafeErrorWithoutPartialChange()
    {
        using var f=new SqliteFixture();var id=f.Seed(version:long.MaxValue);
        var response=await f.Send("PUT",$"/api/change-requests/{id}","{\"title\":\"overflow\"}",key:"overflow",version:long.MaxValue);
        Assert.Equal(HttpStatusCode.InternalServerError,response.StatusCode);
        Assert.Equal(long.MaxValue,f.Scalar("SELECT Version FROM ChangeRequest"));Assert.Equal("original",f.Scalar("SELECT Title FROM ChangeRequest"));
        Assert.Equal(0L,f.Scalar("SELECT COUNT(*) FROM OperationResult"));
        Assert.DoesNotContain("Overflow",await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken),StringComparison.OrdinalIgnoreCase);
    }
}
