using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Domain;

namespace WebApp.Pages.CheckersStates
{
    public class IndexModel : PageModel
    {
        private readonly DAL.Db.AppDbContext _context;

        public IndexModel(DAL.Db.AppDbContext context)
        {
            _context = context;
        }

        public IList<CheckersState> CheckersState { get; set; } = default!;

        public async Task OnGetAsync()
        {
            if (_context.CheckersStates != null)
            {
                CheckersState = await _context.CheckersStates.ToListAsync();
            }
        }

        public static string ShortenSerializedGamePieces(CheckersState checkersState)
        {
            return checkersState.SerializedGamePieces.Length > 50
                ? checkersState.SerializedGamePieces[..47] + "..."
                : checkersState.SerializedGamePieces;
        }
    }
}