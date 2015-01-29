using UnityEngine;
using System.Collections;
using System.IO;

public static class IOUtil
{
	// http://msdn.microsoft.com/en-us/library/vstudio/cc148994%28v=vs.100%29.aspx
	//
	public static void FileCopy(string fileName, string sourcePath, string targetPath)
	{
		// Use Path class to manipulate file and directory paths.
		string sourceFile = Path.Combine(sourcePath, fileName);
		string destFile = Path.Combine(targetPath, fileName);

		// To copy a folder's contents to a new location:
		// Create a new target folder, if necessary.
		if (!Directory.Exists(targetPath))
		{
			Directory.CreateDirectory(targetPath);
		}

		// To copy a file to another location and 
		// overwrite the destination file if it already exists.
		File.Copy(sourceFile, destFile, true);

		// To copy all the files in one directory to another directory.
		// Get the files in the source folder. (To recursively iterate through
		// all subfolders under the current directory, see
		// "How to: Iterate Through a Directory Tree.")
		// Note: Check for target path was performed previously
		//       in this code example.
		if (Directory.Exists(sourcePath))
		{
			string[] files = Directory.GetFiles(sourcePath);

			// Copy the files and overwrite destination files if they already exist.
			foreach (string s in files)
			{
				// Use static Path methods to extract only the file name from the path.
				fileName = Path.GetFileName(s);
				destFile = Path.Combine(targetPath, fileName);
				File.Copy(s, destFile, true);
			}
		}
		else
		{
			Debug.Log("Source path does not exist!");
		}

	}

	// http://msdn.microsoft.com/en-us/library/bb762914%28v=vs.110%29.aspx
	//
	public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
	{
		// Get the subdirectories for the specified directory.
		DirectoryInfo dir = new DirectoryInfo(sourceDirName);
		DirectoryInfo[] dirs = dir.GetDirectories();

		if (!dir.Exists)
		{
			throw new DirectoryNotFoundException(
				"Source directory does not exist or could not be found: "
				+ sourceDirName);
		}

		// If the destination directory doesn't exist, create it. 
		if (!Directory.Exists(destDirName))
		{
			Directory.CreateDirectory(destDirName);
		}

		// Get the files in the directory and copy them to the new location.
		FileInfo[] files = dir.GetFiles();
		foreach (FileInfo file in files)
		{
			string temppath = Path.Combine(destDirName, file.Name);
			file.CopyTo(temppath, false);
		}

		// If copying subdirectories, copy them and their contents to new location. 
		if (copySubDirs)
		{
			foreach (DirectoryInfo subdir in dirs)
			{
				string temppath = Path.Combine(destDirName, subdir.Name);
				DirectoryCopy(subdir.FullName, temppath, copySubDirs);
			}
		}
	}
}
