namespace PetMach.Mobile.Components;

public enum StateViewKind
{
    Loading,
    Empty,
    Error,
    Success,
    Warning,
}

public partial class StateView : ContentView
{
    public static readonly BindableProperty KindProperty = BindableProperty.Create(
        nameof(Kind),
        typeof(StateViewKind),
        typeof(StateView),
        StateViewKind.Empty,
        propertyChanged: OnKindChanged);

    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title),
        typeof(string),
        typeof(StateView),
        string.Empty);

    public static readonly BindableProperty MessageProperty = BindableProperty.Create(
        nameof(Message),
        typeof(string),
        typeof(StateView),
        string.Empty);

    public StateView()
    {
        InitializeComponent();
        ApplyKind();
    }

    public StateViewKind Kind
    {
        get => (StateViewKind)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    private static void OnKindChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((StateView)bindable).ApplyKind();
    }

    private void ApplyKind()
    {
        StateIndicator.IsVisible = Kind == StateViewKind.Loading;
        StateIndicator.IsRunning = Kind == StateViewKind.Loading;
        StateIcon.IsVisible = Kind != StateViewKind.Loading;

        var (styleKey, icon, colorKey) = Kind switch
        {
            StateViewKind.Error => ("PetMachErrorStateStyle", "!", "PetMachError"),
            StateViewKind.Success => ("PetMachSuccessStateStyle", "✓", "PetMachSuccess"),
            StateViewKind.Warning => ("PetMachWarningStateStyle", "!", "PetMachWarning"),
            StateViewKind.Loading => ("PetMachEmptyStateStyle", string.Empty, "PetMachPrimary"),
            _ => ("PetMachEmptyStateStyle", "○", "PetMachSecondary"),
        };

        StateContainer.Style = (Style)Application.Current!.Resources[styleKey];
        StateIcon.Text = icon;
        StateIcon.TextColor = (Color)Application.Current.Resources[colorKey];
    }
}
