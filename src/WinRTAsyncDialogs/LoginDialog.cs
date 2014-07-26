using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;

using Windows.Foundation;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace VisuallyLocated.UI.Popups
{
    public class LoginDialog
    {
        private TaskCompletionSource<LoginResult> _taskCompletionSource;
        private PasswordBox _passwordTextBox;
        private TextBlock _titleTextBlock;
        private TextBlock _userNameTextBlock;
        private TextBox _userTextBox;
        private Popup _popup;

        public LoginDialog(string title)
        {
            if (string.IsNullOrEmpty(title) == false)
            {
                Title = title;
            }
            UserNameTitle = "Name";
            LoginMessage = "Enter the information below to connect to your account.";
            HeaderBrush = new SolidColorBrush(Colors.Blue);
        }

        public LoginDialog(string title, string loginMessage)
            : this(title)
        {
            if (string.IsNullOrEmpty(loginMessage) == false)
            {
                LoginMessage = loginMessage;
            }
        }

        public string LoginMessage { get; set; }

        public Brush HeaderBrush { get; set; }

        public string Title { get; set; }

        public string UserNameTitle { get; set; }

        public IAsyncOperation<LoginResult> ShowAsync()
        {
            _popup = new Popup { Child = CreateLogin() };
            if (_popup.Child != null)
            {
                SubscribeEvents();
                _popup.IsOpen = true;
            }
            return AsyncInfo.Run(WaitForInput);
        }

        private Task<LoginResult> WaitForInput(CancellationToken token)
        {
            _taskCompletionSource = new TaskCompletionSource<LoginResult>();

            token.Register(OnCanceled);

            return _taskCompletionSource.Task;
        }

        private Grid CreateLogin()
        {
            var content = Window.Current.Content as FrameworkElement;
            if (content == null)
            {
                // The dialog is being shown before content has been created for the window
                Window.Current.Activated += OnWindowActivated;
                return null;
            }

            Style basicTextStyle = Application.Current.Resources["BaseTextBlockStyle"] as Style;
            Style subHeaderTextStyle = Application.Current.Resources["SubheaderTextBlockStyle"] as Style;

            double width = Window.Current.Bounds.Width;
            double height = Window.Current.Bounds.Height;
            var rootPanel = new Grid { Width = width, Height = height };
            var overlay = new Grid { Background = new SolidColorBrush(Colors.Black), Opacity = 0.2D };
            rootPanel.Children.Add(overlay);

            var dialog = new Grid { VerticalAlignment = VerticalAlignment.Center, RequestedTheme = ElementTheme.Light};
            dialog.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            dialog.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            dialog.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            dialog.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(410, GridUnitType.Star) });
            dialog.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
            dialog.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(500, GridUnitType.Star) });
            rootPanel.Children.Add(dialog);

            var titleBorder = new Border { Background = HeaderBrush, Height = 80D };
            Grid.SetColumnSpan(titleBorder, 3);
            dialog.Children.Add(titleBorder);

            _titleTextBlock = new TextBlock();
            _titleTextBlock.Text = Title;
            _titleTextBlock.Style = subHeaderTextStyle;
            _titleTextBlock.Margin = new Thickness(0, 0, 0, 20);
            _titleTextBlock.VerticalAlignment = VerticalAlignment.Bottom;
            Grid.SetColumn(_titleTextBlock, 1);
            dialog.Children.Add(_titleTextBlock);

            var infoBorder = new Border { Background = new SolidColorBrush(Colors.White) };
            Grid.SetRow(infoBorder, 1);
            Grid.SetColumnSpan(infoBorder, 3);
            Grid.SetRowSpan(infoBorder, 2);
            dialog.Children.Add(infoBorder);

            var grid = new Grid { MinHeight = 220 };
            Grid.SetRow(grid, 1);
            Grid.SetColumn(grid, 1);
            dialog.Children.Add(grid);

            StackPanel informationPanel = new StackPanel();
            informationPanel.Margin = new Thickness(0, 20, 0, 30);
            informationPanel.Width = 456D;
            Grid.SetColumn(informationPanel, 1);
            Grid.SetRow(informationPanel, 1);

            var descriptionTextBlock = new TextBlock();
            descriptionTextBlock.Text = LoginMessage;
            descriptionTextBlock.Style = basicTextStyle;
            descriptionTextBlock.Width = 456D;
            informationPanel.Children.Add(descriptionTextBlock);

            _userNameTextBlock = new TextBlock();
            _userNameTextBlock.Text = UserNameTitle;
            _userNameTextBlock.Style = basicTextStyle;
            _userNameTextBlock.Margin = new Thickness(0, 20, 0, 4);
            informationPanel.Children.Add(_userNameTextBlock);
            _userTextBox = new TextBox();
            _userTextBox.BorderBrush = new SolidColorBrush(Colors.Black);
            _userTextBox.BorderThickness = new Thickness(1);
            informationPanel.Children.Add(_userTextBox);

            TextBlock passwordTextBlock = new TextBlock();
            passwordTextBlock.Text = "Pasword";
            passwordTextBlock.Style = basicTextStyle;
            passwordTextBlock.FontSize = 16D;
            passwordTextBlock.Margin = new Thickness(0, 16, 0, 0);
            informationPanel.Children.Add(passwordTextBlock);
            _passwordTextBox = new PasswordBox();
            _passwordTextBox.BorderBrush = new SolidColorBrush(Colors.Black);
            _passwordTextBox.BorderThickness = new Thickness(1);
            _passwordTextBox.KeyUp += PasswordTextBoxOnKeyUp;
            informationPanel.Children.Add(_passwordTextBox);

            grid.Children.Add(informationPanel);

            Button connectButton = new Button();
            connectButton.BorderThickness = new Thickness();
            connectButton.Padding = new Thickness(10, 5, 10, 5);
            // TODO: Fill with user input
            connectButton.Content = "Connect";
            connectButton.HorizontalAlignment = HorizontalAlignment.Right;
            connectButton.Background = HeaderBrush;
            connectButton.Margin = new Thickness(0, 0, 0, 20);
            connectButton.MinWidth = 90D;
            connectButton.Click += (okSender, okArgs) => OnCompleted();
            Grid.SetColumn(connectButton, 1);
            Grid.SetRow(connectButton, 2);
            dialog.Children.Add(connectButton);

            Button cancelButton = new Button();
            cancelButton.BorderThickness = new Thickness();
            cancelButton.Padding = new Thickness(10, 5, 10, 5);
            cancelButton.Content = "Cancel";
            cancelButton.HorizontalAlignment = HorizontalAlignment.Left;
            cancelButton.Background = new SolidColorBrush(Color.FromArgb(255, 165, 165, 165));
            cancelButton.Margin = new Thickness(20, 0, 0, 20);
            cancelButton.MinWidth = 90D;
            cancelButton.Click += (cancelSender, cancelArgs) => OnCanceled();
            Grid.SetColumn(cancelButton, 2);
            Grid.SetRow(cancelButton, 2);
            dialog.Children.Add(cancelButton);
            return rootPanel;
        }

        private void PasswordTextBoxOnKeyUp(object sender, KeyRoutedEventArgs keyRoutedEventArgs)
        {
            if (keyRoutedEventArgs.Key == VirtualKey.Enter)
            {
                OnCompleted();
            }
        }

        // adjust for different view states
        private void OnWindowSizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            if (_popup.IsOpen == false) return;

            var child = _popup.Child as FrameworkElement;
            if (child == null) return;

            child.Width = e.Size.Width;
            child.Height = e.Size.Height;
        }

        // Adjust the name/password textboxes for the virtual keyuboard
        private void OnInputShowing(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            var child = _popup.Child as FrameworkElement;
            if (child == null) return;

            var transform = _passwordTextBox.TransformToVisual(child);
            var topLeft = transform.TransformPoint(new Point(0, 0));

            // Need to be able to view the entire textblock (plus a little more)
            var buffer = _passwordTextBox.ActualHeight + 10;
            if ((topLeft.Y + buffer) > sender.OccludedRect.Top)
            {
                var margin = topLeft.Y - sender.OccludedRect.Top;
                margin += buffer;
                child.Margin = new Thickness(0, -margin, 0, 0);
            }
        }

        private void OnInputHiding(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            var child = _popup.Child as FrameworkElement;
            if (child == null) return;

            child.Margin = new Thickness(0);
        }

        private void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Escape)
            {
                OnCanceled();
            }
        }

        private void OnWindowActivated(object sender, WindowActivatedEventArgs windowActivatedEventArgs)
        {
            Window.Current.Activated -= OnWindowActivated;
            SubscribeEvents();
            _popup.Child = CreateLogin();
            _popup.IsOpen = true;
        }

        private void OnCompleted()
        {
            UnsubscribeEvents();

            var result = new LoginResult();
            result.Name = _userTextBox.Text;
            result.Password = _passwordTextBox.Password;

            _popup.IsOpen = false;
            _taskCompletionSource.SetResult(result);
        }

        private void OnCanceled()
        {
            UnsubscribeEvents();
            // null to indicate dialog was canceled
            LoginResult result = null;

            _popup.IsOpen = false;
            _taskCompletionSource.SetResult(result);
        }

        private void SubscribeEvents()
        {
            Window.Current.SizeChanged += OnWindowSizeChanged;
            Window.Current.Content.KeyDown += OnKeyDown;

            var input = InputPane.GetForCurrentView();
            input.Showing += OnInputShowing;
            input.Hiding += OnInputHiding;
        }

        private void UnsubscribeEvents()
        {
            Window.Current.SizeChanged -= OnWindowSizeChanged;
            Window.Current.Content.KeyDown -= OnKeyDown;

            var input = InputPane.GetForCurrentView();
            input.Showing -= OnInputShowing;
            input.Hiding -= OnInputHiding;
        }
    }

    public class LoginResult
    {
        public string Name { get; set; }
        public string Password { get; set; }
    }
}
