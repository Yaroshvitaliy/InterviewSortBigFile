namespace FileSorter
{
    internal readonly struct FileLine
    {
        public int StreamReader { get; init; }
        public string Value { get; init; }
    }
}
