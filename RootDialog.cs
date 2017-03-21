using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.FormFlow.Advanced;
using Microsoft.Bot.Connector;

namespace FormFlow_BranchBasedOnResponse
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var orderStatusForm = new FormDialog<OrderStatusDialog>(new OrderStatusDialog(), OrderStatusDialog.BuildForm, FormOptions.PromptInStart);

            context.Call(orderStatusForm, OrderStatusFormCallback);
        }

        private async Task OrderStatusFormCallback(IDialogContext context, IAwaitable<OrderStatusDialog> result)
        {
            // TODO: Handle FormCanceledException

            var order = await result;

            await context.PostAsync($"Order Number: {order.OrderNumber}");

            await context.PostAsync($"Order Delivery Address: {order.DeliveryAddress}");

            context.Wait(this.MessageReceivedAsync);
        }
    }


    public enum OrderStatusLookupOptions
    {
        Address,
        OrderNumber
    }

    [Serializable]
    public class OrderStatusDialog
    {
        public OrderStatusLookupOptions? LookupOption;

        public int OrderNumber;

        public string DeliveryAddress;

        public static IForm<OrderStatusDialog> BuildForm()
        {
            return new FormBuilder<OrderStatusDialog>()
                .Message("In order to look up the status of a order, we will first need either the order number or your delivery address.")
                .Field(nameof(OrderStatusDialog.LookupOption))
                .Field(new FieldReflector<OrderStatusDialog>(nameof(OrderStatusDialog.OrderNumber))
                    .SetActive(state => state.LookupOption == OrderStatusLookupOptions.OrderNumber))
                .Field(new FieldReflector<OrderStatusDialog>(nameof(OrderStatusDialog.DeliveryAddress))
                    .SetActive(state => state.LookupOption == OrderStatusLookupOptions.Address))
                .Build();
        }
    }
}