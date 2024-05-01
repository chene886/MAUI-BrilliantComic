using BrilliantSee.ViewModels;

namespace BrilliantSee.Views;

public partial class SettingPage : ContentPage
{
    private readonly SettingViewModel _vm;

    public SettingPage(SettingViewModel vm)
    {
        _vm = vm;
        this.BindingContext = _vm;
        InitializeComponent();
    }

    //���ݰ�ť����ִ�в�ͬ����(���Ʒ�������/������/�����ʼ�/���ô������ݣ�����UI)
    private async void Button_Clicked(object sender, EventArgs e)
    {
        var obj = sender! as Button;
        var shadow = obj!.Shadow;
        obj.Shadow = new Shadow()
        {
            Offset = new Point(0, 0),
            Opacity = (float)0.3,
            Radius = 14,
        };
        await obj.ScaleTo(1.05, 100);
        await obj.ScaleTo(1, 100);
        obj.Shadow = shadow;
        if (obj.Text.Contains("ȥ")) await _vm.GoToAsync(obj.Text);
        else
        {
            TapGestureRecognizer_Tapped(sender, new TappedEventArgs(e));
            if (obj.Text == "ȷ��") return;
            await this.message.ScrollToAsync(0, 0, false);
            await _vm.SetContentAsync(obj.Text);
        }
    }

    //�رջ������ں�����
    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        this.cover.IsVisible = !this.cover.IsVisible;
        this.window.IsVisible = !this.window.IsVisible;
    }
}