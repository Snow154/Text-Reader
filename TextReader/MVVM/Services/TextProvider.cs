using System.IO;
using System.Windows;
using TextReader.ViewModel.Model;

namespace TextReader.ViewModel.Services
{
    public class TextProvider
    {
        private string _filePath;
        private FileIndex? _fileIndex;

        public FileIndex? FileIndex
        {
            get => _fileIndex;
            set => _fileIndex = value;
        }

        public TextProvider(string filePath)
        {
            _filePath = filePath;
        }

        public async Task<List<string>> GetLinesAsync(long startline,int count, CancellationToken cancellationToken = default)
        {
            var result = new List<string>();

            if (_fileIndex == null)
                return new List<string>();

            var checkpoint = _fileIndex.FindNearestCheckpoint(startline);

            using var stream = File.OpenRead(_filePath);
            stream.Seek(checkpoint.ByteOffset, SeekOrigin.Begin);

            using var reader = new StreamReader(stream);
            var currentLine = checkpoint.LineNumber;

            while (currentLine < startline)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var line = await reader.ReadLineAsync(cancellationToken);

                if (line == null)
                    return result;

                currentLine++;
            }

            while (result.Count < count)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var line = await reader.ReadLineAsync(cancellationToken);

                if (line == null)
                    break;

                result.Add(line);
            }

            return result;
        }

        public async Task<List<string>> GetFirstLines(int count) 
        {
            var result = new List<string>(count);
            try
            {
                using var stream = File.OpenRead(_filePath);
                using var reader = new StreamReader(stream);

                while (result.Count < count)
                {
                    var line = await reader.ReadLineAsync();

                    if (line == null)
                        break;

                    result.Add(line);
                }

            }
            catch(Exception e) 
            {
                MessageBox.Show(e.Message);
            }

            return result;
        }
    }
}
