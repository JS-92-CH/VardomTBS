using MainCore.Constraints;
using MainCore.Errors;
using MainCore.Parsers;
using MainCore.Services;
using OpenQA.Selenium;

namespace MainCore.Commands.Features.StartAdventure
{
    [Handler]
    public static partial class ToAdventurePageCommand
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser,
            CancellationToken cancellationToken)
        {
            var html = browser.Html;

            var adventure = AdventureParser.GetHeroAdventureButton(html);
            if (adventure is null) return Retry.ButtonNotFound("hero adventure");

            var result = await browser.Click(By.XPath(adventure.XPath));
            if (result.IsFailed) return result;

            // This wait is more robust as it only checks for page content, not the URL.
            result = await browser.Wait(driver =>
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                return AdventureParser.IsAdventurePage(doc);
            }, cancellationToken);

            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}