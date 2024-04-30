using BrilliantSee.ViewModels;
using CommunityToolkit.Maui.Views;

namespace BrilliantSee.Views;

public partial class VideoPage : ContentPage
{
    private readonly DetailViewModel _vm;

    /// <summary>
    /// ��·ѡ��ť
    /// </summary>
    private readonly Dictionary<string, Button> Buttons;

    /// <summary>
    /// ��ǰ��·
    /// </summary>
    private string CurrentRoute { get; set; } = "��·һ";

    public VideoPage(DetailViewModel vm)
    {
        _vm = vm;
        this.BindingContext = _vm;
        InitializeComponent();
        Buttons = new Dictionary<string, Button>()
        {
            { "��·һ", route1 },
            { "��·��", route2 },
            { "��·��", route3 },
            { "��·��", route4 }
        };
    }

    /// <summary>
    /// ҳ���˳��ǶϿ�mediaelement��Դ�����ڴ�й©
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ContentPage_Unloaded(object sender, EventArgs e)
    {
        media.Handler?.DisconnectHandler();
    }

    /// <summary>
    /// ��ť���Ч��
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="type"></param>
    private async Task ButtonTapped(object sender, Type type)
    {
        View obj = type == typeof(Frame) ? (Frame)sender! : (Button)sender!;
        var shadow = obj!.Shadow;
        obj!.Shadow = new Shadow()
        {
            Offset = new Point(0, 0),
            Opacity = (float)0.3,
            Radius = 14,
        };
        await obj!.ScaleTo(1.05, 100);
        await obj!.ScaleTo(1, 100);
        obj!.Shadow = shadow;
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        _ = ButtonTapped(sender, typeof(Frame));
    }

    //�л���·������UI
    private void Button_Clicked(object sender, EventArgs e)
    {
        var btn = sender! as Button;
        var SelectedRoute = btn!.Text;
        if (CurrentRoute != SelectedRoute)
        {
            Buttons[CurrentRoute].TextColor = Color.FromArgb("#000000");
            CurrentRoute = SelectedRoute;
            btn.TextColor = Color.FromArgb("#512BD4");
            _ = ButtonTapped(sender, typeof(Button));
            _vm.SetItemsOnDisplay(CurrentRoute);
        }
    }
}