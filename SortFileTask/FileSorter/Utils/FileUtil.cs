namespace FileSorter.Utils
{
    internal static class FileUtil
    {
        public static IEnumerable<string> ReadAllLines(Func<TextReader> textReaderProvider)
        {
            using (var reader = textReaderProvider())
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

        public static async Task CreateFile(string dirName, string fileName, IEnumerable<string> lines)
        {
            var pathName = Path.Combine(dirName, fileName);
            await CreateFile(pathName, lines);
        }

        public static async Task CreateFile(string fileName, IEnumerable<string> lines)
        {
            await File.WriteAllLinesAsync(fileName, lines);
            while (!File.Exists(fileName)) await Task.Delay(100);
        }
    }
}
