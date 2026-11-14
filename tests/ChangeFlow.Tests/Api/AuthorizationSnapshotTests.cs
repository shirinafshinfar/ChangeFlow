using System.Net;
using ChangeFlow.Api.Persistence;
using ChangeFlow.Core;
using ChangeFlow.Tests.Fixtures;
using Xunit;
namespace ChangeFlow.Tests.Api;
public sealed class AuthorizationSnapshotTests
{
    [Fact]
    public async Task RevocationAfterSnapshotDoesNotCancelCurrentButRejectsNextAndReplay()
    {
        using var f=new SqliteFixture();var id=f.Seed();
        var cancellationToken=TestContext.Current.CancellationToken;
        var entered=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        f.Gates.Callback=(p,r)=>{if(p==MutationPoint.AfterSnapshot&&r.Key=="inflight"){entered.TrySetResult();release.Task.WaitAsync(TimeSpan.FromSeconds(10),cancellationToken).GetAwaiter().GetResult();}};
        var attempt=Task.Run(()=>f.Send("POST",$"/api/change-requests/{id}/submit",key:"inflight",version:1),cancellationToken);
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(10),cancellationToken);
        f.Revoke("alice",Roles.Requester);release.TrySetResult();
        Assert.Equal(HttpStatusCode.OK,(await attempt.WaitAsync(TimeSpan.FromSeconds(10),cancellationToken)).StatusCode);
        Assert.Equal("PendingReview",f.Scalar("SELECT Status FROM ChangeRequest"));
        Assert.Equal(HttpStatusCode.Forbidden,(await f.Send("POST",$"/api/change-requests/{id}/submit",key:"inflight",version:1)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,(await f.Send("POST",$"/api/change-requests/{id}/submit",key:"next",version:2)).StatusCode);
        Assert.Equal(1L,f.Scalar("SELECT COUNT(*) FROM OperationResult"));
    }
}
