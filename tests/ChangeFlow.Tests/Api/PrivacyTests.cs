using System.Net;
using System.Net.Http.Headers;
using System.Text;
using ChangeFlow.Core;
using ChangeFlow.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
namespace ChangeFlow.Tests.Api;
public sealed class PrivacyTests
{
    [Fact]
    public async Task ReceiptsLedgerAndLogsDoNotExposeContentOrCredentials()
    {
        using var f=new SqliteFixture();var id=f.Seed("PendingReview",title:"secret-title",description:"secret-description",reason:"secret-reason");
        using var logs=new CapturedLogs();
        using var host=new ChangeFlowFactory(f.Database.Path,configure:services=>{
            services.AddSingleton<ILoggerProvider>(logs);services.Configure<LoggerFilterOptions>(o=>{o.Rules.Clear();o.MinLevel=LogLevel.Information;});
        });
        using var client=host.CreateClient(new(){BaseAddress=new Uri("https://localhost")});
        async Task<HttpResponseMessage> Approve(){
            using var request=new HttpRequestMessage(HttpMethod.Post,$"/api/change-requests/{id}/approve");
            request.Headers.Authorization=new AuthenticationHeaderValue("Bearer",f.Tokens["bob"]);request.Headers.Add("Idempotency-Key","private-raw-key");request.Headers.TryAddWithoutValidation("If-Match","\"1\"");
            return await client.SendAsync(request);
        }
        var first=await Approve();var replay=await Approve();
        Assert.Equal(HttpStatusCode.OK,first.StatusCode);Assert.Equal(HttpStatusCode.OK,replay.StatusCode);
        var receipt=await replay.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Equal(2,(await SqliteFixture.Json(replay)).EnumerateObject().Count());
        Assert.Equal(HttpStatusCode.NotFound,(await f.Send("GET",$"/api/change-requests/{id}",user:"bob")).StatusCode);
        var ledger=(string)f.Scalar("SELECT ResponseJson FROM OperationResult")!;
        var all=receipt+ledger+string.Join("\n",logs.Entries);
        foreach(var secret in new[]{"secret-title","secret-description","secret-reason",f.Tokens["bob"],"private-raw-key"})
            Assert.DoesNotContain(secret,all,StringComparison.Ordinal);
        Assert.DoesNotContain("ReviewBaseline",receipt,StringComparison.Ordinal);
        Assert.Contains(logs.Structured,fields=>new[]{"correlationId","operation","actor","outcome","elapsed"}.All(key=>fields.Any(f=>f.Key==key)));
        f.Revoke("bob",Roles.Reviewer);
        Assert.Equal(HttpStatusCode.Forbidden,(await Approve()).StatusCode);
        var draft=f.Seed();f.Revoke("alice",Roles.Requester);
        Assert.Equal(HttpStatusCode.Forbidden,(await f.Send("DELETE",$"/api/change-requests/{draft}",key:"revoked",version:1)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,(await f.Send("GET",$"/api/change-requests/{draft}")).StatusCode);
        Assert.Equal(1L,f.Scalar("SELECT Version FROM ChangeRequest WHERE Status='Draft'"));
    }
}
