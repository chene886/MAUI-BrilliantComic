using BrilliantSee.ViewModels;

namespace BrilliantSee.Views;

public partial class NovelPage : ContentPage
{
    private readonly BrowseViewModel _vm;

    public NovelPage(BrowseViewModel vm)
    {
        _vm = vm;
        this.BindingContext = _vm;
        InitializeComponent();
        _vm.ScrollToTop += OnScrollToTopAsync;
    }

    /// <summary>
    /// ����������
    /// </summary>
    private async void OnScrollToTopAsync()
    {
        await this.content.ScrollToAsync(0, 0, false);
    }
}