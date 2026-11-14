using ChangeFlow.Api.Application;
using ChangeFlow.Api.Persistence;
using ChangeFlow.Core;
namespace ChangeFlow.Api.Endpoints;
public static class ContentEndpoints
{
    public static string Id(string id)=>Guid.TryParse(id,out var parsed)?parsed.ToString():throw new InputFailure(400,"INVALID_ID");
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/change-requests",async (HttpContext ctx,ContentCommands command)=>{
            var content=await HttpContract.Read(ctx.Request,true);
            var actor=HttpContract.Actor(ctx,Roles.Requester);var key=HttpContract.Key(ctx.Request);
            await HttpContract.Write(ctx,command.Execute(actor,new("Create",null,null,key,content)));
        });
        app.MapPut("/api/change-requests/{id}",async (HttpContext ctx,string id,ContentCommands command)=>{
            var content=await HttpContract.Read(ctx.Request,true);
            var actor=HttpContract.Actor(ctx,Roles.Requester);var key=HttpContract.Key(ctx.Request);var version=HttpContract.Version(ctx.Request);
            await HttpContract.Write(ctx,command.Execute(actor,new("ReplaceContent",Id(id),version,key,content)));
        });
    }
}
