using Microsoft.Win32;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Input;
using TextReader.MVVM.Utility;
using TextReader.ViewModel.Model;
using TextReader.ViewModel.Services;
using TextReader.ViewModel.Utility;

namespace TextReader.ViewModel.ViewModel
{
    public class MainViewModel : BaseViewModel
    {

        #region Variables
        private AsyncRelayCommand? _openFileCommand;
        private AsyncRelayCommand? _openUrlCommand;
        private AsyncRelayCommand? _generateRandomCommand;
        private AsyncRelayCommand? _saveAsCommand;
        private TextProvider? _textProvider;
        private AsyncRelayCommand? _searchCommand;
        private AsyncRelayCommand? _findNextCommand;
        private AsyncRelayCommand? _findPreviousCommand;

        private readonly List<string> _tempFiles = new();

        private int VisibleLineCount = 1000;
        private int ViewportLineCount = 100;
        private string _displayText = "";

        private long _currentStartLine = 0;
        private double _scrollBarPosition = 0;
        private long _totalLines = 0;

        private bool _isIndexReady;
        private bool _isBusy;
        private bool _isSearchVisible;


        private string _statusText = "";
        private string _urlText = "";
        private string _keyword = "";
        private string? _currentFilePath;
        private string? _lastSearchKeyword;
        private long _lastFoundLine = -1;
        private int _highlightLineIndex = -1;
        private int _highlightStartIndex = -1;
        private int _highlightLength = 0;
        #endregion

        #region Properties
        public bool IsIndexReady
        {
            get => _isIndexReady;
            set
            {

                if (_isIndexReady == value)
                    return;

                _isIndexReady = value;
                OnPropertyChanged();

                CommandsStateChanged();
                OnPropertyChanged(nameof(IsNavigable));
            }
        }

        public string StatusText
        {
            get => _statusText;
            set
            {
                _statusText = value;
                OnPropertyChanged();
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy == value)
                    return;

                _isBusy = value;
                OnPropertyChanged();

                CommandsStateChanged();
            }
        }

        public ICommand OpenFileCommand
        {
            get
            {
                return _openFileCommand ??=
                    new AsyncRelayCommand(
                        OpenFileAsync,
                        () => !IsBusy);
            }
        }

        public string DisplayText
        {
            get => _displayText;
            set
            {
                _displayText = value;
                OnPropertyChanged();
            }
        }

        public ICommand OpenUrlCommand
        {
            get
            {
                return _openUrlCommand ??=
                    new AsyncRelayCommand(
                        OpenUrlAsync,
                        () => !IsBusy);
            }
        }

        public ICommand GenerateRandomCommand
        {
            get
            {
                return _generateRandomCommand ??=
                    new AsyncRelayCommand(
                        GenerateRandomAsync,
                        () => !IsBusy);
            }
        }

        public ICommand SaveAsCommand
        {
            get
            {
                return _saveAsCommand ??=
                    new AsyncRelayCommand(
                        SaveAsAsync,
                        () => !IsBusy && _currentFilePath != null);


            }
        }

        public string Keyword
        {
            get { return _keyword; }
            set
            {
                _keyword = value;
                OnPropertyChanged();

                _lastSearchKeyword = null;
                _lastFoundLine = -1;

                _searchCommand?.RaiseCanExecuteChanged();
                _findNextCommand?.RaiseCanExecuteChanged();
                _findPreviousCommand?.RaiseCanExecuteChanged();
            }
        }

        public bool IsSearchVisible
        {
            get => _isSearchVisible;
            set
            {
                if (_isSearchVisible == value)
                    return;

                _isSearchVisible = value;
                OnPropertyChanged();
            }
        }
        
        public ICommand SearchCommand
        {
            get
            {
                return _searchCommand ??=
                    new AsyncRelayCommand(
                        FindNextAsync,
                        () => CanSearch());
            }
        }

        public ICommand FindNextCommand
        {
            get
            {
                return _findNextCommand ??=
                    new AsyncRelayCommand(
                        FindNextAsync,
                        () => CanSearch());
            }
        }

        public ICommand FindPreviousCommand
        {
            get
            {
                return _findPreviousCommand ??=
                    new AsyncRelayCommand(
                        FindPreviousAsync,
                        () => CanSearch());
            }
        }

        public string UrlText
        {
            get => _urlText;
            set
            {
                _urlText = value;
                OnPropertyChanged();
            }
        }

        public long CurrentStartLine
        {
            get => _currentStartLine;
            set
            {
                SetCurrentStartLine(value, true);
            }
        }

        public double ScrollBarPosition
        {
            get => _scrollBarPosition;
            set
            {
                double maxClamped = Math.Clamp(value, 0, MaxStartLine);

                if (Math.Abs(_scrollBarPosition - maxClamped) < 0.001)
                    return;

                _scrollBarPosition = maxClamped;
                OnPropertyChanged();
            }
        }

        public long TotalLines
        {
            get => _totalLines;
            set
            {
                _totalLines = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MaxStartLine));
                OnPropertyChanged(nameof(IsNavigable));
            }
        }

        public bool IsNavigable => TotalLines > 0 && IsIndexReady;

        public long MaxStartLine => Math.Max(0, TotalLines - ViewportLineCount);

        public int HighlightLineIndex
        {
            get => _highlightLineIndex;
            set
            {
                if (_highlightLineIndex == value)
                    return;

                _highlightLineIndex = value;
                OnPropertyChanged();
            }
        }

        public int HighlightStartIndex
        {
            get => _highlightStartIndex;
            set
            {
                if (_highlightStartIndex == value)
                    return;

                _highlightStartIndex = value;
                OnPropertyChanged();
            }
        }

        public int HighlightLength
        {
            get => _highlightLength;
            set
            {
                if (_highlightLength == value)
                    return;

                _highlightLength = value;
                OnPropertyChanged();
            }
        }

        #endregion

        private void SetCurrentStartLine(long value, bool updateScrollBar)
        {
            long maxClamped = Math.Clamp(value, 0, MaxStartLine);

            if (_currentStartLine == maxClamped)
                return;

            _currentStartLine = maxClamped;
            HighlightLineIndex = -1;
            HighlightStartIndex = -1;
            HighlightLength = 0;

            if (updateScrollBar)
                ScrollBarPosition = maxClamped;

            OnPropertyChanged(nameof(CurrentStartLine));
            OnPropertyChanged(nameof(MaxStartLine));

            _ = LoadVisibleLinesAsync();
        }


        private async Task OpenFileAsync()
        {
            OpenFileDialog fileDialog = new();
            fileDialog.Filter = "text files (*.txt)|*.txt|MD files (*.md)|*.md|log files (*.log)|*.log|csv files (*.csv)|*.csv|json files (*.json)|*.json|xml files (*.xml)|*.xml|html files (*.html)|*.html|All files (*.*)|*.*";

            if (fileDialog.ShowDialog() != true)
                return;

            await OpenTextSourceAsync(fileDialog.FileName);
        }

        private async Task OpenUrlAsync()
        {
            if (string.IsNullOrWhiteSpace(UrlText))
                return;

            try
            {
                IsIndexReady = false;
                StatusText = "Downloading...";

                string filePath = await DownloadUrlToTempFileAsync(UrlText.Trim(), _tempFiles);
                await OpenTextSourceAsync(filePath);
            }
            catch (Exception ex)
            {
                IsIndexReady = false;
                StatusText = $"Download failed: {ex.Message}";
            }
        }

        private async Task GenerateRandomAsync()
        {
            try
            {
                IsIndexReady = false;
                StatusText = "Generating random text...";

                string filePath = await GenerateRandomTextFileAsync(_tempFiles);
                await OpenTextSourceAsync(filePath);
            }
            catch (Exception ex)
            {
                IsIndexReady = false;
                StatusText = $"Random generation failed: {ex.Message}";
            }
        }

        private async Task OpenTextSourceAsync(string filePath)
        {
            IsIndexReady = false;
            IsBusy = true;

            _currentFilePath = filePath;

            CurrentStartLine = 0;
            ScrollBarPosition = 0;
            TotalLines = 0;
            IsIndexReady = false;

            StatusText = "Loading preview...";

            _textProvider = new TextProvider(filePath);

            var initialLines = await _textProvider.GetFirstLines(1000);
            DisplayText = string.Join(Environment.NewLine, initialLines);

            StatusText = "Indexing...";
            _ = BuildIndexInBackgroundAsync(filePath);
        }

        private async Task LoadVisibleLinesAsync()
        {
            if (_textProvider == null)
                return;

            try
            {
                var lines = await _textProvider.GetLinesAsync(
                    CurrentStartLine,
                    VisibleLineCount);

                DisplayText = string.Join(Environment.NewLine, lines);
            }
            catch (Exception ex)
            {
                StatusText = $"Loading lines failed: {ex.Message}";
            }
        }

        private async Task BuildIndexInBackgroundAsync(string filePath)
        {
            try
            {
                StatusText = "Indexing...";

                FileIndexer fileIndexer = new();
                FileIndex fileIndex = await fileIndexer.BuildIndexAsync(filePath, lines =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        StatusText = $"Indexing... {lines:N0} lines";
                    });
                });

                if (_currentFilePath != filePath)
                    return;

                if (_textProvider == null)
                    return;

                _textProvider.FileIndex = fileIndex;

                TotalLines = fileIndex.TotalLines;
                IsIndexReady = true;

                StatusText = $"Ready: {TotalLines} lines";
                await LoadVisibleLinesAsync();
            }
            catch (Exception ex)
            {
                IsIndexReady = false;
                StatusText = $"Indexing failed: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private static async Task<string> DownloadUrlToTempFileAsync(string url, List<string> tempFiles)
        {
            DateTime localDate = DateTime.Now;
            string filePath = $"temp_url_download_{localDate:HH_mm_ss}.txt";
            tempFiles.Add(filePath);
            using HttpClient httpClient = new();
            await using Stream input = await httpClient.GetStreamAsync(url);
            await using FileStream output = File.Create(filePath);

            await input.CopyToAsync(output);

            return filePath;
        }

        private static async Task<string> GenerateRandomTextFileAsync(List<string> tempFiles)
        {
            DateTime localDate = DateTime.Now;
            string filePath = $"temp_random_{localDate:HH_mm_ss}.txt";
            tempFiles.Add(filePath);

            Random random = new();

            await using FileStream stream = File.Create(filePath);
            await using StreamWriter writer = new(stream);
            long lineCount = random.Next(500_000, 1_000_000);

            for (int lineNumber = 0; lineNumber < lineCount; lineNumber++)
            {
                int stringlen = random.Next(5, 15);
                int numOfWords = random.Next(5,15);

                int randValue;
                StringBuilder builder = new();
                builder.Append($"Line {lineNumber}: ");

                for(int i=0; i < numOfWords; i++) 
                {
                    for (int j = 0; j < stringlen; j++)
                    {
                        randValue = random.Next(0, 26);
                        char letter = Convert.ToChar(randValue + 97);
                        builder.Append((char)letter);
                    }

                    builder.Append(" ");
                }

                await writer.WriteLineAsync(builder);
            }

            return filePath;
        }

        private async Task SaveAsAsync()
        {
            if (_currentFilePath == null) return;

            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
            };

            if (saveFileDialog.ShowDialog() != true) return;

            if (_currentFilePath == saveFileDialog.FileName)
            {
                MessageBox.Show("File can't be the same as file being used.");
                return;
            }

            await CopyFileFromReader(_currentFilePath, saveFileDialog.FileName);

            StatusText = "File successfully saved";
        }

        private async Task CopyFileFromReader(string filePath, string destinationPath)
        {
            await using FileStream stream = File.OpenRead(filePath);
            await using FileStream dest = File.Create(destinationPath);

            await stream.CopyToAsync(dest);
        }

        private void CommandsStateChanged()
        {
            _saveAsCommand?.RaiseCanExecuteChanged();
            _openFileCommand?.RaiseCanExecuteChanged();
            _generateRandomCommand?.RaiseCanExecuteChanged();
            _openUrlCommand?.RaiseCanExecuteChanged();
            _searchCommand?.RaiseCanExecuteChanged();
            _findNextCommand?.RaiseCanExecuteChanged();
            _findPreviousCommand?.RaiseCanExecuteChanged();
        }

        public void DeleteTempFiles()
        {
            try
            {
                if (_tempFiles.Count == 0) return;

                foreach (var tempFile in _tempFiles)
                {
                    File.Delete(tempFile);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private async void Search() 
        {
            await FindNextAsync();
        }

        private async void FindNext()
        {
            await FindNextAsync();
        }

        private async void FindPrevious()
        {
            await FindPreviousAsync();
        }

        private bool CanSearch()
        {
            return !string.IsNullOrWhiteSpace(Keyword) && IsIndexReady;
        }

        public async Task FindNextAsync()
        {
            if (_textProvider == null || !CanSearch())
                return;

            long startLine = _lastSearchKeyword == Keyword ? _lastFoundLine + 1: CurrentStartLine;

            if (startLine >= TotalLines)
                startLine = 0;

            StatusText = $"Searching for '{Keyword}'...";

            SearchResult? foundResult = await FindNextInRange(
                startLine,
                TotalLines - 1);

            if (foundResult == null && startLine > 0)
            {
                foundResult = await FindNextInRange(
                    0,
                    startLine - 1);
            }

            if (foundResult != null)
            {
                SetSearchResult(foundResult);
                return;
            }

            StatusText = "Search did not find anything";
        }

        public async Task<SearchResult?> FindNextInRange(long start, long end) 
        {

            if (_textProvider == null || !CanSearch()) return null;

            const int batchSize = 2000;

            long currentLine = start;

            while (currentLine <= end)
            {
                int count = (int)Math.Min(
                    batchSize,
                    end - currentLine + 1);

                var lines = await _textProvider.GetLinesAsync(
                    currentLine,
                    count);

                if (lines.Count == 0)
                    break;

                for (int i = 0; i < lines.Count; i++)
                {
                    int column = lines[i].IndexOf(Keyword, StringComparison.OrdinalIgnoreCase);

                    if (column >= 0)
                        return new SearchResult(currentLine + i, column);
                }

                currentLine += lines.Count;
            }

            return null;
        }

        public async Task FindPreviousAsync()
        {
            if (_textProvider == null || !CanSearch())
                return;

            long startLine =
                _lastSearchKeyword == Keyword
                    ? _lastFoundLine - 1
                    : CurrentStartLine - 1;

            if (startLine < 0)
                startLine = TotalLines - 1;

            StatusText = $"Searching backwards for '{Keyword}'...";

            SearchResult? foundResult = await FindPreviousInRangeAsync(
                startLine,
                0);

            if (foundResult == null && startLine < TotalLines - 1)
            {
                foundResult = await FindPreviousInRangeAsync(
                    TotalLines - 1,
                    startLine + 1);
            }

            if (foundResult != null)
            {
                SetSearchResult(foundResult);
                return;
            }

            StatusText = "Search did not find anything";
        }

        private async Task<SearchResult?> FindPreviousInRangeAsync(long start,long end)
        {

            if (_textProvider == null || !CanSearch()) return null;
            const int batchSize = 2000;

            long blockEnd = start;

            

            while (blockEnd >= end)
            {
                long blockStart = Math.Max(
                    end,
                    blockEnd - batchSize + 1);

                int count = (int)(blockEnd - blockStart + 1);

                var lines = await _textProvider.GetLinesAsync(
                    blockStart,
                    count);

                if (lines.Count == 0)
                    break;

                for (int i = lines.Count - 1; i >= 0; i--)
                {
                    int column = lines[i].IndexOf(Keyword, StringComparison.OrdinalIgnoreCase);

                    if (column >= 0)
                        return new SearchResult(blockStart + i, column);
                }

                blockEnd = blockStart - 1;
            }

            return null;
        }

        private void SetSearchResult(SearchResult result)
        {
            _lastSearchKeyword = Keyword;
            _lastFoundLine = result.Line;

            StatusText = $"Found keyword at line {result.Line} - {result.Column}";
            CurrentStartLine = result.Line;
            HighlightLineIndex = 0;
            HighlightStartIndex = result.Column;
            HighlightLength = Keyword.Length;
        }

    }
}
