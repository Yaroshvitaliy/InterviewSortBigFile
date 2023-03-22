using FileSorter.Utils;

namespace FileSorter.Infra
{
    internal class Sorter
    {
        private readonly Func<string, string> _fileNameProvider;

        public Sorter(Func<string, string> fileNameProvider) 
        {
            _fileNameProvider = fileNameProvider;
        }

        public async Task<string> Sort(string fileName)
        {
            var sortedFileName = _fileNameProvider(fileName);
            var lines = await File.ReadAllLinesAsync(fileName);
            var orderedLines = OrderLines(lines);
            await FileUtil.CreateFile(sortedFileName, orderedLines);
            return sortedFileName;
        }

        public static IEnumerable<string> OrderLines(IEnumerable<string> unorderedLines) =>
            unorderedLines.Select(long.Parse).Order().Select(x => x.ToString());
    }
}
