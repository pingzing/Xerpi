namespace Xerpi.ViewModels
{
    public class AboutViewModel : BasePageViewModel
    {
        public override string Url => "about";

        public AboutViewModel()
        {
            Title = "About";
        }
    }
}