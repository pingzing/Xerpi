using FFImageLoading.Forms;
using System;
using Xamarin.Forms;
using Xerpi.Models.API;

namespace Xerpi.Extensions
{
    public class ImageCacheKeyFactory : ICacheKeyFactory
    {
        public ImageType ImageType { get; set; } = ImageType.Thumbnail;

        public string GetKey(ImageSource imageSource, object bindingContext)
        {
            if (bindingContext is ApiImage image)
            {
                string suffix = ImageType switch
                {
                    ImageType.Thumbnail => image.Representations.Thumb,
                    ImageType.Full => image.Representations.Full,
                    _ => image.Representations.Thumb,
                };
                return $"{image.Id}+{suffix}";
            }
            else if (imageSource is UriImageSource uriSource)
            {
                return uriSource.Uri.ToString();
            }

            throw new NotImplementedException("This factory must be used for URI-based images or ApiImage-based images.");
        }
    }

    public enum ImageType
    {
        Thumbnail,
        Full
    }
}
