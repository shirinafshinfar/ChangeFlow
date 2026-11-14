using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ChangeFlow.Api.Persistence;
using ChangeFlow.Core;
namespace ChangeFlow.Api.Endpoints;
public sealed class InputFailure(int status,string code):Exception
{
    public OperationResult Result {get;}=OperationResult.Error(status,code);
}
public static partial class HttpContract
{
    public const int MaxBody=128*1024;
    public static ActorContext Actor(HttpContext context,Roles roles,bool either=false)
    {
        if(context.User.Identity?.IsAuthenticated!=true||context.Items[typeof(ActorContext)] is not ActorContext actor)
            throw new InputFailure(401,"UNAUTHORIZED");
        if(either?(actor.Roles&roles)==0:!actor.Has(roles))throw new InputFailure(403,"ROLE_REQUIRED");
        return actor;
    }
    public static string Key(HttpRequest request)
    {
        var key=request.Headers["Idempotency-Key"];
        if(key.Count!=1||string.IsNullOrEmpty(key[0]))throw new InputFailure(400,"IDEMPOTENCY_KEY_REQUIRED");
        var value=key[0]!;
        if(value.Length>128||value.Any(ch=>ch<33||ch>126))throw new InputFailure(400,"INVALID_IDEMPOTENCY_KEY");
        return value;
    }
    public static long Version(HttpRequest request)
    {
        var value=request.Headers.IfMatch;
        if(value.Count==0)throw new InputFailure(428,"PRECONDITION_REQUIRED");
        var match=VersionPattern().Match(value.ToString());
        if(value.Count!=1||!match.Success||!long.TryParse(match.Groups[1].Value,NumberStyles.None,CultureInfo.InvariantCulture,out var v)||v<=0)
            throw new InputFailure(400,"INVALID_PRECONDITION");
        return v;
    }
    [GeneratedRegex("^\"([0-9]+)\"$")]
    private static partial Regex VersionPattern();
    public static async Task<ContentInput?> Read(HttpRequest request,bool content)
    {
        if(request.ContentLength>MaxBody)throw new InputFailure(413,"PAYLOAD_TOO_LARGE");
        using var buffer=new MemoryStream();
        var bytes=new byte[8192]; int size;
        while((size=await request.Body.ReadAsync(bytes))>0) {
            if(buffer.Length+size>MaxBody)throw new InputFailure(413,"PAYLOAD_TOO_LARGE");
            buffer.Write(bytes,0,size);
        }
        if(buffer.Length==0&&!content)return null;
        if(request.ContentType is null||!request.ContentType.Split(';')[0].Trim().Equals("application/json",StringComparison.OrdinalIgnoreCase))
            throw new InputFailure(415,"UNSUPPORTED_MEDIA_TYPE");
        try {
            using var json=JsonDocument.Parse(buffer.ToArray(),new JsonDocumentOptions{MaxDepth=32});
            if(json.RootElement.ValueKind!=JsonValueKind.Object)throw new InputFailure(400,"INVALID_JSON");
            var seen=new HashSet<string>(StringComparer.Ordinal); string? title=null,description=null,reason=null;
            foreach(var property in json.RootElement.EnumerateObject()) {
                if(!seen.Add(property.Name)||!content||property.Name is not ("title" or "description" or "reason"))
                    throw new InputFailure(400,"INVALID_JSON");
                if(property.Value.ValueKind is not (JsonValueKind.String or JsonValueKind.Null))throw new InputFailure(400,"INVALID_JSON");
                var text=property.Value.ValueKind==JsonValueKind.Null?null:property.Value.GetString();
                ValidateUtf16(text);
                switch(property.Name) {case "title":title=text;break;case "description":description=EmptyToNull(text);break;case "reason":reason=EmptyToNull(text);break;}
            }
            return content?new(title,description,reason):null;
        } catch(Exception ex) when(ex is JsonException or InvalidOperationException or ArgumentException) {throw new InputFailure(400,"INVALID_JSON");}
    }
    private static string? EmptyToNull(string? s)=>s is ""?null:s;
    private static void ValidateUtf16(string? s)
    {
        if(s is null)return;
        for(var i=0;i<s.Length;i++)if(char.IsSurrogate(s[i])) {
            if(!char.IsHighSurrogate(s[i])||i+1>=s.Length||!char.IsLowSurrogate(s[++i]))throw new InputFailure(400,"INVALID_JSON");
        }
    }
    public static Task Write(HttpContext context,OperationResult result)
    {
        context.Response.StatusCode=result.Status;
        context.Response.ContentType=result.Status>=400?"application/problem+json":"application/json";
        context.Response.Headers.CacheControl="no-store";
        if(result.Status==401)context.Response.Headers.WWWAuthenticate="Bearer";
        if(result.Status==503)context.Response.Headers.RetryAfter="1";
        if(result.Location is not null)context.Response.Headers.Location=result.Location;
        return context.Response.WriteAsync(result.Json);
    }
}
