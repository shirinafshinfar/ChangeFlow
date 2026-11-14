using ChangeFlow.Api.Authentication;
using ChangeFlow.Api.Application;
using ChangeFlow.Api.Endpoints;
using ChangeFlow.Api.Maintenance;
using ChangeFlow.Api.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Data.Sqlite;

var maintenance=args.Contains("--seed-dev-identities")||args.Contains("--revoke-role");
var builder = WebApplication.CreateBuilder(maintenance ? Array.Empty<string>() : args);
builder.Logging.AddFilter("Microsoft.AspNetCore.Hosting.Diagnostics",LogLevel.Warning);
builder.Services.AddSingleton<SqliteConnectionFactory>();
builder.Services.AddSingleton<MutationExecutor>();
builder.Services.AddSingleton<ContentCommands>();
builder.Services.AddSingleton<SubmitCommand>();
builder.Services.AddSingleton<ReviewCommands>();
builder.Services.AddSingleton<ChangeRequestQueries>();
builder.Services.AddSingleton<ReadRequest>();
builder.Services.AddSingleton<ListRequests>();
builder.Services.AddSingleton<DeleteDraft>();
builder.Services.AddAuthentication("Bearer").AddScheme<AuthenticationSchemeOptions,OpaqueBearerHandler>("Bearer", _ => {});
var app = builder.Build();
var database=app.Services.GetRequiredService<SqliteConnectionFactory>();
var nativeVersion=SchemaInitializer.Initialize(database);
app.Logger.LogInformation("SQLite {Version}; journal=DELETE synchronous=EXTRA foreign_keys=ON",nativeVersion);
if(DevelopmentIdentityCommands.Run(args,app.Environment,database)) return;
foreach(var url in ChangeFlow.Api.HttpsBindings.Resolve(app.Configuration))app.Urls.Add(url);
app.Use(async (context,next) => {
    var started=System.Diagnostics.Stopwatch.GetTimestamp();
    context.Response.Headers.CacheControl="no-store";
    try { await next(context); }
    catch(InputFailure failure) { await HttpContract.Write(context,failure.Result); }
    catch(SqliteException) { await HttpContract.Write(context,OperationResult.Error(503,"STORAGE_UNAVAILABLE")); }
    catch(IOException) { await HttpContract.Write(context,OperationResult.Error(503,"STORAGE_UNAVAILABLE")); }
    catch(Exception) { await HttpContract.Write(context,OperationResult.Error(500,"INTERNAL_ERROR")); }
    finally {
        var actor=(context.Items[typeof(ChangeFlow.Core.ActorContext)] as ChangeFlow.Core.ActorContext)?.UserId??"unauthenticated";
        var route=(context.GetEndpoint() as Microsoft.AspNetCore.Routing.RouteEndpoint)?.RoutePattern.RawText??"unmatched";
        app.Logger.LogInformation("correlationId={correlationId} operation={operation} actor={actor} outcome={outcome} elapsed={elapsed}",
            context.TraceIdentifier,context.Request.Method+" "+route,actor,context.Response.StatusCode,
            System.Diagnostics.Stopwatch.GetElapsedTime(started).TotalMilliseconds);
    }
});
app.UseAuthentication();
ContentEndpoints.Map(app);
SubmitEndpoints.Map(app);
ReviewEndpoints.Map(app);
ReadEndpoints.Map(app);
DeleteEndpoints.Map(app);
app.Run();

public partial class Program { }
