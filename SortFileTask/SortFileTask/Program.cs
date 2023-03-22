var srcFileName = "Source.txt";
var destFileName = "Dest.txt";
var fileSorter = new FileSorter.FileSorter(1000);

await fileSorter.SortFile(srcFileName, destFileName);