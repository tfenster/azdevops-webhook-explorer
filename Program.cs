using System.Text.Json;

if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_URLS")))
{
    Environment.SetEnvironmentVariable("ASPNETCORE_URLS", "http://0.0.0.0:8080;https://0.0.0.0:8443");
}

var app = WebApplication.CreateBuilder().Build();

app.MapPost("/webhook/inspect", async (HttpRequest request) =>
{
    using var reader = new StreamReader(request.Body);
    var body = await reader.ReadToEndAsync();
    Console.WriteLine($"Received webhook data:\n{body}");
    return Results.Ok();
});

app.MapPost("webhook/updatedWorkItem", async (HttpRequest request) =>
{
    Console.WriteLine("Updated work item received");
    using var reader = new StreamReader(request.Body);
    var body = await reader.ReadToEndAsync();
    var payload = JsonSerializer.Deserialize<AzDevOpsWebhookPayload>(body, new JsonSerializerOptions()
    {
        PropertyNameCaseInsensitive = true
    });
    if (payload == null)
    {
        return Results.BadRequest("Invalid payload");
    }

    // Process the payload as needed, e.g. check whether the title has changed
    if (payload.EventType == "workitem.updated" && payload.Resource?.Fields != null)
    {
        if (payload.Resource.Fields.TryGetValue("System.Title", out var titleField))
        {
            Console.WriteLine($"Work item updated: {payload.Resource.WorkItemId}, Old Title: {titleField.OldValue} --> New Title: {titleField.NewValue}");
        }
    }
    return Results.Ok();
});

app.Run();