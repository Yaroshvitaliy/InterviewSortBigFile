using FileSorter.Infra;
using FileSorter.Utils;
using System.Data;

namespace FileSorter
{
    public class FileSorter
    {
        private readonly int _linesCount;

        public FileSorter(int linesCount)
        {
            _linesCount= linesCount;
        }

        public async Task SortFile(string srcFileName, string destFileName)
        {
            if (!File.Exists(srcFileName)) 
            {
                throw new ArgumentException($"File does not exist: {srcFileName}");
            }

            var dirName = Guid.NewGuid().ToString("N");

            try
			{
                Directory.CreateDirectory(dirName);
                Console.WriteLine($"Created Directory: {dirName}");

                var unsortedFiles = Split(srcFileName, dirName);

                if (unsortedFiles.Length > 0)
                {
                    var sortedFiles = await Sort(unsortedFiles);
                    await Merge(sortedFiles, destFileName);
                }
            }
			catch (Exception ex)
			{
				Console.WriteLine($"An error has occurred: ${ex.Message}");
				throw;
			}
            finally
            {
                Directory.Delete(dirName, true);
                Console.WriteLine($"Deleted Directory: {dirName}");
            }
        }

        private string[] Split(string srcFileName, string dirName)
        {
            var textReaderProvider = () => File.OpenText(srcFileName);

            using (var fileSplitter = new Splitter(dirName, _linesCount))
            {
                foreach (var line in FileUtil.ReadAllLines(textReaderProvider))
                {
                    fileSplitter.AddLine(line);
                    //Console.WriteLine($"Added Line: {line}");
                }
            }

            var unsortedFiles = Splitter.GetUnsortedFiles(dirName);
            return unsortedFiles;
        }

        private async Task<string[]> Sort(string[] unsortedFiles)
        {
            var fileSorter = new Sorter(fileName => fileName.Replace("unsorted", "sorted"));
            var sortedFiles = new List<string>(unsortedFiles.Length);

            foreach (var unsortedFile in unsortedFiles)
            {
                var sortedFile = await fileSorter.Sort(unsortedFile);
                sortedFiles.Add(sortedFile);
                //Console.WriteLine($"Sorted: {sortedFile}");
            }

            return sortedFiles.ToArray();
        }

        private async Task Merge(string[] sortedFiles, string destFileName)
        {
            var (streamReaders, lines) = await GetLines(sortedFiles);
            var finishedStreamReaders = new List<int>(streamReaders.Length);
            var finished = false;
            await using var outputWriter = new StreamWriter(destFileName);

            while (!finished)
            {
                lines.Sort((x, y) => long.Parse(x.Value).CompareTo(long.Parse(y.Value)));
                var valueToWrite = lines[0].Value;
                var streamReaderIndex = lines[0].StreamReader;
                await outputWriter.WriteLineAsync(valueToWrite.AsMemory());

                if (streamReaders[streamReaderIndex].EndOfStream)
                {
                    var indexToRemove = lines.FindIndex(x => x.StreamReader == streamReaderIndex);
                    lines.RemoveAt(indexToRemove);
                    finishedStreamReaders.Add(streamReaderIndex);
                    finished = finishedStreamReaders.Count == streamReaders.Length;
                    continue;
                }

                var value = await streamReaders[streamReaderIndex].ReadLineAsync();
                lines[0] = new FileLine { Value = value!, StreamReader = streamReaderIndex };
            }

            outputWriter.Close();

            Clean(streamReaders, sortedFiles);
        }

        private async Task<(StreamReader[] StreamReaders, List<FileLine> rows)> GetLines(string[] sortedFiles)
        {
            var streamReaders = new StreamReader[sortedFiles.Length];
            var rows = new List<FileLine>(sortedFiles.Length);
            for (var i = 0; i < sortedFiles.Length; i++)
            {
                var sortedFilePath = sortedFiles[i];
                var sortedFileStream = File.OpenRead(sortedFilePath);
                streamReaders[i] = new StreamReader(sortedFileStream);
                var value = await streamReaders[i].ReadLineAsync();
                var row = new FileLine
                {
                    Value = value!,
                    StreamReader = i
                };
                rows.Add(row);
            }

            return (streamReaders, rows);
        }

        private void Clean(StreamReader[] streamReaders, string[] filesToMerge)
        {
            for (var i = 0; i < streamReaders.Length; i++)
            {
                streamReaders[i].Dispose();
                var temporaryFilename = $"{filesToMerge[i]}.tmp";
                File.Move(filesToMerge[i], temporaryFilename);
                File.Delete(temporaryFilename);
            }
        }
    }
}
