using FileSorter.Utils;
using System.Threading.Tasks.Dataflow;

namespace FileSorter.Infra
{
    internal class Splitter : IDisposable
    {
        private readonly BatchBlock<string> _addLineBatchBlock;
        private readonly ActionBlock<string[]> _createFileActionBlock;
        private readonly IDisposable _batchToActionBlockLink;

        private readonly string _dirName;
        private readonly int _linesCount;

        private int _currentFileIndex;

        public Splitter(string dirName, int linesCount)
        {
            _dirName = dirName;
            _linesCount = linesCount;

            _addLineBatchBlock = new BatchBlock<string>(_linesCount, new GroupingDataflowBlockOptions { BoundedCapacity = DataflowBlockOptions.Unbounded });
            _createFileActionBlock = new ActionBlock<string[]>(ProcessLines, new ExecutionDataflowBlockOptions { BoundedCapacity = DataflowBlockOptions.Unbounded });

            _batchToActionBlockLink = _addLineBatchBlock.LinkTo(_createFileActionBlock);
        }

        public bool AddLine(string line) => _addLineBatchBlock.Post(line);


        public void Dispose()
        {
            _addLineBatchBlock.TriggerBatch();
            while (_addLineBatchBlock.OutputCount > 0) Task.Delay(100).Wait();
            Task.Delay(100).Wait();

            _addLineBatchBlock.Complete();
            _batchToActionBlockLink.Dispose();
        }

        private async Task ProcessLines(string[] lines)
        {
            try
            {
                var fileName = $"{++_currentFileIndex}_{Guid.NewGuid().ToString("N")}.unsorted.txt";
                var pathName = Path.Combine(_dirName, fileName);
                await FileUtil.CreateFile(pathName, lines);
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                throw;
            }
        }

        public static string[] GetUnsortedFiles(string dirName) => Directory.GetFiles(dirName, "*.unsorted.txt");
    }
}
