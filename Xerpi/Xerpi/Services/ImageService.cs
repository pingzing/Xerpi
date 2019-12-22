using DynamicData;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xerpi.Models.API;

namespace Xerpi.Services
{
    public interface IImageService
    {
        IObservableCache<ApiImage, uint> CurrentImages { get; }
        Task UpdateFrontPage(uint page = 1, uint itemsPerPage = 15);
    }

    public class ImageService : IImageService
    {
        private readonly IDerpiNetworkService _networkService;

        private SourceCache<ApiImage, uint> _currentImages = new SourceCache<ApiImage, uint>(x => x.Id);
        public IObservableCache<ApiImage, uint> CurrentImages { get; private set; }

        public ImageService(IDerpiNetworkService networkService)
        {
            CurrentImages = _currentImages.AsObservableCache();
            _networkService = networkService;
        }

        public async Task UpdateFrontPage(uint page = 1, uint itemsPerPage = 15)
        {
            var images = await _networkService.GetImages(page, itemsPerPage);
            if (images == null)
            {
                return;
            }

            _currentImages.Edit(x =>
            {
                foreach (var image in images.Images)
                {
                    x.AddOrUpdate(image);
                }
            });
        }
    }
}
