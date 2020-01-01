using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Xerpi.Controls
{
    public partial class TitleSearchBar : ContentView
    {
        private State _currentState;

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
        }

        private void ToggleSearchButton_Clicked(object sender, EventArgs e)
        {
            UpdateState(State.Searching);
        }

        private void UpdateState(State newState)
        {
            switch (newState)
            {
                case State.Collapsed:
                    TitleLabel.IsVisible = true;
                    ToggleSearchButton.IsVisible = true;
                    SearchBar.IsVisible = false;
                    SearchBar.Unfocused -= SearchBar_Unfocused;
                    break;
                case State.Searching:
                    TitleLabel.IsVisible = false;
                    ToggleSearchButton.IsVisible = false;
                    SearchBar.IsVisible = true;
                    SearchBar.Focus();
                    SearchBar.Unfocused += SearchBar_Unfocused;
                    break;
            }
            _currentState = newState;
        }

        private void SearchBar_Unfocused(object sender, FocusEventArgs e)
        {
            UpdateState(State.Collapsed);
        }

        private void SearchBar_SearchButtonPressed(object sender, EventArgs e)
        {
            var command = SearchCommand;
            var commandParameter = SearchCommandParameter;
            if (command != null && command.CanExecute(commandParameter))
            {
                command.Execute(commandParameter);
            }
        }

        private enum State
        {
            Collapsed,
            Searching
        }
    }
}