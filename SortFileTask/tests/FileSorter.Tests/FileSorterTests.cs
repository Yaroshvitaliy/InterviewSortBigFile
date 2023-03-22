namespace FileSorter.Tests
{
    public class FileSorterTests
    {
        [Theory]
        [InlineData("data_correct.txt", "data_correct_dest.txt")]
        public async Task SortFileShouldWorkProperlyForCorrectScr(string src, string dest)
        {
            // Arrange
            var fileSorter = new FileSorter(3);

            // Act
            await fileSorter.SortFile(src, dest);

            // Assert
            var srcLines = File.ReadAllLines(src);
            var destLines = File.ReadAllLines(dest);
            Assert.Equal(srcLines.Length, destLines.Length);

            long? prevVal = null;

            foreach (var line in destLines)
            {
                var val = long.Parse(line);

                if (prevVal.HasValue)
                {
                    Assert.True(prevVal < val);
                }

                prevVal = val;
            }

            // Clean
            File.Delete(dest);
        }

        [Theory]
        [InlineData("data_empty.txt", "data_empty_dest.txt")]
        public async Task SortFileShouldWorkProperlyForEmptyScr(string src, string dest)
        {
            // Arrange
            var fileSorter = new FileSorter(3);

            // Act
            await fileSorter.SortFile(src, dest);

            // Assert
            Assert.False(File.Exists(dest));
        }


        [Theory]
        [InlineData("data_wrong.txt", "data_wrong_dest.txt")]
        public async Task SortFileShouldWorkProperlyForWrongScr(string src, string dest)
        {
            // Arrange
            var fileSorter = new FileSorter(3);

            // Act, Assert
            await Assert.ThrowsAsync<FormatException>(async () => await fileSorter.SortFile(src, dest));
        }

        [Theory]
        [InlineData("data_no_file.txt", "data_no_file_dest.txt")]
        public async Task SortFileShouldWorkProperlyForNotExistingScr(string src, string dest)
        {
            // Arrange
            var fileSorter = new FileSorter(3);

            // Act, Assert
            await Assert.ThrowsAsync<ArgumentException>(async () => await fileSorter.SortFile(src, dest));
        }
    }
}