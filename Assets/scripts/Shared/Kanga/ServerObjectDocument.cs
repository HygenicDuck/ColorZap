using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Kanga
{
	/// <summary>
	/// Server object document.
	/// For every return schema there should be a matching ServerObjectDocument.
	/// </summary>
	public class ServerObjectDocument : Kanga.ServerObjectBase
	{
		public string objectID;
		public string objectType;
		public long deletedOn;
		public long createdOn;
		public long updatedOn;
		public bool isTempObject = false;

		public override string GetObjectID()
		{
			return objectID;
		}

		protected override void ParseResponePartial(Hashtable responseData, bool initEmptyArrays = false)
		{
			Dictionary<string, ServerObjectMappings.MapObject>.KeyCollection keysToParse = ServerObjectMappings.GetKeys(typeof(ServerObjectDocument));
			ParseResponseForKeys(responseData, keysToParse, initEmptyArrays);
		}

		public override bool IsNewer(ServerObjectBase serverObjectBase)
		{
			ServerObjectDocument serverObjectDocument = serverObjectBase as ServerObjectDocument;

			if (serverObjectDocument != null && serverObjectDocument.DisableOverwriteCache())
			{
				return false;
			}

			if (updatedOn > 0 && serverObjectDocument != null)
			{
				bool newer = updatedOn > serverObjectDocument.updatedOn;
				return newer;
			}

			return true;
		}

		public override void Initialise(string generatedID)
		{
			isTempObject = true;

			objectID = generatedID;
			objectType = ServerObjectFactory.GetObjectType(this.GetType());
		}

		protected override void AddMappings()
		{
			AddMapping("object_id", () => objectID);
			AddMapping("object_type", () => objectType);
			AddMapping("deleted_on", () => deletedOn);
			AddMapping("created_on", () => createdOn);
			AddMapping("updated_on", () => updatedOn);
		}

		public virtual bool DisableOverwriteCache()
		{
			return false;
		}
	}
}
