using BrilliantSee.Models.Enums;
using BrilliantSee.ViewModels;
using CommunityToolkit.Maui.Core.Platform;

namespace BrilliantSee.Views;

public partial class SearchPage : ContentPage
{
    private readonly SearchViewModel _vm;

    /// <summary>
    /// ��ť�ı���Ӧ�����
    /// </summary>
    private Dictionary<string, SourceCategory> Categories;

    /// <summary>
    /// ����Ӧ�İ�ť
    /// </summary>
    private Dictionary<SourceCategory, Button> Buttons;

    public SearchPage(SearchViewModel vm)
    {
        _vm = vm;
        this.BindingContext = _vm;
        InitializeComponent();
        Categories = new Dictionary<string, SourceCategory>()
        {
            { "ȫ��", SourceCategory.All },
            { "С˵", SourceCategory.Novel },
            { "����", SourceCategory.Comic },
            { "����", SourceCategory.Video }
        };
        Buttons = new Dictionary<SourceCategory, Button>()
        {
            { SourceCategory.All, all },
            { SourceCategory.Novel, novels },
            { SourceCategory.Comic, comics },
            { SourceCategory.Video, videos }
        };
        this.Loaded += SearchPage_Loaded;
    }

    /// <summary>
    /// ҳ�����ʱ������ȡ����
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void SearchPage_Loaded(object? sender, EventArgs e)
    {
        this.input.Focus();
        await Task.Delay(250);
        this.input.Focus();
        //this.audio.IsVisible = await _vm._db.GetAudioStatus();
    }

    /// <summary>
    /// �������
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void HideKeyboard(object sender, TappedEventArgs e)
    {
#if ANDROID
        if (input.IsSoftKeyboardShowing())
        {
            _ = input.HideKeyboardAsync(CancellationToken.None);
        }
#endif
    }

    /// <summary>
    /// ���������¼���ʵ�ַ��ض�����ť����ʾ�������Լ����״������ظ���
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void CollectionView_Scrolled(object sender, ItemsViewScrolledEventArgs e)
    {
        this.floatButton.IsVisible = e.FirstVisibleItemIndex == 0 ? false : true;
        if (e.LastVisibleItemIndex == _vm.CurrentObjsCount - 1 && _vm.IsGettingResult == false && _vm.CurrentObjsCount != 0)
        {
            await _vm.GetMoreAsync();
        }
    }

    /// <summary>
    /// ���ض���
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BacktoTop(object sender, EventArgs e)
    {
        this.comicList.ScrollTo(0, position: ScrollToPosition.Start);
    }

    /// <summary>
    /// ��ť���Ч��
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="type"></param>
    private async Task ButtonTapped(object sender, Type type)
    {
        View obj = (View)sender;
        await obj!.ScaleTo(1.15, 100);
        await obj!.ScaleTo(1, 100);
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        _ = ButtonTapped(sender, sender.GetType());
    }

    private void floatButton_Pressed(object sender, EventArgs e)
    {
        _ = ButtonTapped(sender, sender.GetType());
    }

    /// <summary>
    /// �л���𣬸���UI,ˢ���б�
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_Clicked(object sender, EventArgs e)
    {
        var button = sender! as Button;
        var selectedCategory = Categories[button!.Text];

        if (selectedCategory == _vm.CurrentCategory) return;
        Buttons[_vm.CurrentCategory].TextColor = Color.FromArgb("#212121");
        button.TextColor = Color.FromArgb("#512BD4");
        _ = ButtonTapped(sender, typeof(Button));

        _vm.ChangeCurrentCategory(selectedCategory);
        this.comicList.ItemsSource = _vm.GetObjsOnDisplay();
    }
}