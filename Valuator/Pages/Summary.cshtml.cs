using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Valuator.Pages;
public class SummaryModel : PageModel
{
    private readonly ILogger<SummaryModel> _logger;
    private readonly Repository repository;

    public SummaryModel(ILogger<SummaryModel> logger)
    {
        _logger = logger;
        repository = new Repository();
    }

    public string Rank { get; set; }
    public string Similarity { get; set; }

    public void OnGet(string id)
    {
        _logger.LogDebug(id);

        Rank = repository.Get("RANK-" + id)?[..5] ?? "0";
        Similarity = repository.Get("SIMILARITY-" + id)?[..1] ?? "0";
    }
}
