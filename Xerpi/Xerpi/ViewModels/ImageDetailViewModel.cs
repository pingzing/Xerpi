using System;
using System.Threading.Tasks;
using Xerpi.Models;
using Xerpi.Models.API;

namespace Xerpi.ViewModels
{
    public class ImageDetailViewModel : BaseViewModel
    {
        public override string Url => "imagedetails";

        private ApiImage _backingImage;
        public ApiImage BackingImage
        {
            get => _backingImage;
            set => SetProperty(ref _backingImage, value);
        }

        public ImageDetailViewModel()
        {
        }

        public override Task NavigatedTo()
        {
            if (NavigationParameter is ApiImage image)
            {
                BackingImage = image;
            }

            return Task.CompletedTask;
        }

        public override Task NavigatedFrom()
        {
            return base.NavigatedFrom();
        }
    }
}
