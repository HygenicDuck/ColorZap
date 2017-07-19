using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UI;
using System.IO;

namespace Utils
{
	public class Debugger
	{
		private const string SAVE_FILE = "waffle_log.txt";
		private const string PREVIOUS_SAVE_FILE = "previous_waffle_log.txt";

		public enum Severity
		{
			MESSAGE = 0x000001,
			// log a message, will only show if logging is enabled on the system
			WARNING = 0x000010,
			// log a warning, will always show, calls Debug.LogWarning
			ERROR = 0x000100,
			// error, log an error, display a popup, errors can be ignored to stop them from spamming
		}

		private class System
		{
			public string name = "DEFAULT";
			public List<string> logs = new List<string> ();

			public bool enableLogging = true;
		}

		static private string DEBUG_POPUP_NAME = "DebugPopup";
		static private int s_loggingLevel = (int)Severity.WARNING | (int)Severity.ERROR;
		static private System s_defaultSystem = new System ();
		static private System[] s_systems;
		static private HashSet<string> s_ignoreDebug = new HashSet<string> ();
		static private StreamWriter s_logStream;

		static public void Init(Type gameSystems)
		{
			string[] systems = Enum.GetNames(gameSystems);
			string[] sharedSystems = Enum.GetNames(typeof(SharedSystems.Systems));
			string fullpath = Path.Combine(Application.persistentDataPath, SAVE_FILE);

			if (File.Exists(fullpath))
			{
				string copypath = Path.Combine(Application.persistentDataPath, PREVIOUS_SAVE_FILE);

				if (File.Exists(copypath))
				{
					File.Delete(copypath);
				}

				File.Copy(fullpath, copypath);
				File.Delete(fullpath);
			}

			s_logStream = File.AppendText(fullpath);

			//	gameSystemSize has a -1 here as the systems.Length includes the MAX_SYSTEMS string in the array
			int gameSystemSize = systems.Length - 1;

			int sharedSystemSize = (int)SharedSystems.Systems.MAX_SYSTEMS;
			int systemSize = sharedSystemSize + gameSystemSize;
			s_systems = new System[systemSize];

			for (int i = 0; i < systemSize; ++i)
			{
				string name = (i < sharedSystemSize) ? sharedSystems[i] : systems[i - sharedSystemSize];

				s_systems[i] = new System ();
				s_systems[i].name = name;
				s_systems[i].enableLogging = false;
			}
		}

		static public void CloseFileStream()
		{
			s_logStream.Close();
		}

		static public void EnableAllSystems(bool enable = true)
		{
			for (int i = 0; i < s_systems.Length; ++i)
			{
				s_systems[i].enableLogging = enable;
			}
		}

		static public void SetLoggingLevel(int level)
		{
			s_loggingLevel = level;
		}

		static public void SetLoggingEnabled(bool enabled, int system)
		{
			s_systems[system].enableLogging = enabled;
		}

		static public void Log(string log, Severity severity, int system = -1)
		{
			if (system == -1 || s_systems == null)
			{
				LogInternal(ref s_defaultSystem, log, severity);
			}
			else
			{
				LogInternal(ref s_systems[system], log, severity);
			}
		}

		static public void Log(string log, int system = -1)
		{
			Log(log, Severity.MESSAGE, system);
		}

		static public void Warning(string log, int system = -1)
		{
			Log(log, Severity.WARNING, system);
		}

		static public void Error(string log, int system = -1)
		{
			Log(log, Severity.ERROR, system);
		}

		public static void LogHashtableAsJson(string message, Hashtable hashtable, int system = -1)
		{
			if (hashtable != null && (system == -1 || s_systems[system].enableLogging))
			{
				Debugger.Log(message + " :: " + JSON.JsonEncode(hashtable), Debugger.Severity.MESSAGE, system);
			}
		}

		public static void PrintHashTableAsServerObject(Hashtable hashtable, string name = "object", int system = -1)
		{	
			if (hashtable != null && (system == -1 || s_systems[system].enableLogging))
			{
				Debugger.Log(name + ":" + ServerObjectAsString(hashtable), Debugger.Severity.MESSAGE, system);
			}
		}

		public static void PrintDictionaryAsServerObject(IDictionary<string,object> dictionary, string name = "object", int system = -1)
		{	
			if (dictionary != null && (system == -1 || s_systems[system].enableLogging))
			{
				Hashtable hastable = new Hashtable ((IDictionary)dictionary);
				Debugger.Log(name + ":" + ServerObjectAsString(hastable), Debugger.Severity.MESSAGE, system);
			}
		}

		public static void PrintHashTableKeys(Hashtable hashtable, string name = "object", int system = -1)
		{
			if (hashtable != null && (system == -1 || s_systems[system].enableLogging))
			{
				string keys = "[\n";
				foreach (string key in hashtable.Keys)
				{
					keys += key + ",\n";
				}
				keys += "]\n";
				Debugger.Log(name + ":" + keys, Debugger.Severity.MESSAGE, system);
			}
		}

		static public void Assert(bool condition, string message)
		{
			if (condition == false)
			{
				Debugger.Log(message, Debugger.Severity.ERROR);
			}
		}

		static private void LogInternal(ref System system, string log, Severity severity)
		{
			if (((int)severity & s_loggingLevel) == (int)severity && system.enableLogging)
			{
				if (s_logStream != null)
				{
					s_logStream.WriteLine(Date.GetEpochTimeMills() + " :: " + severity.ToString() + " :: " + system.name + " :: " + log);
				}
			}

			#if SERVER_ENVIROMENT_CANDIDATE && !CANDIDATE_DEBUG

			return;

			#else

			system.logs.Add(log);

			switch (severity)
			{
			case Severity.MESSAGE:
				
				if (system.enableLogging)
				{
					Debug.Log(system.name + " :: " + log);
				}

				break;
			case Severity.WARNING:
				
				Debug.LogWarning(system.name + " :: " + log);

				break;
			case Severity.ERROR:
				
				ErrorInternal(ref system, log);

				break;
			default:
				break;
			}
			#endif
		}

		static private void ErrorInternal(ref System system, string error)
		{
			string stackTrace = Environment.StackTrace;

			if (s_ignoreDebug.Contains(stackTrace))
			{
				return;
			}

			Debug.LogError(system.name + " :: " + error);

/*			PopupMessage.SetupData data = new PopupMessage.SetupData {
				popupType = UI.PopupManager.PopupType.DEBUG,
				prefabName = DEBUG_POPUP_NAME,
				title = "ERROR :: " + system.name,
				button1Text = "CONTINUE",
				button2Text = "IGNORE",
				description = error,
				buttonCallback = (int buttonIndex) => {
					if (buttonIndex == PopupMessage.SECOND_BUTTON_INDEX)
					{
						s_ignoreDebug.Add(stackTrace);
					}
				}
			};
				
			UI.PopupManager.Instance.ShowPopup(data);*/
		}
	

		public static string ServerObjectAsString(Hashtable hashtable, int level = 0)
		{
			string tabs = GetTabs(level);
			string output = "\n" + tabs + "{";

			if (hashtable == null)
			{
				Debugger.Log("error", Debugger.Severity.ERROR);
			}

			foreach (string key in hashtable.Keys)
			{
				var element = hashtable[key];
				if (element != null)
				{
					if (element.GetType() == typeof(Hashtable)) //Print HashTable
					{
						output += "\n" + tabs + "\t" + key + " : " + ServerObjectAsString((Hashtable)element, level + 1);
					}
					else if (element.GetType() == typeof(Hashtable[])) //Print Array
					{
						output += "\n\t" + tabs + key + ": [";
						Hashtable[] elements = (Hashtable[])element;
						for (int i = 0; i < elements.Length; ++i)
						{
							output += ServerObjectAsString(elements[i], level + 1) + ",";
						}
						output += "\n\t" + tabs + "]";
					}
					else if (element.GetType() == typeof(ArrayList))
					{
						ArrayList elements = (ArrayList)element;
						output += "\n\t" + tabs + key + ": [";
						for (int i = 0; i < elements.Count; ++i)
						{
							if (elements[i] == null)
							{
								output += "\n\t\t" + tabs + " null,";
							}
							else if (elements[i].GetType() == typeof(Hashtable))
							{
								output += ServerObjectAsString((Hashtable)(elements[i]), level + 1) + ",";
							}
							else
							{
								output += "\n\t\t" + tabs + elements[i] + ",";
							}
						}
						output += "\n\t" + tabs + "]";
					}
					else if (element.GetType() == typeof(List<object>))
					{
						List<object> elements = (List<object>)element;
						output += "\n\t" + tabs + key + ": [";
						for (int i = 0; i < elements.Count; ++i)
						{
							if (elements[i] == null)
							{
								output += "\n\t\t" + tabs + " null,";
							}
							else if (elements[i].GetType() == typeof(Dictionary<string,object>))
							{
								Hashtable newtable = new Hashtable ((IDictionary)elements[i]);
								output += ServerObjectAsString(newtable, level + 1) + ",";
							}
							else
							{
								output += "\n\t\t" + tabs + elements[i] + ",";
							}
						}
						output += "\n\t" + tabs + "]";
					}
					else if (element.GetType() == typeof(Dictionary<string, object>))
					{
						Hashtable newtable = new Hashtable ((IDictionary)element);
						output += ServerObjectAsString(newtable, level + 1) + ",";
					}
					else if (element.GetType() == typeof(String[]))
					{
						String[] elements = (String[])element;
						output += "\n\t" + tabs + key + ": [";
						for (int i = 0; i < elements.Length; ++i)
						{
							output += "\n\t\t" + tabs + elements[i] + ",";
						}
						output += "\n\t" + tabs + "]";
					}
					else //Print value
					{
						output += "\n" + tabs + "\t" + key + " : " + hashtable[key];
					}
				}
				else
				{
					output += "\n" + tabs + "\t" + key + " : null";
				}
			}
			output += "\n" + tabs + "}";
			return output;		
		}

		private static string GetTabs(int level)
		{
			string output = "";
			for (int i = 0; i < level; ++i)
			{
				output += output + "\t"; 
			}
			return output;
		}
	}
}