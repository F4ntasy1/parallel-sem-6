using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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

        string textKey = "TEXT-" + id;

        string rankKey = "RANK-" + id;
        repository.Set(rankKey, GetRank(text));

        string similarityKey = "SIMILARITY-" + id;
        repository.Set(similarityKey, GetSimilarity(text));

        repository.Set(textKey, text);

        return Redirect($"summary?id={id}");
    }

    private double GetRank(string text)
    {
        double notLetterCharacters = 0;
        foreach (var ch in text)
        {
            if (!char.IsLetter(ch))
            {
                notLetterCharacters++;
            }
        }
        return notLetterCharacters / text.Length;
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
}
