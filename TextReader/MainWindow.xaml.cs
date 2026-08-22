using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using TextReader.ViewModel.ViewModel;

namespace TextReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainViewModel();
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (DataContext is not MainViewModel viewModel)
                return;

            if (!viewModel.IsNavigable)
                return;

            int lineDelta = e.Delta > 0 ? -20 : 20;
            ScrollTextLines(lineDelta, viewModel);
            e.Handled = true;
        }

        private void MainWindow_CleanTempFiles(object sender, CancelEventArgs e) 
        {
            if (DataContext is not MainViewModel viewModel)
                return;

            viewModel.DeleteTempFiles();
        }

        private void MainWindow_KeybindAction(object sender, KeyEventArgs e) 
        {
            if (DataContext is not MainViewModel viewModel)
                return;

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F)
            {
                viewModel.IsSearchVisible = true;
                SearchTxtBox.Focus();
                SearchTxtBox.SelectAll();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Escape && viewModel.IsSearchVisible)
            {
                viewModel.IsSearchVisible = false;
                e.Handled = true;
                return;
            }

            if (e.Key == Key.F3)
            {
                if (Keyboard.Modifiers == ModifierKeys.Shift)
                {
                    if (viewModel.FindPreviousCommand.CanExecute(null))
                        viewModel.FindPreviousCommand.Execute(null);
                }
                else
                {
                    if (viewModel.FindNextCommand.CanExecute(null))
                        viewModel.FindNextCommand.Execute(null);
                }

                e.Handled = true;
                return;
            }

            if (viewModel.IsNavigable)
            {
                switch (e.Key)
                {

                    case Key.PageUp:
                        ScrollTextLines(-100, viewModel);
                        e.Handled = true;
                        break;
                    case Key.PageDown:
                        ScrollTextLines(100, viewModel);
                        e.Handled = true;
                        break;
                    case Key.End:
                        viewModel.CurrentStartLine = viewModel.MaxStartLine;
                        TextScrollViewer.ScrollToTop();
                        e.Handled = true;
                        break;
                    case Key.Home:
                        viewModel.CurrentStartLine = 0;
                        TextScrollViewer.ScrollToTop();
                        e.Handled = true;
                        break;
                    case Key.Up:
                        ScrollTextLines(-1, viewModel);
                        e.Handled = true;
                        break;

                    case Key.Down:
                        ScrollTextLines(1, viewModel);
                        e.Handled = true;
                        break;
                }
            }
        }

        private void SearchTxtBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            if (DataContext is not MainViewModel viewModel)
                return;

            if (viewModel.FindNextCommand.CanExecute(null))
                viewModel.FindNextCommand.Execute(null);

            e.Handled = true;
        }

        private void DocumentScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (DataContext is not MainViewModel viewModel)
                return;

            if (!viewModel.IsNavigable)
                return;

            long targetLine = (long)Math.Round(e.NewValue);

            if (viewModel.CurrentStartLine == targetLine)
                return;

            viewModel.CurrentStartLine = targetLine;
            TextScrollViewer.ScrollToTop();
        }

        private void ScrollTextLines(int lineDelta, MainViewModel viewModel)
        {
            if (viewModel.IsIndexReady)
            {
                viewModel.CurrentStartLine += lineDelta;
                TextScrollViewer.ScrollToTop();
            }
        }
    }
}
