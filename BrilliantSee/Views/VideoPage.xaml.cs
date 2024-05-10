using BrilliantSee.ViewModels;
using CommunityToolkit.Maui.Core.Handlers;
using CommunityToolkit.Maui.Views;

namespace BrilliantSee.Views;

public partial class VideoPage : ContentPage
{
    private readonly DetailViewModel _vm;

    public VideoPage(DetailViewModel vm)
    {
        _vm = vm;
        this.BindingContext = _vm;
        InitializeComponent();
#if ANDROID
        var activity = Platform.CurrentActivity ?? throw new InvalidOperationException("Android Activity can't be null.");
        activity.RequestedOrientation = Android.Content.PM.ScreenOrientation.User;
#endif
    }

    /// <summary>
    /// ҳ���˳��ǶϿ�mediaelement��Դ�����ڴ�й©
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ContentPage_Unloaded(object sender, EventArgs e)
    {
        media.Handler?.DisconnectHandler();
#if ANDROID
        var activity = Platform.CurrentActivity ?? throw new InvalidOperationException("Android Activity can't be null.");
        activity.RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;
#endif
    }

    /// <summary>
    /// ��ť���Ч��
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="type"></param>
    private async Task ButtonTapped(object sender, Type type)
    {
        View obj = type == typeof(Frame) ? (Frame)sender! : (Button)sender!;
        await obj!.ScaleTo(1.05, 100);
        await obj!.ScaleTo(1, 100);
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
        if (_vm.CurrentRoute != SelectedRoute)
        {
            _vm.CurrentRoute = SelectedRoute;
            _ = ButtonTapped(sender, typeof(Button));
            _vm.SetItemsOnDisplay();
        }
    }

    protected override bool OnBackButtonPressed()
    {
        if (media.Height == 0)
        {
            _vm._ms.WriteMessage("�����˳�ȫ���ٷ���");
            return true;
        }
        else
        {
            return base.OnBackButtonPressed();
        }
    }
}