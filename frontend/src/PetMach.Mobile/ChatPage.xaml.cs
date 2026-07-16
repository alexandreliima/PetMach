using PetMach.Mobile.Core.Features;

namespace PetMach.Mobile;

public partial class ChatPage : ContentPage, IQueryAttributable
{
    private readonly ChatViewModel viewModel;
    public ChatPage(ChatViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = this.viewModel = viewModel;
    }

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("conversationId", out object? value) && Guid.TryParse(value?.ToString(), out Guid id))
            await viewModel.InitializeAsync(id);
    }

    protected override async void OnDisappearing()
    {
        await viewModel.StopAsync();
        base.OnDisappearing();
    }
}
