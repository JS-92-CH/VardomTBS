using MainCore.Constraints;
using MainCore.Entities;
using MainCore.Notifications.Behaviors;
using Microsoft.EntityFrameworkCore;

namespace MainCore.Commands.UI.Misc
{
    [Handler]
    [Behaviors(typeof(AccountSettingUpdatedBehavior<,>))]
    public static partial class SaveAccountSettingCommand
    {
        public sealed record Command(AccountId AccountId, Dictionary<AccountSettingEnums, int> Settings) : IAccountCommand;

        private static async ValueTask HandleAsync(
            Command command,
            AppDbContext context,
            CancellationToken cancellationToken
            )
        {
            var (accountId, settings) = command;
            if (settings.Count == 0) return;

            foreach (var setting in settings)
            {
                var dbSetting = await context.AccountsSetting
                    .FirstOrDefaultAsync(x => x.AccountId == accountId.Value && x.Setting == setting.Key, cancellationToken);

                if (dbSetting is not null)
                {
                    if (dbSetting.Value != setting.Value)
                    {
                        dbSetting.Value = setting.Value;
                    }
                }
            }

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}