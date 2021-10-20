using Bot.Builder.Community.Adapters.ActionsSDK.Core;
using Bot.Builder.Community.Adapters.ActionsSDK.Core.Model;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace FindyBot4000
{
    /// <summary>
    /// Extends the utility of ActionsSdkRequestMapper.
    /// 1. Make the incoming ActionsSdkRequest accessible in the returned Activity
    /// 2. Configure the next scene for Actions on Google to transition to from the webhook response
    /// </summary>
    public class ActionsSdkRequestMapperCustom : ActionsSdkRequestMapper
    {
        public ActionsSdkRequestMapperCustom(ActionsSdkRequestMapperOptions options = null, ILogger logger = null)
            : base(options, logger)
        {
        }

        public new Activity RequestToActivity(ActionsSdkRequest request)
        {
            Activity activity = base.RequestToActivity(request);

            activity.Name = request.Intent.Name;
            activity.Value = request;
            activity.ValueType = request.GetType().ToString();

            return activity;
        }

        public new ActionsSdkResponse ActivityToResponse(Activity activity, ActionsSdkRequest request)
        {
            ActionsSdkResponse response = base.ActivityToResponse(activity, request);

            if (request.Intent.Name == "RemoveObject")
            {
                response.Scene.Next.Name = "ConfirmRemove";
            }

            return response;
        }
    }
}
