namespace TextReader.ViewModel.Model
{
    public class FileIndex
    {
        public List<IndexCheckpoint> Checkpoints { get; } = new();
        public long TotalLines { get; set; }
        public long FileSizeBytes { get; set; }

        public IndexCheckpoint FindNearestCheckpoint(long targetLine) 
        {
            if (Checkpoints.Count == 0)
                return new IndexCheckpoint(0, 0);

            int left = 0;
            int right = Checkpoints.Count - 1;
            int nearestIndex = 0;

            while (left <= right)
            {
                int middle = left + ((right - left) / 2);
                var checkpoint = Checkpoints[middle];

                if (checkpoint.LineNumber <= targetLine)
                {
                    nearestIndex = middle;
                    left = middle + 1;
                }
                else
                {
                    right = middle - 1;
                }
            }

            return Checkpoints[nearestIndex];
        }

    }
}
