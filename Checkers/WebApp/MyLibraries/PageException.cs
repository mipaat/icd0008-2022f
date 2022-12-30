using Microsoft.AspNetCore.Mvc;

namespace WebApp.MyLibraries;

public class PageException : Exception
{
    private readonly string _customPath;
    public RedirectToPageResult RedirectTarget => new RedirectToPageResult(_customPath, new { error = Message });

    public PageException(string message, string customPath = "/Index") : base(message)
    {
        _customPath = customPath;
    }
}