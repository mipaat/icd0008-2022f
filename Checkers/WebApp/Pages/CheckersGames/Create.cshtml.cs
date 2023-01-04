using DAL;
using Domain.Model;
using Domain.Model.Helpers;
using GameBrain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.MyLibraries.PageModels;

namespace WebApp.Pages.CheckersGames;

public class CreateModel : PageModelDb
{
    public CreateModel(IRepositoryContext ctx) : base(ctx)
    {
    }

    [BindProperty] public string? WhitePlayerName { get; set; }
    [BindProperty] public string? BlackPlayerName { get; set; }
    [BindProperty] public EAiType? WhiteAiType { get; set; }
    [BindProperty] public EAiType? BlackAiType { get; set; }
    [BindProperty] public int CheckersRulesetId { get; set; }

    public CheckersRuleset CheckersRuleset { get; set; } = default!;

    public IEnumerable<SelectListItem> CheckersRulesetSelectListItems => new List<SelectListItem>
    {
        new SelectListItem(CheckersRuleset.ToString(), CheckersRuleset.Id.ToString(), true)
    };

    public IActionResult OnGet(int? selectedCheckersRulesetId)
    {
        if (selectedCheckersRulesetId == null) return RedirectToPage("/CheckersRulesets/Select");
        CheckersRulesetId = selectedCheckersRulesetId.Value;
        var checkersRuleset = Ctx.CheckersRulesetRepository.GetById(CheckersRulesetId);
        if (checkersRuleset == null) return NotFound();
        CheckersRuleset = checkersRuleset;
        return Page();
    }

    // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
    public IActionResult OnPost()
    {
        if (!ModelState.IsValid) return Page();

        var checkersRuleset = Ctx.CheckersRulesetRepository.GetById(CheckersRulesetId);
        if (checkersRuleset?.Saved == true) checkersRuleset = checkersRuleset.GetClone(false); 

        if (checkersRuleset == null) return Page();

        var checkersBrain =
            new CheckersBrain(checkersRuleset, WhitePlayerName, BlackPlayerName, WhiteAiType, BlackAiType);
        var checkersGame = checkersBrain.CheckersGame;
        Ctx.CheckersGameRepository.Add(checkersGame);

        return RedirectToPage("./Launch", new { id = checkersGame.Id });
    }
}