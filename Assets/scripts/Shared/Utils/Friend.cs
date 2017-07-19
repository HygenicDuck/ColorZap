
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class Contact
{
	public enum ContactType
	{
		WAFFLE,
		FACEBOOK,
		TWITTER
	};
		
	public string Name { get; set; }
	public string Id { get; set; }
	public string PictureUrl { get; set; }
	public string Title { get; set; }
	public ContactType Type { get; set; }
	public bool AlreadyInvited { get; set; }
	public float Level { get; set; }
	public string FacebookId { get; set; }
	public long TimeStamp { get; set; }

	public bool ExistInList(List<Contact> list)
	{
		for (int i = 0; i < list.Count; ++i)
		{
			if (Type == ContactType.WAFFLE)
			{
				if (list[i].Id == Id)
				{
					return true;
				}
			}
			else if (Type == ContactType.FACEBOOK)
			{
				if (list[i].FacebookId == FacebookId)
				{
					return true;
				}
			}
		}
		return false;
	}
}

public class InvitedContacts
{
	private static Dictionary<string, long> s_dictionary;
	private static long s_expireTime;
	static string FileName = "InvitedContactsFile_C";

	public static bool IsExpired(Contact contact)
	{
		string contactId = contact.Name;
		long currentTime = Utils.Date.GetEpochTimeMills();

		if (s_dictionary.ContainsKey(contactId))
		{
			bool isExpired = currentTime - s_dictionary[contactId] >= s_expireTime;

			return isExpired;
		}
		return true;
	}

	public static void AddContacts(List<Contact> contacts)
	{
		long timeStamp = Utils.Date.GetEpochTimeMills();
		for (int i = 0; i < contacts.Count; ++i)
		{
			Contact contact = contacts[i];
			if (contact.AlreadyInvited)
			{
				string key = contact.Name;
				if (!s_dictionary.ContainsKey(key))
				{
					s_dictionary.Add(key, timeStamp);
				}
				else if (IsExpired(contact))
				{
					s_dictionary[key] = timeStamp;
				}
			}
		}
		Serialize();
	}

	public static void AddContact(Contact contact)
	{
		long timeStamp = Utils.Date.GetEpochTimeMills();

		contact.AlreadyInvited = true;

		string key = contact.Name;
		if (!s_dictionary.ContainsKey(key))
		{
			s_dictionary.Add(key, timeStamp);
		}
		else if (IsExpired(contact))
		{
			s_dictionary[key] = timeStamp;
		}


		Serialize();
	}

	public static void Serialize()
	{
		string filePath = System.IO.Path.Combine(UnityEngine.Application.persistentDataPath, FileName);

		FileStream file = new FileStream (filePath, FileMode.OpenOrCreate);
		BinaryWriter writer = new BinaryWriter (file);
		writer.Write(s_dictionary.Count);
		foreach (var kvp in s_dictionary)
		{
			writer.Write(kvp.Key);
			writer.Write(kvp.Value);
		}
		writer.Flush();
	}

	public static void Deserialize(long expireTime)
	{
		if (s_dictionary == null)
		{
			s_expireTime = expireTime;

			string filePath = System.IO.Path.Combine(UnityEngine.Application.persistentDataPath, FileName);

			if (File.Exists(filePath))
			{
				long currentTime =  Utils.Date.GetEpochTimeMills();

				FileStream file = new FileStream (filePath, FileMode.Open);
				BinaryReader reader = new BinaryReader (file);
				int count = reader.ReadInt32();
				var dictionary = new Dictionary<string,long> (count);
				for (int n = 0; n < count; n++)
				{
					string key = reader.ReadString();
					long timeStamp = reader.ReadInt64();

					if (currentTime - timeStamp < s_expireTime)
					{
						dictionary.Add(key, timeStamp);
					}
				}
				s_dictionary = dictionary;      
			}
			else
			{
				s_dictionary = new Dictionary<string, long> ();
			}
		}
	}
}