using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.ActionsSDK;
using Bot.Builder.Community.Adapters.ActionsSDK.Core;
using Bot.Builder.Community.Adapters.ActionsSDK.Core.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace FindyBot4000
{
    /// <summary>
    /// Extends the utility of ActionsSdkAdapter to allow a custom ActionsSdkRequestMapper.
    /// ActionsSdkRequestMapperCustom makes the incoming ActionsSdkRequest accessible to Bot classes.
    /// </summary>
    public class ActionsSdkAdapterCustom : ActionsSdkAdapter
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
        };

        private readonly ILogger _logger;
        private readonly ActionsSdkRequestMapperOptions _actionsSdkRequestMapperOptions;
        private readonly ActionsSdkAdapterOptions _options;

        public ActionsSdkAdapterCustom(ActionsSdkAdapterOptions options = null, ILogger logger = null)
            : base(options, logger)
        {
            _options = options ?? new ActionsSdkAdapterOptions();
            _logger = logger ?? NullLogger.Instance;

            _actionsSdkRequestMapperOptions = new ActionsSdkRequestMapperOptions()
            {
                ActionInvocationName = _options.ActionInvocationName,
                ShouldEndSessionByDefault = _options.ShouldEndSessionByDefault
            };
        }

        /// <summary>
        /// Override ProcessAsync with the only intention of replacing ActionsSdkRequestMapper with ActionsSdkRequestMapperCustom
        /// </summary>
        public async new Task ProcessAsync(HttpRequest httpRequest, HttpResponse httpResponse, IBot bot, CancellationToken cancellationToken = default)
        {
            if (httpRequest == null)
            {
                throw new ArgumentNullException(nameof(httpRequest));
            }

            if (httpResponse == null)
            {
                throw new ArgumentNullException(nameof(httpResponse));
            }

            if (bot == null)
            {
                throw new ArgumentNullException(nameof(bot));
            }

            string body;
            using (var sr = new StreamReader(httpRequest.Body))
            {
                body = await sr.ReadToEndAsync();
            }

            var actionsSdkRequest = JsonConvert.DeserializeObject<ActionsSdkRequest>(body);
            var actionsSdkRequestMapper = new ActionsSdkRequestMapperCustom(_actionsSdkRequestMapperOptions, _logger);
            var activity = actionsSdkRequestMapper.RequestToActivity(actionsSdkRequest);
            var context = await CreateContextAndRunPipelineAsync(bot, cancellationToken, activity);
            var actionsSdkResponse = actionsSdkRequestMapper.ActivityToResponse(await ProcessOutgoingActivitiesAsync(context.SentActivities, context), actionsSdkRequest);
            var responseJson = JsonConvert.SerializeObject(actionsSdkResponse, JsonSerializerSettings);
            
            httpResponse.ContentType = "application/json;charset=utf-8";
            httpResponse.StatusCode = (int)HttpStatusCode.OK;

            var responseData = Encoding.UTF8.GetBytes(responseJson);
            await httpResponse.Body.WriteAsync(responseData, 0, responseData.Length, cancellationToken).ConfigureAwait(false);
        }

        private async Task<TurnContextEx> CreateContextAndRunPipelineAsync(IBot bot, CancellationToken cancellationToken, Activity activity)
        {
            var context = new TurnContextEx(this, activity);
            await RunPipelineAsync(context, bot.OnTurnAsync, cancellationToken).ConfigureAwait(false);
            return context;
        }
    }
}