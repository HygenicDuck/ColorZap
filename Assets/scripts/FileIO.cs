using UnityEngine;
using System.Collections;
//using Utils;
using System.IO;

public abstract class FileIO
{

	protected FileIO()
	{
	}

	~FileIO()
	{
	}

	protected abstract string GetFileName();
	protected abstract Hashtable CreateHashtableToWrite();
	protected abstract void HandleHashtableFromRead(Hashtable hashtable);

	protected virtual void Initialise()
	{
		ReadFile();
	}

	protected void SaveData()
	{
		WriteFile();
	}

	private void ReadFile()
	{
		string fileName = GetFileName();
		//Debugger.Assert(!string.IsNullOrEmpty(fileName), "FileIO::ReadFile no filename from derived class");
		if (!string.IsNullOrEmpty(fileName))
		{
			string fullpath = Path.Combine(Application.persistentDataPath, GetFileName());
			Hashtable contentsHashtable = null;
			if (File.Exists(fullpath))
			{
				string json = File.ReadAllText(fullpath);
				contentsHashtable = JSON.JsonDecode(json) as Hashtable;
			}
			else
			{
				contentsHashtable = new Hashtable ();
			}

			HandleHashtableFromRead(contentsHashtable);
		}
	}

	private void WriteFile()
	{
		string fileName = GetFileName();
		//Debugger.Assert(!string.IsNullOrEmpty(fileName), "FileIO::WriteFile no filename from derived class");
		if (!string.IsNullOrEmpty(fileName))
		{
			string fullpath = Path.Combine(Application.persistentDataPath, GetFileName());

			Hashtable contentsHastable = CreateHashtableToWrite();
			//Debugger.Assert(contentsHastable != null, "FileIO::WriteFile null hashtable to write");
			if (contentsHastable != null)
			{
				string json = JSON.JsonEncode(contentsHastable);

				if (File.Exists(fullpath))
				{
					File.Delete(fullpath);
				}

				StreamWriter stream = File.AppendText(fullpath);
				stream.Write(json);
				stream.Close();
			}
		}
	}
}


