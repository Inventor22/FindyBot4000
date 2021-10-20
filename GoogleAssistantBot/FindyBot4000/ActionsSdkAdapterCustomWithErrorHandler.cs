using Bot.Builder.Community.Adapters.ActionsSDK;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Logging;
using System;

namespace FindyBot4000
{
    public class ActionsSdkAdapterCustomWithErrorHandler : ActionsSdkAdapterCustom
    {
        public ActionsSdkAdapterCustomWithErrorHandler(
            ILogger<ActionsSdkAdapterCustom> logger, 
            ActionsSdkAdapterOptions adapterOptions, 
            ConversationState conversationState = default)
            : base(adapterOptions, logger)
        {
            OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application.
                logger.LogError($"Exception caught : {exception.Message}");

                if (conversationState != null)
                {
                    try
                    {
                        // Delete the conversationState for the current conversation to prevent the
                        // bot from getting stuck in a error-loop caused by being in a bad state.
                        // ConversationState should be thought of as similar to "cookie-state" in a Web pages.
                        await conversationState.DeleteAsync(turnContext);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"Exception caught on attempting to Delete ConversationState : {ex.Message}");
                    }
                }

                // Send a catch-all apology to the user.
                await turnContext.SendActivityAsync("Sorry, it looks like something went wrong.");
            };
        }
    }
}
