using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq.Expressions;
using Utils;


namespace Kanga
{
	/// <summary>
	/// Server object base.
	/// Base object for all objects returned by the server.
	/// AddMappings should be overridden in the child class to allow the object to populate itself from the hash table response
	/// All fields should be readonly so they can only be set in the constructor
	/// </summary>
	public abstract class ServerObjectBase
	{
		private static readonly bool s_enableLogs = false;
		protected Hashtable m_responseData;
		private QueueItem m_dependantQueueItem;
		private bool m_populated = false;
		private bool m_writeToDisk = false;

		public ServerObjectBase()
		{
			bool createdMapping = ServerObjectMappings.CreateNewType(this.GetType());
			if (createdMapping)
			{
				AddMappings();
			}
		}

		public void SetWriteToDisk()
		{
			m_writeToDisk = true;
		}

		public bool ShouldWriteToDisk()
		{
			return m_writeToDisk;
		}

		// override this to calculate if the server response object is newer or older than the version in the local cache
		public virtual bool IsNewer(ServerObjectBase cacheData)
		{
			return true;
		}

		// override this to fill out any information you can get when creating a new server object, eg player_id etc
		public virtual void Initialise(string objectID)
		{
		}

		public void Clone(ServerObjectBase serverObject)
		{
			Hashtable responseData = serverObject.GenerateHashTable(false);

			ParseResponseInternal(responseData, initEmptyArrays: true);
		}

		protected abstract void AddMappings();

		protected void AddMapping<T>(string serverKey, Expression<Func<T>> expr, bool uploadable = false)
		{
			var body = ((MemberExpression)expr.Body);
			ServerObjectMappings.AddMapping(this.GetType(), serverKey, body.Member.Name, uploadable);
		}

		public void ParseResponse(Hashtable responseData, bool populateNow)
		{
			m_responseData = responseData;

			if (populateNow)
			{
				ParseResponseInternal(m_responseData);
				m_populated = true;
			}
			else
			{
				ParseResponePartial(m_responseData);
			}
		}

		protected virtual void ParseResponePartial(Hashtable responseData, bool initEmptyArrays = false)
		{			
		}

		private void ParseDictionary(IDictionary objDict, Hashtable responseData, string key, Type objType)
		{
			Hashtable hTable = new Hashtable ();
			GetValue<Hashtable>(ref hTable, key, responseData);

			Type[] arguments = objType.GetGenericArguments();						
			Type valueType = arguments[1];

			if (valueType.IsSubclassOf(typeof(ServerObjectBase)))
			{
				foreach (var hTableKey in hTable.Keys)
				{
					ServerObjectBase newObj = (ServerObjectBase)Activator.CreateInstance(valueType);
					newObj.ParseResponse((Hashtable)hTable[hTableKey], true);
					objDict.Add(hTableKey, newObj);	
				}
			}
			else if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
			{
				foreach (var hTableKey in hTable.Keys)
				{
					IDictionary recurseDict = (IDictionary)Activator.CreateInstance(valueType);
					ParseDictionary(recurseDict, hTable, (string)hTableKey, valueType);
					objDict.Add(hTableKey, recurseDict);
				}
			}
			else
			{
				foreach (var hTableKey in hTable.Keys)
				{
					if (hTable[hTableKey] != null)
					{
						Type responseObjectType = hTable[hTableKey].GetType();
						TypeCode expectedTypeCode = Type.GetTypeCode(valueType);
						TypeCode responseTypeCode = Type.GetTypeCode(responseObjectType);

						if (responseTypeCode == expectedTypeCode)
						{
							objDict.Add(hTableKey, hTable[hTableKey]);
						}
						else
						{
							System.Object valueObject = 0;
							if (TryToCast(valueType, hTable[hTableKey], (string)hTableKey, ref valueObject))
							{
								objDict.Add(hTableKey, valueObject);
							}
							else
							{
								Debugger.Warning("Types do not match! Found a " + responseObjectType + " but expecting a " + valueType + " for server object " + key, (int)SharedSystems.Systems.SERVER_OBJECT);
							}
						}
					}
				}
			}
		}

		private void ParseArray(System.Collections.IList objArray, ArrayList responseDataList, Type objType, bool initEmptyArrays)
		{
			Type elementType = objType.GetElementType();

			if (elementType.IsSubclassOf(typeof(ServerObjectBase)))
			{
				int idx = 0;
				foreach (Hashtable item in responseDataList)
				{
					objArray[idx] = Activator.CreateInstance(elementType);
					ServerObjectBase serverObj = (ServerObjectBase)objArray[idx];
					serverObj.ParseResponse(item, true);
					idx++;
				}
			}
			else if (elementType.IsArray)
			{
				int numObjs = responseDataList.Count;

				for (int i = 0; i < numObjs; ++i)
				{
					ArrayList responseDataSubList = responseDataList[i] as ArrayList;

					if (responseDataSubList.Count > 0 || initEmptyArrays)
					{
						System.Collections.IList objSubArray = (System.Collections.IList)Activator.CreateInstance(elementType, new object[]{ responseDataSubList.Count });
						ParseArray(objSubArray, responseDataSubList, elementType, initEmptyArrays);
						objArray[i] = objSubArray;
					}
				}
			}
			else
			{
				Debugger.Log("type = " + elementType, (int)SharedSystems.Systems.SERVER_OBJECT);

				for (int j = 0; j < responseDataList.Count; ++j)
				{
					objArray[j] = responseDataList[j];
				}
			}
		}

		private void ParseResponseInternal(Hashtable responseData, bool initEmptyArrays = false)
		{
			Dictionary<string, ServerObjectMappings.MapObject>.KeyCollection keysToParse = ServerObjectMappings.GetKeys(this.GetType());
			ParseResponseForKeys(responseData, keysToParse, initEmptyArrays);
		}

		protected void ParseResponseForKeys(Hashtable responseData, Dictionary<string, ServerObjectMappings.MapObject>.KeyCollection keysToParse, bool initEmptyArrays = false)
		{
			foreach (string key in keysToParse)
			{
				Debugger.Log("key = " + key, (int)SharedSystems.Systems.SERVER_OBJECT);

				if (responseData.ContainsKey(key))
				{
					if (responseData[key] != null)
					{
						FieldInfo field = ServerObjectMappings.GetValue(this.GetType(), key);
						Type objType = field.FieldType;

						if (objType.IsGenericType && objType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
						{
							IDictionary objDict = (IDictionary)Activator.CreateInstance(objType);
							ParseDictionary(objDict, responseData, key, objType);
							field.SetValue(this, objDict);	
						}
						else if (objType.IsArray)
						{
							ArrayList responseDataList = new ArrayList ();
							GetValue<ArrayList>(ref responseDataList, key, responseData);

							if (responseDataList.Count > 0 || initEmptyArrays)
							{
								IList objArray = (IList)Activator.CreateInstance(objType, new object[]{ responseDataList.Count });

								ParseArray(objArray, responseDataList, objType, initEmptyArrays);

								field.SetValue(this, objArray);
							}
						}
						else if (objType.IsSubclassOf(typeof(ServerObjectBase)))
						{
							Hashtable hTableData = new Hashtable ();
							GetValue<Hashtable>(ref hTableData, key, responseData);

							ServerObjectBase responseObject = (ServerObjectBase)Activator.CreateInstance(objType);
							responseObject.ParseResponse(hTableData, true);
							field.SetValue(this, responseObject);
						}
						else
						{
							object responseObject = responseData[key];

							Type responseObjectType = responseObject.GetType();

							if (field.FieldType != responseObjectType)
							{
								System.Object valueObject = 0;
								if (TryToCast(field.FieldType, responseObject, key, ref valueObject))
								{
									field.SetValue(this, valueObject);
								}
							}
							else
							{
								field.SetValue(this, responseObject);
							}
						}
					}
				}
			}
		}

		private bool TryToCast(Type expectedType, object responseObject, string key, ref System.Object objectValue)
		{
			Type responseObjectType = responseObject.GetType();
			bool success = false;

			if (IsNumber(responseObjectType) && IsNumber(expectedType))
			{
				TypeCode expectedTypeCode = Type.GetTypeCode(expectedType);
				TypeCode responseTypeCode = Type.GetTypeCode(responseObjectType);

				if (responseTypeCode == System.TypeCode.Single && expectedTypeCode == System.TypeCode.Int32)
				{
					// Still do cast but chuck out a msg anyway
					Debugger.Log("Potential loss of precision. Casting from " + System.TypeCode.Single + " to " + System.TypeCode.Int32 + " for server object " + key, (int)SharedSystems.Systems.KANGA);

					float fValue = (float)responseObject;
					objectValue = (int)fValue;

					success = true;
				}
				else if (responseTypeCode == System.TypeCode.Single && expectedTypeCode == System.TypeCode.Int64)
				{
					// Still do cast but chuck out a msg anyway
					Debugger.Log("Potential loss of precision. Casting from " + System.TypeCode.Single + " to " + System.TypeCode.Int64 + " for server object " + key, (int)SharedSystems.Systems.KANGA);

					float fValue = (float)responseObject;
					objectValue = (long)fValue;

					success = true;
				}
				else if (responseTypeCode == System.TypeCode.Int32 && expectedTypeCode == System.TypeCode.Single)
				{
					int fValue = (int)responseObject;
					objectValue = (float)fValue;

					success = true;
				}
				else if (responseTypeCode == System.TypeCode.Int32 && expectedTypeCode == System.TypeCode.Int64)
				{
					int fValue = (int)responseObject;
					objectValue = (long)fValue;

					success = true;
				}
				else
				{
					Debugger.Warning("Types do not match! Found a " + responseObjectType + " but expecting a " + expectedType + " for server object " + key, (int)SharedSystems.Systems.KANGA);
				}
			}
			else
			{
				Debugger.Warning("Types do not match. Cannot cast from a " + responseObjectType + " to a" + expectedType + " for server object " + key, (int)SharedSystems.Systems.KANGA);	
			}

			return success;
		}


		/// <summary>
		/// JSON.cs always parses a number as either a float, int or long, so we only need to check for those 3 types
		/// </summary>
		private bool IsNumber(Type checkType)
		{
			bool ret = false;

			switch (Type.GetTypeCode(checkType))
			{
			case System.TypeCode.Int32:
			case System.TypeCode.Int64:
			case System.TypeCode.Single:
				ret = true;
				break;
			}
			return ret;
		}


		public QueueItem QueueItemDependancy
		{
			set
			{
				m_dependantQueueItem = value;
			}
			get
			{
				return m_dependantQueueItem;
			}
		}

		//
		public Hashtable GenerateHashTable(bool onlyUploadableFields = true)
		{
			Hashtable table = new Hashtable ();

			PopulateHashTable(ref table, onlyUploadableFields);

			return table;
		}

		private void PopulateDict(ref Hashtable table, string key, IDictionary objDict)
		{
			if (objDict != null)
			{
				Hashtable hTable = new Hashtable ();

				foreach (var dictKey in objDict.Keys)
				{
					Type objType = objDict[dictKey].GetType();
					if (objType.IsGenericType && objType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
					{
						IDictionary objDictNest = (IDictionary)objDict[dictKey];
						PopulateDict(ref hTable, (string)dictKey, objDictNest);
					}
					else
					{
						hTable.Add(dictKey, objDict[dictKey]);
					}
				}

				table.Add(key, hTable);
			}
			else
			{
				// TODO change back to null
				table.Add(key, new Hashtable ());
			}
		}

		//
		public void PopulateHashTable(ref Hashtable table, bool onlyUploadableFields = true)
		{
			Dictionary<string, ServerObjectMappings.MapObject>.KeyCollection keyColl = ServerObjectMappings.GetKeys(this.GetType());

			Populate();

			foreach (string key in keyColl)
			{
				if (onlyUploadableFields && !ServerObjectMappings.GetUploadable(this.GetType(), key))
				{
					continue;
				}
				FieldInfo field = ServerObjectMappings.GetValue(this.GetType(), key);
				Type objType = field.FieldType;

				// maybe need to do something else with arrays as well but we need to try it first and see how it ends up in the hash table
				if (objType.IsSubclassOf(typeof(ServerObjectBase)))
				{
					ServerObjectBase serverObject = (ServerObjectBase)field.GetValue(this);
					if (serverObject != null)
					{
						table.Add(key, serverObject.GenerateHashTable(onlyUploadableFields));
					}
					else
					{
						// TODO change back to null
						table.Add(key, new Hashtable ());
					}
				}
				else if (objType.IsGenericType && objType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
				{
					IDictionary objDict = (IDictionary)field.GetValue(this);
					PopulateDict(ref table, key, objDict);
				}
				else if (objType.IsArray)
				{
					IList objList = (IList)field.GetValue(this);
					if (objList != null)
					{
						ArrayList toArrayList = new ArrayList ();

						foreach (var objInList in objList)
						{
							if (objInList != null && objInList.GetType().IsSubclassOf(typeof(ServerObjectBase)))
							{
								ServerObjectBase serverObject = (ServerObjectBase)objInList;
								toArrayList.Add(serverObject.GenerateHashTable(onlyUploadableFields));	
							}
							else if (objInList != null && objInList.GetType().IsArray)
							{
								ArrayList subList = new ArrayList ();

								foreach (var objInSubList in objInList as IList)
								{
									subList.Add(objInSubList);
								}

								toArrayList.Add(subList);
							}
							else
							{
								toArrayList.Add(objInList);	
							}
						}

						table.Add(key, toArrayList);				
					}
					else
					{
						table.Add(key, null);
					}
				}
				else
				{
					table.Add(key, field.GetValue(this));
				}
			}
		}

		public virtual string GetObjectID()
		{
			return null;
		}

		public virtual string[] GetCacheDependecies()
		{
			Populate();
			
			return null;
		}

		public void Populate()
		{
			if (!m_populated && m_responseData != null)
			{
				ParseResponseInternal(m_responseData);
				m_populated = true;
			}
		}

		private void GetValue<T>(ref T obj, string key, Hashtable responseData)
		{
			if (responseData.ContainsKey(key))
			{
				if (responseData[key] != null)
				{
					object uncastObj = responseData[key];
					if (uncastObj.GetType() == typeof(T))
					{
						obj = (T)uncastObj;	
					}
					else
					{						
						Debugger.Log("Could cast server object type " + uncastObj.GetType() + " to type " + typeof(T) + " for key " + key, Debugger.Severity.ERROR);
					}	
				}
			}
			else
			{
				if (s_enableLogs)
				{
					Debugger.Log("Could not find key " + key + " in reposonse data");
				}
			}
		}

		public void Print(string msg = "", int system = -1)
		{
			#if SERVER_ENVIROMENT_CANDIDATE && !CANDIDATE_DEBUG
			return;
			#else

			Hashtable ht = m_responseData;
			if (ht == null)
			{
				ht = GenerateHashTable();
			}

			Debugger.PrintHashTableAsServerObject(ht, msg, system);

			#endif
		}
	}
}
