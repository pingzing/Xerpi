using Microsoft.Extensions.DependencyInjection;
using Rg.Plugins.Popup.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xerpi.Models;
using Xerpi.Services;
using Xerpi.Views.Popups;

namespace Xerpi.Controls
{
    public partial class TitleSearchBar : ContentView
    {
        private const string CollapsedStateName = "Collapsed";
        private const string SearchingStateName = "Searching";

        private readonly IPopupService _popupService;

        private State _currentState;

        public event EventHandler<SearchSortOptions> SearchSortOptionsChanged;

        public static BindableProperty TitleProperty = BindableProperty.Create(
            nameof(Title),
            typeof(string),
            typeof(TitleSearchBar));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static BindableProperty QueryProperty = BindableProperty.Create(
            nameof(Query),
            typeof(string),
            typeof(TitleSearchBar),
            defaultValue: null,
            defaultBindingMode: BindingMode.TwoWay);

        public string Query
        {
            get => (string)GetValue(QueryProperty);
            set => SetValue(QueryProperty, value);
        }

        public static BindableProperty SearchCommandProperty = BindableProperty.Create(
            nameof(SearchCommand),
            typeof(ICommand),
            typeof(TitleSearchBar));

        public ICommand SearchCommand
        {
            get => (ICommand)GetValue(SearchCommandProperty);
            set => SetValue(SearchCommandProperty, value);
        }

        public static BindableProperty SearchCommandParameterProperty = BindableProperty.Create(
            nameof(SearchCommandParameter),
            typeof(object),
            typeof(TitleSearchBar));

        public object SearchCommandParameter
        {
            get => GetValue(SearchCommandParameterProperty);
            set => SetValue(SearchCommandParameterProperty, value);
        }

        public TitleSearchBar()
        {
            InitializeComponent();
            _currentState = State.Collapsed;
            _popupService = Startup.ServiceProvider.GetRequiredService<IPopupService>();
        }

        private void OpenSearchButton_Clicked(object sender, EventArgs e)
        {
            UpdateState(State.Searching);
        }

        private void UpdateState(State newState)
        {
            switch (newState)
            {
                case State.Collapsed:
                    VisualStateManager.GoToState(RootGrid, CollapsedStateName);
                    SearchBox.Unfocused -= SearchBox_Unfocused;
                    break;
                case State.Searching:
                    VisualStateManager.GoToState(RootGrid, SearchingStateName);
                    SearchBox.Focus();
                    SearchBox.CursorPosition = 0;
                    SearchBox.SelectionLength = SearchBox.Text.Length;
                    SearchBox.Unfocused += SearchBox_Unfocused;
                    break;
            }
            _currentState = newState;
        }

        private void SearchBox_Unfocused(object sender, FocusEventArgs e)
        {
            UpdateState(State.Collapsed);
        }

        private void SearchBox_Completed(object sender, EventArgs e)
        {
            TriggerSearch();
        }

        private void ClearEntryButton_Clicked(object sender, EventArgs e)
        {
            SearchBox.Text = "";
            // We lose focus when the user taps this button.
            // By the time this handler fires, it's too late to suppress, so we have to just undo it.
            // Proper solution: Custom Entry renderer that incorporates a clear button
            UpdateState(State.Searching);
        }

        private void SearchBar_SearchButtonPressed(object sender, EventArgs e)
        {
            TriggerSearch();
        }

        private void TriggerSearch()
        {
            var command = SearchCommand;
            var commandParameter = SearchCommandParameter;
            if (command != null && command.CanExecute(commandParameter))
            {
                command.Execute(commandParameter);
            }
        }

        private async void SortFilterButton_Clicked(object sender, EventArgs e)
        {
            SearchSortOptions? sortOptions = await _popupService.ShowPopup(Startup.ServiceProvider.GetRequiredService<SortFilterPopupViewModel>());
            if (sortOptions != null)
            {
                SearchSortOptionsChanged?.Invoke(this, sortOptions);
            }
        }

        private enum State
        {
            Collapsed,
            Searching
        }
    }
}