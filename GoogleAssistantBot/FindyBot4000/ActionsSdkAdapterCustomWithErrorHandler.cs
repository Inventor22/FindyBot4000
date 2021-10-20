using Bot.Builder.Community.Adapters.ActionsSDK;
using Microsoft.Extensions.Logging;

namespace FindyBot4000
{
    public class ActionsSdkAdapterCustomWithErrorHandler : ActionsSdkAdapterCustom
    {
        public ActionsSdkAdapterCustomWithErrorHandler(ILogger<ActionsSdkAdapter> logger, ActionsSdkAdapterOptions adapterOptions)
            : base(adapterOptions, logger)
        {
            OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application.
                logger.LogError($"Exception caught : {exception.Message}");

                // Send a catch-all apology to the user.
                await turnContext.SendActivityAsync("Sorry, it looks like something went wrong.");
            };
        }
    }
}
