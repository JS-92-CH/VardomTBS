using MainCore.Constraints;
using MainCore.Errors;
using MainCore.Parsers;
using OpenQA.Selenium;

namespace MainCore.Commands.Features.StartAdventure
{
    [Handler]
    public static partial class ExploreAdventureCommand
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var html = browser.Html;

            if (!AdventureParser.CanStartAdventure(html)) return Skip.NoAdventure;

            var adventureButton = AdventureParser.GetAdventureButton(html);
            if (adventureButton is null) return Retry.ButtonNotFound("adventure");
            logger.Information("Start adventure {Adventure}", AdventureParser.GetAdventureInfo(adventureButton));

            var result = await browser.Click(By.XPath(adventureButton.XPath));
            if (result.IsFailed) return result;

            static bool HeroIsMoving(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                var heroStatus = doc.DocumentNode
                    .Descendants("div")
                    .FirstOrDefault(x => x.HasClass("heroStatus"));
                if (heroStatus is null) return false;
                var heroHome = heroStatus.Descendants("i")
                    .Any(x => x.HasClass("heroHome"));
                return !heroHome;
            }

            result = await browser.Wait(HeroIsMoving, cancellationToken);
            if (result.IsFailed)
            {
                return result.WithError(new Error("Hero is still at home after trying to start adventure."));
            }

            return Result.Ok();
        }
    }
}