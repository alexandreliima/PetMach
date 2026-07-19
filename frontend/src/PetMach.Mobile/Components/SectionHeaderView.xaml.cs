namespace PetMach.Mobile.Components;

public partial class SectionHeaderView : ContentView
{
    public static readonly BindableProperty EyebrowProperty = BindableProperty.Create(
        nameof(Eyebrow),
        typeof(string),
        typeof(SectionHeaderView),
        string.Empty);

    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title),
        typeof(string),
        typeof(SectionHeaderView),
        string.Empty);

    public static readonly BindableProperty SubtitleProperty = BindableProperty.Create(
        nameof(Subtitle),
        typeof(string),
        typeof(SectionHeaderView),
        string.Empty);

    public SectionHeaderView()
    {
        InitializeComponent();
    }

    public string Eyebrow
    {
        get => (string)GetValue(EyebrowProperty);
        set => SetValue(EyebrowProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Subtitle
    {
        get => (string)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }
}
