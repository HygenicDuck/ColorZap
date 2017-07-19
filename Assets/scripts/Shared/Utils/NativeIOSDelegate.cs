using UnityEngine;
using Core;

namespace Utils
{
	public class NativeIOSDelegate : BaseBehaviour
	{
		public string randomId;
		bool invoked = false;
		System.Action< System.Collections.Hashtable > deleg;

		public static NativeIOSDelegate CreateNativeIOSDelegate(System.Action< System.Collections.Hashtable > methodToCall)
		{
			int rndId = UnityEngine.Random.Range(100000, 999999);
			int iter = 0;
			while (GameObject.Find("NativeDelegate_" + rndId) != null && iter++ < 100)
			{
				rndId = UnityEngine.Random.Range(100000, 999999);
			}

			if (GameObject.Find("NativeDelegate_" + rndId) != null)
			{
				Debugger.Error("Error there is allready a callback existing");
				return null;
			}

			GameObject delegRoot = GameObject.Find("NativeDelegates");

			if (delegRoot == null)
			{
				delegRoot = new GameObject ("NativeDelegates");
			}

			GameObject goDeleg = new GameObject ("NativeDelegate_" + rndId);
			goDeleg.transform.parent = delegRoot.transform;
			NativeIOSDelegate test = goDeleg.AddComponent<NativeIOSDelegate>() as NativeIOSDelegate;
			test.deleg = methodToCall;

			return test;
		}

		public void CallDelegateFromNative(string strData)
		{
			if (invoked)
			{
				Debugger.Error("Error the delegate is allready invoked");
				return;
			}


			System.Collections.Hashtable retParams = JSON.JsonDecode(strData) as System.Collections.Hashtable;

			if (retParams == null)
			{
				Debugger.Log("NativeIOSDelegate returning null args");
			}

			Debugger.Log("Str= " + retParams + " str data = " + strData);

			deleg(retParams);
			this.enabled = false;
			this.gameObject.SetActive(false);
			GameObject.DestroyObject(this.gameObject); // <- destroy this game object. to be sure we dont call it anymore
		}
	}
}