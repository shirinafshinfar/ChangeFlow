using System.Data;
using ChangeFlow.Core;
using Microsoft.Data.Sqlite;
namespace ChangeFlow.Api.Persistence;
public sealed class MutationExecutor(SqliteConnectionFactory factory,IServiceProvider services)
{
    public OperationResult Execute(ActorContext actor,MutationRequest request,Func<SqliteConnection,SqliteTransaction,OperationResult> action)
    {
        var observer=services.GetService<IMutationObserver>();
        observer?.Observe(MutationPoint.AfterSnapshot,request);
        var fingerprint=OperationFingerprint.Compute(request);
        SqliteConnection? c=null; SqliteTransaction? tx=null; var commitAttempted=false;
        try {
            c=factory.Open();
            observer?.Observe(MutationPoint.BeforeWriterReservation,request);
            tx=c.BeginTransaction(IsolationLevel.Serializable,deferred:false);
            observer?.Observe(MutationPoint.AfterWriterReservation,request);
            var prior=OperationResultStore.Find(c,tx,actor,request);
            if(prior is not null) return prior.Value.Fingerprint.AsSpan().SequenceEqual(fingerprint)
                ? prior.Value.Result : OperationResult.Error(409,"IDEMPOTENCY_MISMATCH");
            var result=action(c,tx);
            observer?.Observe(MutationPoint.AfterMutation,request);
            OperationResultStore.Insert(c,tx,actor,request,fingerprint,result);
            observer?.Observe(MutationPoint.AfterLedger,request);
            observer?.Observe(MutationPoint.BeforeCommit,request);
            commitAttempted=true;
            tx.Commit();
            observer?.Observe(MutationPoint.AfterCommit,request);
            return result;
        } catch(Exception ex) when(ex is SqliteException or IOException or InvalidOperationException) {
            try { tx?.Rollback(); } catch { /* The original transaction may already be committed. */ }
            tx?.Dispose(); tx=null; c?.Dispose(); c=null;
            if(commitAttempted) {
                try {
                    observer?.Observe(MutationPoint.BeforeReconcile,request);
                    using var fresh=factory.Open();
                    using var reservation=fresh.BeginTransaction(IsolationLevel.Serializable,deferred:false);
                    var prior=OperationResultStore.Find(fresh,reservation,actor,request);
                    if(prior is not null && prior.Value.Fingerprint.AsSpan().SequenceEqual(fingerprint))
                        return prior.Value.Result;
                } catch(Exception recover) when(recover is SqliteException or IOException or InvalidOperationException) { }
                return OperationResult.Error(503,"STORAGE_UNAVAILABLE");
            }
            return ex is SqliteException or IOException
                ? OperationResult.Error(503,"STORAGE_UNAVAILABLE") : OperationResult.Error(500,"INTERNAL_ERROR");
        } finally { tx?.Dispose(); c?.Dispose(); }
    }
}
