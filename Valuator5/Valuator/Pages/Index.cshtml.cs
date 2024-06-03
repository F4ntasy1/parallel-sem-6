using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NATS.Client;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Text;
using System.Text.Json;

namespace Valuator.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly Repository repository;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
        repository = new Repository();
    }

    public void OnGet()
    {

    }

    public IActionResult OnPost(string text, string country)
    {
        _logger.LogDebug(text);

        string id = Guid.NewGuid().ToString();
        string region = RegionTypes.COUNTRY_TO_REGION[country];

        int similarity = GetSimilarity(text, region);

        repository.StoreText(id, text, region);

        repository.StoreSimilarity(id, similarity);

        PublishSimilarity(id, similarity);

        CancellationTokenSource cts = new();
        Task t = Task.Factory.StartNew(() => CalculateRankAsync(cts.Token, id, region), cts.Token);
        t.Wait();

        return Redirect($"summary?id={id}");
    }

    // Публикация в nats
    private void PublishSimilarity(string id, int similarity)
    {
        ConnectionFactory cf = new();
        IConnection c = cf.CreateConnection();

        var msgBytes = Encoding.UTF8.GetBytes($"{id};{similarity}");
        c.Publish("SimilarityCalculated", msgBytes);
    }

    private int GetSimilarity(string text, string country)
    {
        foreach (var value in repository.GetValuesByKey("TEXT", country))
        {
            if (value == text)
            {
                return 1;
            }
        }
        return 0;
    }

    private async Task CalculateRankAsync(CancellationToken ct, string id, string region)
    {
        ConnectionFactory cf = new();

        using (IConnection c = cf.CreateConnection())
        {
            var message = new
            {
                Id = id,
                HostAndPort = Environment.GetEnvironmentVariable(region),
                Region = region
            };

            byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            do
            {
                c.Publish("valuator.processing.rank", data);
                await Task.Delay(1000);
            } while (ct.IsCancellationRequested);
            c.Drain();

            c.Close();
        }
    }
}
