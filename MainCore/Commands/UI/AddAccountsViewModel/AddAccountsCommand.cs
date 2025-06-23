using MainCore.Constraints;
using MainCore.Notifications.Behaviors;

namespace MainCore.Commands.UI.AddAccountsViewModel
{
    [Handler]
    [Behaviors(typeof(AccountListUpdatedBehavior<,>))]
    public static partial class AddAccountsCommand
    {
        public sealed record Command(List<AccountDto> Dtos) : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            AppDbContext context, IUseragentManager useragentManager,
            CancellationToken cancellationToken
            )
        {
            await Task.CompletedTask;
            
            var accounts = command.Dtos
                .Select(x => x.ToEntity());

            foreach (var access in accounts.SelectMany(x => x.Accesses).Where(access => string.IsNullOrEmpty(access.Useragent)))
            {
                access.Useragent = useragentManager.Get();
            }

            foreach (var account in accounts)
            {
                account.Info = new();

                account.Settings = [];
                foreach (var (setting, value) in AppDbContext.AccountDefaultSettings)
                {
                    account.Settings.Add(new AccountSetting
                    {
                        Setting = setting,
                        Value = value,
                    });
                }
            }

            context.AddRange(accounts);
            context.SaveChanges();
            return Result.Ok();
        }
    }
}