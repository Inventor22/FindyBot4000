using System.Collections.Generic;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace FindyBot4000
{
    /// <summary>
    /// Redefining, since this class in internal in Bot.Builder.Community.Adapters.ActionsSDK 
    /// and therefore inaccessible to ActionsSdkAdapterCustom
    /// </summary>
    internal class TurnContextEx : TurnContext
    {
        public TurnContextEx(BotAdapter adapter, Activity activity) : base(adapter, activity)
        {
            this.OnSendActivities(async (ctx, activities, nextSend) =>
            {
                var responses = await nextSend().ConfigureAwait(false);

                SentActivities.AddRange(activities);

                return responses;
            });
        }

        public List<Activity> SentActivities { get; internal set; } = new List<Activity>();
    }
}