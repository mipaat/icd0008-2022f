using DAL;
using Domain.Model;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.MyLibraries.PageModels;

namespace WebApp.Pages.CheckersStates;

public class EditModel : EditModel<CheckersState>
{
    public IEnumerable<SelectListItem> CheckersGameSelectListItems
    {
        get
        {
            var result = new List<SelectListItem>();
            foreach (var checkersGame in Ctx.CheckersGameRepository.GetAll())
            {
                result.Add(new SelectListItem(checkersGame.ToString(true), checkersGame.Id.ToString(), checkersGame.Id == Entity.CheckersGameId));
            }

            return result;
        }
    }

    public EditModel(IRepositoryContext ctx) : base(ctx)
    {
    }

    protected override IRepository<CheckersState> Repository => Ctx.CheckersStateRepository;
}