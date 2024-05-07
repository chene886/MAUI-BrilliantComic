using BrilliantSee.Models.Enums;
using BrilliantSee.ViewModels;

namespace BrilliantSee.Views;

public partial class HistoryPage : ContentPage
{
    private readonly HistoryViewModel _vm;

    /// <summary>
    /// ��ť�ı���Ӧ�����
    /// </summary>
    private Dictionary<string, SourceCategory> Categories;

    public HistoryPage(HistoryViewModel vm)
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
    }

    /// <summary>
    /// ҳ�����ʱ������ʷ��¼������
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.OnLoadHistoryObjAsync();
        //if (_vm._ai.hasModel)
        //{
        //    _vm._ai.RemovePlugins();
        //    _vm._ai.ImportPlugins(new Services.Plugins.HistoryPlugins(_vm._db));
        //}
        //this.audio.IsVisible = await _vm._db.GetAudioStatus();
    }


    /// <summary>
    /// ��ť���Ч��
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="type"></param>
    private async Task ButtonTapped(object sender, Type type)
    {
        View obj = type == typeof(Frame) ? (Frame)sender! : (Button)sender!;
        await obj!.ScaleTo(1.15, 100);
        await obj!.ScaleTo(1, 100);
    }

    /// <summary>
    /// �����ʷ��¼��ʾ
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void CleanTapped(object sender, TappedEventArgs e)
    {
        _ = ButtonTapped(sender, typeof(Frame));
        bool answer = await DisplayAlert("�����ʷ��¼", "��ʷ��¼��պ��޷��ָ����Ƿ����?", "ȷ��", "ȡ��");
        if (answer)
        {
            await _vm.ClearHistoryObjsAsync();
        }
    }

    /// <summary>
    /// ��ת������ҳ��
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void JumpToSettingPage(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync("SettingPage");
    }

    /// <summary>
    /// �л���𣬼��ز�ͬ������ʷ��¼������UI
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Button_Clicked(object sender, EventArgs e)
    {
        var button = sender! as Button;
        var selectedCategory = Categories[button!.Text];

        if (selectedCategory == _vm.CurrentCategory) return;
        _vm.ChangeCurrentCategory(selectedCategory);
        _ = ButtonTapped(sender, typeof(Button));

        await _vm.OnLoadHistoryObjAsync();
    }
}