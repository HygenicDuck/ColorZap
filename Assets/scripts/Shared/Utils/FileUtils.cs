using System.Collections;
using System.IO;

namespace Utils
{
	public class FileUtils
	{
		public static string ReadTextFile(string sFileName)
		{
			string sFileNameFound = "";
			if (File.Exists(sFileName))
			{
				sFileNameFound = sFileName;
			}
			else if (File.Exists(sFileName + ".txt"))
			{
				sFileNameFound = sFileName + ".txt";
			}
			else
			{
				Debugger.Log("Could not find file '" + sFileName + "'.");
				return null;
			}

			StreamReader sr;
			try
			{
				sr = new StreamReader(sFileNameFound);
			}
			catch (System.Exception e)
			{
				Debugger.Warning("Something went wrong with read.  " + e.Message);
				return null;
			}

			string fileContents = sr.ReadToEnd();
			sr.Close();

			return fileContents;
		}

		public static void WriteTextFile(string sFilePathAndName, string sTextContents)
		{
			StreamWriter sw = new StreamWriter(sFilePathAndName);
			sw.WriteLine(sTextContents);
			sw.Flush();
			sw.Close();
		}
	}
}
