using System.Net;
using System.Net.Http.Headers;
using System.Text;
using ChangeFlow.Tests.Fixtures;
using Xunit;

namespace ChangeFlow.Tests.Api;

public sealed class StaleContentUpdateTests
{
    [Fact]
    public async Task StalePutWithFreshKeyPreservesNewerContentAndReplaysVersionConflict()
    {
        using var fixture = new SqliteFixture();
        var id = fixture.Seed();
        var path = $"/api/change-requests/{id}";
        var cancellationToken = TestContext.Current.CancellationToken;

        async Task<HttpResponseMessage> Put(string json, string key, string etag)
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, path);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", fixture.Tokens["alice"]);
            request.Headers.Add("Idempotency-Key", key);
            request.Headers.TryAddWithoutValidation("If-Match", etag);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            return await fixture.Client.SendAsync(request, cancellationToken);
        }

        (string Title, string Description, string Reason, string Owner, string Status, long Version) ReadStored()
        {
            using var connection = fixture.Database.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT Title,Description,Reason,RequesterId,Status,Version FROM ChangeRequest WHERE Id=$id";
            command.Parameters.AddWithValue("$id", id);
            using var reader = command.ExecuteReader();
            Assert.True(reader.Read());
            var stored = (reader.GetString(0), reader.GetString(1), reader.GetString(2),
                reader.GetString(3), reader.GetString(4), reader.GetInt64(5));
            Assert.False(reader.Read());
            return stored;
        }

        using var original = await fixture.Send("GET", path);
        Assert.Equal(HttpStatusCode.OK, original.StatusCode);
        var originalEtag = original.Headers.ETag!.Tag;
        var originalVersion = (await SqliteFixture.Json(original)).GetProperty("version").GetInt64();

        const string newerInput = "{\"title\":\"newer title\",\"description\":\"newer description\",\"reason\":\"newer reason\"}";
        using var changed = await Put(newerInput, "newer-edit", originalEtag);
        Assert.Equal(HttpStatusCode.OK, changed.StatusCode);

        using var newer = await fixture.Send("GET", path);
        Assert.Equal(HttpStatusCode.OK, newer.StatusCode);
        var newerBody = await newer.Content.ReadAsStringAsync(cancellationToken);
        var newerView = await SqliteFixture.Json(newer);
        Assert.Equal(originalVersion + 1, newerView.GetProperty("version").GetInt64());
        Assert.Equal("newer title", newerView.GetProperty("title").GetString());
        Assert.Equal("newer description", newerView.GetProperty("description").GetString());
        Assert.Equal("newer reason", newerView.GetProperty("reason").GetString());
        Assert.Equal(SqliteFixture.User("alice"), newerView.GetProperty("requesterId").GetString());
        Assert.Equal("Draft", newerView.GetProperty("status").GetString());
        Assert.NotEqual(originalEtag, newer.Headers.ETag!.Tag);
        var expected = ("newer title", "newer description", "newer reason",
            SqliteFixture.User("alice"), "Draft", originalVersion + 1);
        Assert.Equal(expected, ReadStored());

        const string staleInput = "{\"title\":\"stale title\",\"description\":\"stale description\",\"reason\":\"stale reason\"}";
        using var rejected = await Put(staleInput, "stale-edit", originalEtag);
        Assert.Equal(HttpStatusCode.PreconditionFailed, rejected.StatusCode);
        Assert.Equal("VERSION_CONFLICT", (await SqliteFixture.Json(rejected)).GetProperty("code").GetString());
        var rejectedBody = await rejected.Content.ReadAsStringAsync(cancellationToken);

        using var afterRejected = await fixture.Send("GET", path);
        Assert.Equal(HttpStatusCode.OK, afterRejected.StatusCode);
        Assert.Equal(newerBody, await afterRejected.Content.ReadAsStringAsync(cancellationToken));
        Assert.Equal(newer.Headers.ETag.Tag, afterRejected.Headers.ETag!.Tag);
        Assert.Equal(expected, ReadStored());

        using (var connection = fixture.Database.Open())
        using (var command = connection.CreateCommand())
        {
            command.CommandText = "SELECT HttpStatus,ResultCode,ResponseJson,TargetId FROM OperationResult WHERE ActorId=$actor AND OperationKind='ReplaceContent' AND IdempotencyKey=$key";
            command.Parameters.AddWithValue("$actor", SqliteFixture.User("alice"));
            command.Parameters.AddWithValue("$key", "stale-edit");
            using var reader = command.ExecuteReader();
            Assert.True(reader.Read());
            Assert.Equal(412, reader.GetInt32(0));
            Assert.Equal("VERSION_CONFLICT", reader.GetString(1));
            Assert.Equal(rejectedBody, reader.GetString(2));
            Assert.Equal(id, reader.GetString(3));
            Assert.False(reader.Read());
        }
        Assert.Equal(2L, fixture.Scalar("SELECT COUNT(*) FROM OperationResult"));

        using var replay = await Put(staleInput, "stale-edit", originalEtag);
        Assert.Equal(HttpStatusCode.PreconditionFailed, replay.StatusCode);
        Assert.Equal(rejectedBody, await replay.Content.ReadAsStringAsync(cancellationToken));
        Assert.Equal(expected, ReadStored());
        Assert.Equal(2L, fixture.Scalar("SELECT COUNT(*) FROM OperationResult"));
        using var afterReplay = await fixture.Send("GET", path);
        Assert.Equal(HttpStatusCode.OK, afterReplay.StatusCode);
        Assert.Equal(newerBody, await afterReplay.Content.ReadAsStringAsync(cancellationToken));
        Assert.Equal(newer.Headers.ETag.Tag, afterReplay.Headers.ETag!.Tag);
        Assert.Equal(1L, fixture.Scalar("SELECT COUNT(*) FROM ChangeRequest"));
    }
}
