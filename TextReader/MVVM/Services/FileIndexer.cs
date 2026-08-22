using System.IO;
using TextReader.ViewModel.Model;

namespace TextReader.ViewModel.Services
{
    public class FileIndexer
    {
        const int _checkpointThreshold = 500;
        public async Task<FileIndex> BuildIndexAsync(string filePath, Action<long>? action)
        {
            var index = new FileIndex();
            await using var stream = File.OpenRead(filePath);

            index.FileSizeBytes = stream.Length;
            index.Checkpoints.Add(new IndexCheckpoint(0, 0));

            long lineNumber = 0;
            long absoluteOffset = 0;
            int lastByte = -1;
            byte[] buffer = new byte[4 * 1024 * 1024];
            int bytesRead;

            while ((bytesRead = await stream.ReadAsync(buffer)) > 0)
            {
                for (int i = 0; i < bytesRead; i++)
                {
                    lastByte = buffer[i];

                    if (buffer[i] != (byte)'\n')
                        continue;

                    lineNumber++;

                    if (lineNumber % _checkpointThreshold == 0)
                        index.Checkpoints.Add(new IndexCheckpoint(lineNumber, absoluteOffset + i + 1));

                    if (lineNumber % 100_000 == 0)
                        action?.Invoke(lineNumber);
                }

                absoluteOffset += bytesRead;
            }

            if (index.FileSizeBytes > 0 && lastByte != (byte)'\n')
                lineNumber++;

            action?.Invoke(lineNumber);

            index.TotalLines = lineNumber;
            return index;
        }
    }
}
