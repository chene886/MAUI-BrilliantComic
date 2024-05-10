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

    private Button[] Buttons;

    private int CurrentButtonIndex = 0;
    private SwipeDirection _direction { get; set; }
    private double _offset { get; set; } = 0;

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
        Buttons = new Button[] { all, novels, comics, videos };
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

    private void SwipeView_SwipeChanging(object sender, SwipeChangingEventArgs e)
    {
        _direction = e.SwipeDirection;
        _offset = e.Offset;
    }

    private void SwipeView_SwipeEnded(object sender, SwipeEndedEventArgs e)
    {
        var value = _direction == SwipeDirection.Left ? 1 : -1;
        if (Math.Abs(_offset) > 24)
        {
            swipeView.Close();
            var index = CurrentButtonIndex + value;
            if (index < 0 || index > 3)
            {
                return;
            }
            CurrentButtonIndex = index;
            Button_Clicked(Buttons[CurrentButtonIndex], e);
        }
    }

    private void ImageButton_Clicked(object sender, EventArgs e)
    {
        var imgbtn = sender! as ImageButton;
        var grid = imgbtn!.Parent as Grid;
        grid!.IsVisible = false;
    }

    private void DragGestureRecognizer_DragStarting(object sender, DragStartingEventArgs e)
    {
        e.Cancel = true;
        var drag = (DragGestureRecognizer)sender!;
        drag.FindByName<Grid>("buttons").IsVisible = true;
    }
}