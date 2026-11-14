namespace ChangeFlow.Api;
public static class HttpsBindings
{
    public static string[] Resolve(IConfiguration configuration)
    {
        var urls=(configuration["urls"]??"https://localhost:7043").Split(';',StringSplitOptions.RemoveEmptyEntries);
        var endpoints=configuration.GetSection("Kestrel:Endpoints").GetChildren()
            .Select(endpoint=>endpoint["Url"]).Where(url=>url is not null).Cast<string>();
        if(urls.Length==0||urls.Concat(endpoints).Any(url=>!url.StartsWith("https://",StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("ChangeFlow requires HTTPS bindings; configure a local development certificate.");
        return urls;
    }
}
