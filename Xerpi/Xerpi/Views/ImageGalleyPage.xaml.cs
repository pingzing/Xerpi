using Xerpi.ViewModels;

namespace Xerpi.Views
{
    public partial class ImageGalleryPage : NavigablePage
    {
        private ImageGalleryViewModel ViewModel => (ImageGalleryViewModel)_viewModel;

        public ImageGalleryPage() : base(typeof(ImageGalleryViewModel))
        {
            InitializeComponent();
            BindingContext = ViewModel;
        }

        private void TapGestureRecognizer_Tapped(object sender, System.EventArgs e)
        {
            if (BottomPanel.IsOpen)
            {
                BottomPanel.IsOpen = false;
            }
            else
            {
                if (ViewModel?.FullSizeButtonCommand?.CanExecute(null) == true)
                {
                    ViewModel.FullSizeButtonCommand.Execute(null);
                }
            }
        }
    }
}