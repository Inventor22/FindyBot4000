using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.ActionsSDK.Core.Attachments;
using Bot.Builder.Community.Adapters.ActionsSDK.Core.Helpers;
using Bot.Builder.Community.Adapters.ActionsSDK.Core.Model;
using Bot.Builder.Community.Adapters.ActionsSDK.Core.Model.ContentItems;
using System;
using Bot.Builder.Community.Adapters.ActionsSDK.Core;

namespace FindyBot4000
{
    public class FindyBot : ActivityHandler
    {
        private BotState conversationState;
        private BotState userState;

        public FindyBot(ConversationState conversationState, UserState userState)
        {
            this.conversationState = conversationState;
            this.userState = userState;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occurred during the turn.
            await this.conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await this.userState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var conversationStateAccessors = this.conversationState.CreateProperty<ConversationData>(nameof(ConversationData));
            ConversationData conversationData = await conversationStateAccessors.GetAsync(turnContext, () => new ConversationData());

            switch (turnContext.Activity.Text.ToLower())
            {
                default:
                    try
                    {
                        ActionsSdkRequest request = (ActionsSdkRequest)turnContext.Activity.Value;

                        switch (request.Intent.Name) 
                        {
                            default:
                                await turnContext.SendActivityAsync(
                                    MessageFactory.Text(
                                        $"The intent \"{request.Intent.Name}\" is not recognized.",
                                        inputHint: InputHints.IgnoringInput),
                                    cancellationToken);
                                break;

                            case "FindObject":
                            case "FindTags":
                                await turnContext.SendActivityAsync(
                                    MessageFactory.Text(
                                        $"Echoing intent \"{request.Intent.Name}\" with content \"{request.Intent.Query}\".",
                                        inputHint: InputHints.AcceptingInput),
                                    cancellationToken);
                                break;

                            case "RemoveObject":

                                string item = new ActionsSdkRequestMapper().StripInvocation(request.Intent.Query, "remove");

                                conversationData.PreviousScene = "RemoveObject";

                                await turnContext.SendActivityAsync(
                                    MessageFactory.Text(
                                        $"Removing {item}.",
                                        inputHint: InputHints.AcceptingInput),
                                    cancellationToken);
                                break;

                            case "actions.intent.YES":

                                string response = conversationData.PreviousScene == "RemoveObject"
                                    ? "Ok, I've removed the object"
                                    : "YES";

                                conversationData.PreviousScene = "YES";

                                await turnContext.SendActivityAsync(
                                    MessageFactory.Text(
                                        response,
                                        inputHint: InputHints.IgnoringInput),
                                    cancellationToken);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        await turnContext.SendActivityAsync(
                            MessageFactory.Text(
                                $"Exception: {ex}",
                                inputHint: InputHints.IgnoringInput), 
                            cancellationToken);
                    }

                    break;

                case "finish":
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Ok, I won't ask anymore.", inputHint: InputHints.IgnoringInput), cancellationToken);
                    break;

                case "card":
                    var activityWithCard = MessageFactory.Text($"Ok, I included a simple card.");
                    var card = ContentItemFactory.CreateCard("card title", "card subtitle", new Link()
                    {
                        Name = "Microsoft",
                        Open = new OpenUrl() { Url = "https://www.microsoft.com" }
                    });
                    activityWithCard.Attachments.Add(card.ToAttachment());
                    await turnContext.SendActivityAsync(activityWithCard, cancellationToken);
                    break;

                case "signin":
                    var channelData = (ActionsSdkRequest)turnContext.Activity.ChannelData;
                    if (channelData.User.AccountLinkingStatus == "LINKED")
                    {
                        await turnContext.SendActivityAsync("You're already signed in!", cancellationToken: cancellationToken);
                    }
                    else
                    {
                        var activityWithSigninCard = MessageFactory.Text($"Ok, I included a signin card.");
                        var signinCard = new SigninCard();
                        activityWithSigninCard.Attachments.Add(signinCard.ToAttachment());
                        await turnContext.SendActivityAsync(activityWithSigninCard, cancellationToken);
                    }

                    break;

                case "chips":
                    var activityWithChips = MessageFactory.Text($"Ok, I included some suggested actions.");
                    activityWithChips.SuggestedActions = new SuggestedActions(actions: new List<CardAction>
                    {
                        new CardAction { Title = "Yes", Type= ActionTypes.ImBack, Value = "Y" },
                        new CardAction { Title = "No", Type= ActionTypes.ImBack, Value = "N" },
                        new CardAction { Title = "Click to learn more", Type= ActionTypes.OpenUrl, Value = "http://www.progressive.com" }
                    });
                    await turnContext.SendActivityAsync(activityWithChips, cancellationToken);
                    break;

                case "list":
                    var activityWithListAttachment = MessageFactory.Text($"This is a list.");
                    var list = new ListContentItem()
                    {
                        Title = "InternalList title",
                        Subtitle = "InternalList subtitle",
                        Items = new List<ListItem>()
                        {
                            new ListItem()
                            {
                                Key = "ITEM_1",
                                Synonyms = new List<string>() { "Item 1", "First item" },
                                Item = new EntryDisplay()
                                {
                                    Title = "Item #1",
                                    Description = "Description of Item #1",
                                    Image = new Image()
                                    {
                                        Url = "https://developers.google.com/assistant/assistant_96.png",
                                        Height = 0,
                                        Width = 0,
                                        Alt = "Google Assistant logo"
                                    }
                                }
                            },
                            new ListItem()
                            {
                                Key = "ITEM_2",
                                Synonyms = new List<string>() { "Item 2", "Second item" },
                                Item = new EntryDisplay()
                                {
                                    Title = "Item #2",
                                    Description = "Description of Item #2",
                                    Image = new Image()
                                    {
                                        Url = "https://developers.google.com/assistant/assistant_96.png",
                                        Height = 0,
                                        Width = 0,
                                        Alt = "Google Assistant logo"
                                    }
                                }
                            },
                            new ListItem()
                            {
                                Key = "ITEM_3",
                                Synonyms = new List<string>() { "Item 3", "Third item" },
                                Item = new EntryDisplay()
                                {
                                    Title = "Item #3",
                                    Description = "Description of Item #3",
                                    Image = new Image()
                                    {
                                        Url = "https://developers.google.com/assistant/assistant_96.png",
                                        Height = 0,
                                        Width = 0,
                                        Alt = "Google Assistant logo"
                                    }
                                }
                            },
                            new ListItem()
                            {
                                Key = "ITEM_4",
                                Synonyms = new List<string>() { "Item 4", "Fourth item" },
                                Item = new EntryDisplay()
                                {
                                    Title = "Item #4",
                                    Description = "Description of Item #4",
                                    Image = new Image()
                                    {
                                        Url = "https://developers.google.com/assistant/assistant_96.png",
                                        Height = 0,
                                        Width = 0,
                                        Alt = "Google Assistant logo"
                                    }
                                }
                            },
                        }
                    };
                    activityWithListAttachment.Attachments.Add(list.ToAttachment());
                    await turnContext.SendActivityAsync(activityWithListAttachment, cancellationToken);
                    break;

                case "collection":
                    var activityWithCollectionAttachment = MessageFactory.Text($"Ok, I included a collection.");
                    var collection = new CollectionContentItem()
                    {
                        Title = "InternalList title",
                        Subtitle = "InternalList subtitle",
                        Items = new List<CollectionItem>()
                        {
                            new CollectionItem()
                            {
                                Key = "ITEM_1",
                                Synonyms = new List<string>() { "Item 1", "First item" },
                                Item = new EntryDisplay()
                                {
                                    Title = "Item #1",
                                    Description = "Description of Item #1",
                                    Image = new Image()
                                    {
                                        Url = "https://developers.google.com/assistant/assistant_96.png",
                                        Height = 0,
                                        Width = 0,
                                        Alt = "Google Assistant logo"
                                    }
                                }
                            },
                            new CollectionItem()
                            {
                                Key = "ITEM_2",
                                Synonyms = new List<string>() { "Item 2", "Second item" },
                                Item = new EntryDisplay()
                                {
                                    Title = "Item #2",
                                    Description = "Description of Item #2",
                                    Image = new Image()
                                    {
                                        Url = "https://developers.google.com/assistant/assistant_96.png",
                                        Height = 0,
                                        Width = 0,
                                        Alt = "Google Assistant logo"
                                    }
                                }
                            },
                            new CollectionItem()
                            {
                                Key = "ITEM_3",
                                Synonyms = new List<string>() { "Item 3", "Third item" },
                                Item = new EntryDisplay()
                                {
                                    Title = "Item #3",
                                    Description = "Description of Item #3",
                                    Image = new Image()
                                    {
                                        Url = "https://developers.google.com/assistant/assistant_96.png",
                                        Height = 0,
                                        Width = 0,
                                        Alt = "Google Assistant logo"
                                    }
                                }
                            },
                            new CollectionItem()
                            {
                                Key = "ITEM_4",
                                Synonyms = new List<string>() { "Item 4", "Fourth item" },
                                Item = new EntryDisplay()
                                {
                                    Title = "Item #4",
                                    Description = "Description of Item #4",
                                    Image = new Image()
                                    {
                                        Url = "https://developers.google.com/assistant/assistant_96.png",
                                        Height = 0,
                                        Width = 0,
                                        Alt = "Google Assistant logo"
                                    }
                                }
                            },
                        }
                    };
                    activityWithCollectionAttachment.Attachments.Add(collection.ToAttachment());
                    await turnContext.SendActivityAsync(activityWithCollectionAttachment, cancellationToken);
                    break;

                case "table":
                    var activityWithTableCardAttachment = MessageFactory.Text($"Ok, I included a table.");
                    var table = ContentItemFactory.CreateTable(
                        new List<TableColumn>()
                        {
                            new TableColumn() { Header = "Column 1" },
                            new TableColumn() { Header = "Column 2" }
                        },
                        new List<TableRow>()
                        {
                            new TableRow() {
                                Cells = new List<TableCell>
                                {
                                    new TableCell { Text = "Row 1, Item 1" },
                                    new TableCell { Text = "Row 1, Item 2" }
                                }
                            },
                            new TableRow() {
                                Cells = new List<TableCell>
                                {
                                    new TableCell { Text = "Row 2, Item 1" },
                                    new TableCell { Text = "Row 2, Item 2" }
                                }
                            }
                        },
                        "Table Card Title",
                        "Table card subtitle",
                        new Link { Name = "Microsoft", Open = new OpenUrl() { Url = "https://www.microsoft.com" } });
                    activityWithTableCardAttachment.Attachments.Add(table.ToAttachment());
                    await turnContext.SendActivityAsync(activityWithTableCardAttachment, cancellationToken);
                    break;
            }
        }

        protected override async Task OnEventActivityAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(MessageFactory.Text($"Event Received. Name: {turnContext.Activity.Name}, Value: {turnContext.Activity.Value}"), cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Hello and welcome!"), cancellationToken);
                }
            }
        }
    }
}
