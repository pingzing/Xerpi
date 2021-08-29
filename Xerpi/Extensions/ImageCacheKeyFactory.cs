using FFImageLoading.Forms;
using System;
using Xamarin.Forms;
using Xerpi.Models.API;

namespace Xerpi.Extensions
{
    public class ImageCacheKeyFactory : ICacheKeyFactory
    {
        public static ICacheKeyFactory ThumbnailCacheFactory { get; } = new ImageCacheKeyFactory(ImageType.Thumbnail);
        public static ICacheKeyFactory MediumCacheFactory { get; } = new ImageCacheKeyFactory(ImageType.Medium);
        public static ICacheKeyFactory LargeCacheFactory { get; } = new ImageCacheKeyFactory(ImageType.Large);
        public static ICacheKeyFactory FullCacheFactory { get; } = new ImageCacheKeyFactory(ImageType.Full);

        public ImageType ImageType { get; set; } = ImageType.Thumbnail;

        public ImageCacheKeyFactory() { }

        public ImageCacheKeyFactory(ImageType imageType)
        {
            ImageType = imageType;
        }

        public string GetKey(ImageSource imageSource, object bindingContext)
        {
            if (bindingContext is ApiImage image)
            {
                string suffix = ImageType switch
                {
                    ImageType.ThumbTiny => image.Representations.ThumbTiny,
                    ImageType.ThumbSmall => image.Representations.ThumbSmall,
                    ImageType.Thumbnail => image.Representations.Thumb,
                    ImageType.Small => image.Representations.Small,
                    ImageType.Medium => image.Representations.Medium,
                    ImageType.Large => image.Representations.Large,
                    ImageType.Tall => image.Representations.Tall,
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
        ThumbTiny,
        ThumbSmall,
        Thumbnail,
        Small,
        Medium,
        Large,
        Tall,
        Full
    }
}
