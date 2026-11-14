using ChangeFlow.Api.Persistence;
namespace ChangeFlow.Tests.Fixtures;
public static class MutationFaults
{
    public static Action<MutationPoint,MutationRequest> Once(MutationPoint point)
    {
        var fired=0;
        return (p,_)=>{if(p==point&&Interlocked.Exchange(ref fired,1)==0)throw new IOException("Injected persistence failure");};
    }
    public static async Task<(HttpResponseMessage First,HttpResponseMessage Second)> Race(
        SqliteFixture f,Func<Task<HttpResponseMessage>> winner,Func<Task<HttpResponseMessage>> contender,
        CancellationToken ct)
    {
        var reserved=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondStarted=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var reservations=0;
        f.Gates.Callback=(p,_)=>{
            if(p==MutationPoint.BeforeWriterReservation&&reserved.Task.IsCompleted)secondStarted.TrySetResult();
            if(p==MutationPoint.AfterWriterReservation&&Interlocked.Increment(ref reservations)==1){
                reserved.TrySetResult();release.Task.WaitAsync(TimeSpan.FromSeconds(10),ct).GetAwaiter().GetResult();
            }
        };
        var a=Task.Run(winner,ct);
        try {
            await reserved.Task.WaitAsync(TimeSpan.FromSeconds(10),ct);
            var b=Task.Run(contender,ct);
            await secondStarted.Task.WaitAsync(TimeSpan.FromSeconds(10),ct);
            release.TrySetResult();
            return (await a.WaitAsync(TimeSpan.FromSeconds(15),ct),await b.WaitAsync(TimeSpan.FromSeconds(15),ct));
        } finally {release.TrySetResult();}
    }
}
