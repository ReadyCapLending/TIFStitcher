// See https://aka.ms/new-console-template for more information
using System.Diagnostics;

Console.WriteLine("Starting the TIF Stitcher...");
Console.WriteLine("Enter the loan number to start processing");
string startingLoanNumber = Console.ReadLine();
try
{
    // Iterate through all of the content folders 
    DirectoryInfo rootDir = new DirectoryInfo("P:");
    DirectoryInfo[] folders = rootDir.GetDirectories();
    Stopwatch stopwatch = new Stopwatch();

    // Loop through all of the folders and process the ones with unprocessed content
    foreach (DirectoryInfo folder in folders)
    {
        if (folder.Name.IndexOf("_") == -1)
        {
            if (Convert.ToInt32(folder.Name) >= Convert.ToInt32(startingLoanNumber))
            {
                stopwatch.Start();
                Console.WriteLine("Processing folder: " + folder.Name + " (Started at: " + DateTime.Now + ")");
                FileInfo[] files = folder.GetFiles();
                List<string> rootFilenames = new List<string>();

                // Get a distinct list of all the root file names (without _001 etc extensions) 
                foreach (FileInfo file in files)
                {
                    string fileNoSequence = file.Name.ToUpper();
                    if (fileNoSequence.ToUpper().Contains(".TIF"))
                    {
                        int underscorePos = file.Name.LastIndexOf('_');
                        if (underscorePos == -1)
                        {
                            underscorePos = file.Name.LastIndexOf('-');
                        }

                        if (underscorePos != -1)
                        {
                            fileNoSequence = fileNoSequence.Remove(underscorePos, fileNoSequence.Length - underscorePos);

                            if (!rootFilenames.Contains(fileNoSequence))
                            {
                                rootFilenames.Add(fileNoSequence);
                            }
                        }
                    }
                }

                int iRootName = 0;
                int rootNameCount = rootFilenames.Count;
                // Iterate through all of the distinct root filenames 
                foreach (string rootFileName in rootFilenames)
                {
                    iRootName++;
                    // First create a new directory to hold the processed files. 
                    Directory.CreateDirectory(rootDir + "\\_mergedTiff\\" + folder.Name);

                    string fullFileName = rootDir + "\\_mergedTiff\\" + folder.Name + "\\" + rootFileName + ".TIF";
                    if (File.Exists(fullFileName))
                    {
                        Console.WriteLine("Skipping: " + fullFileName + " because it already exists.");
                    }
                    else
                    {
                        Console.WriteLine("Stitching together TIFs for (" + iRootName + " of " + rootNameCount + "): " + rootFileName + ".TIF");
                        List<FileInfo> tiffFiles = files.Where(x => x.Name.IndexOf(rootFileName) > -1).OrderBy(x=>x.Name).ToList();
                        byte[][] multiTiffData = new byte[tiffFiles.Count][];

                        // Find all of the files associated to the current root filename 
                        int tiffCount = 0;
                        foreach (FileInfo file in tiffFiles)
                        {
                            byte[] tiffData = File.ReadAllBytes(file.FullName);
                            multiTiffData[tiffCount] = tiffData;
                            tiffCount++;
                        }

                        byte[] mergedTiff = TIFStitcher.TiffHelper.MergeTiff(multiTiffData);
                        File.WriteAllBytes(fullFileName, mergedTiff);
                        //Console.WriteLine("Created: " + rootFileName);
                    }
                }
                Console.WriteLine("Processed " + rootFilenames.Count + " distinct filenames in " + folder.Name);
                Console.WriteLine("Elapsed seconds: " + Convert.ToString(stopwatch.ElapsedMilliseconds / 1000.0));
                File.Create(rootDir + "\\" + folder.Name + "\\TiffsMergedSuccessfully.txt");
                stopwatch.Stop();
                stopwatch.Restart();
            }
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
}