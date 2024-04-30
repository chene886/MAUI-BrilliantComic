using BrilliantSee.ViewModels;

namespace BrilliantSee.Views;

public partial class BrowsePage : ContentPage
{
    private readonly BrowseViewModel _vm;

    public BrowsePage(BrowseViewModel vm)
    {
        _vm = vm;
        this.BindingContext = _vm;
        InitializeComponent();
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        if (button!.Text == "���������һ��")
        {
            this.list.Command.Execute("Next");
            if (_vm.Images.Count > 0) this.listView.ScrollTo(_vm.Images.First(), ScrollToPosition.Start, false);
        }
    }

    /// <summary>
    /// �˳�ҳ��ʱ��ȡ�����ص�ǰ�½�ͼƬ
    /// </summary>
    protected override void OnDisappearing()
    {
        _vm.CancelLoadCurrentChapterImage();

        base.OnDisappearing();
    }
}