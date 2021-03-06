using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;

namespace FindyBot4000
{
    // This ASP Controller is created to handle a request. Dependency Injection will provide the Adapter and IBot
    // implementation at runtime. Multiple different IBot implementations running at different endpoints can be
    // achieved by specifying a more specific type for the bot constructor argument.
    [Route("api/actionssdk")]
    [ApiController]
    public class GoogleController : ControllerBase
    {
        private readonly ActionsSdkAdapterCustom Adapter;
        private readonly IBot Bot;

        public GoogleController(ActionsSdkAdapterCustom adapter, IBot bot)
        { 
            Adapter = adapter;
            Bot = bot;
        }

        [HttpPost]
        public async Task PostAsync()
        {
            // Delegate the processing of the HTTP POST to the adapter.
            // The adapter will invoke the bot.
            await Adapter.ProcessAsync(Request, Response, Bot);
        }
    }
}
