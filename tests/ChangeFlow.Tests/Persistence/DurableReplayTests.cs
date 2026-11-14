using System.Net;
using System.Net.Http.Headers;
using System.Text;
using ChangeFlow.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
namespace ChangeFlow.Tests.Persistence;
public sealed class DurableReplayTests
{
    private sealed class Later:TimeProvider {public override DateTimeOffset GetUtcNow()=>DateTimeOffset.UtcNow.AddHours(25);}
    [Fact]
    public async Task RestartAndClockAdvancePreserveReceiptWithoutTtl()
    {
        using var f=new SqliteFixture();
        var first=await f.Send("POST","/api/change-requests","{\"title\":\"durable\"}",key:"durable");
        var body=await first.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        f.Client.Dispose();await f.Host.DisposeAsync();
        using var restart=new ChangeFlowFactory(f.Database.Path,configure:services=>services.AddSingleton<TimeProvider>(new Later()));
        using var client=restart.CreateClient(new(){BaseAddress=new Uri("https://localhost")});
        using var request=new HttpRequestMessage(HttpMethod.Post,"/api/change-requests");
        request.Headers.Authorization=new AuthenticationHeaderValue("Bearer",f.Tokens["alice"]);request.Headers.Add("Idempotency-Key","durable");
        request.Content=new StringContent("{\"title\":\"durable\"}",Encoding.UTF8,"application/json");
        var replay=await client.SendAsync(request,TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created,replay.StatusCode);Assert.Equal(body,await replay.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.Equal(first.Headers.Location,replay.Headers.Location);Assert.Equal(1L,f.Scalar("SELECT COUNT(*) FROM ChangeRequest"));
        Assert.Equal(1L,f.Scalar("SELECT COUNT(*) FROM OperationResult"));
        var schema=(string)f.Scalar("SELECT sql FROM sqlite_schema WHERE name='OperationResult'")!;
        Assert.DoesNotContain("Expires",schema,StringComparison.OrdinalIgnoreCase);Assert.DoesNotContain("TTL",schema,StringComparison.OrdinalIgnoreCase);
    }
}
