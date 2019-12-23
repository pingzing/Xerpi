using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xerpi.Models.API;
using Xerpi.Services;

namespace Xerpi.ViewModels
{
    public class SettingsViewModel : BasePageViewModel
    {
        public override string Url => "settings";

        private readonly ISettingsService _settingsService;
        private readonly IDerpiNetworkService _networkService;

        private List<SettingsFilterViewModel> _filters = new List<SettingsFilterViewModel>();
        public List<SettingsFilterViewModel> Filters
        {
            get => _filters;
            set => Set(ref _filters, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => Set(ref _isLoading, value);
        }

        public SettingsViewModel(ISettingsService settingsService,
            IDerpiNetworkService networkService)
        {
            _settingsService = settingsService;
            _networkService = networkService;

            Title = "Settings";

            IsLoading = true;
        }

        public override async Task NavigatedTo()
        {
            var defaultFilters = await _networkService.GetDefaultFilters();
            if (defaultFilters != null)
            {
                Filters = defaultFilters
                    .Select(x => new SettingsFilterViewModel(x, _settingsService))
                    .ToList();
            }

            IsLoading = false;
        }
    }
}
