using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using NATS.Client;

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

    public IActionResult OnPost(string text)
    {
        _logger.LogDebug(text);

        string id = Guid.NewGuid().ToString();

        string similarityKey = "SIMILARITY-" + id;
        int similarity = GetSimilarity(text);
        repository.Set(similarityKey, similarity);

        string textKey = "TEXT-" + id;
        repository.Set(textKey, text);

        PublishSimilarity(similarityKey, similarity);

        CancellationTokenSource cts = new();
        Task t = Task.Factory.StartNew(() => CalculateRankAsync(cts.Token, textKey, "RANK-" + id), cts.Token);
        t.Wait();

        return Redirect($"summary?id={id}");
    }

    // Публикация в nats
    private void PublishSimilarity(string similarityKey, int similarity)
    {
        ConnectionFactory cf = new();
        IConnection c = cf.CreateConnection();

        var msgBytes = Encoding.UTF8.GetBytes($"{similarityKey},{similarity}");
        c.Publish("SimilarityCalculated", msgBytes);
    }

    private int GetSimilarity(string text)
    {
        foreach (var value in repository.GetValuesByKey("TEXT"))
        {
            if (value == text)
            {
                return 1;
            }
        }
        return 0;
    }

    private async Task CalculateRankAsync(CancellationToken ct, string textKey, string rankKey)
    {
        ConnectionFactory cf = new();

        using (IConnection c = cf.CreateConnection())
        {
            string m = $"{textKey},{rankKey}";
            byte[] data = Encoding.UTF8.GetBytes(m);
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
