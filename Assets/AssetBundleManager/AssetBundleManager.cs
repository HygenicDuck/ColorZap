using UnityEngine;
using Utils;


#if UNITY_EDITOR	
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace AssetBundles
{	
	// Loaded assetBundle contains the references count which can be used to unload dependent assetBundles automatically.
	public class LoadedAssetBundle
	{
		public AssetBundle m_AssetBundle;
		public int m_ReferencedCount;
		
		public LoadedAssetBundle(AssetBundle assetBundle)
		{
			m_AssetBundle = assetBundle;
			m_ReferencedCount = 1;
		}
	}
	
	// Class takes care of loading assetBundle and its dependencies automatically, loading variants automatically.
	public class AssetBundleManager : MonoBehaviour
	{
		public enum LogMode { All, JustErrors };
		public enum LogType { Info, Warning, Error };
	
		static LogMode m_LogMode = LogMode.All;
		static string m_BaseDownloadingURL = "";
		static string[] m_ActiveVariants =  {  };
		static AssetBundleManifest m_AssetBundleManifest = null;

		#if UNITY_EDITOR	
		static int m_SimulateAssetBundleInEditor = -1;
		static int m_LocalAssetBundlesInEditor = -1;
		static int m_SingleVariantSelectedInEditor = -1;
		static string m_SelectedDefaultVariantInEditor = "";
		const string kSimulateAssetBundles = "SimulateAssetBundles";
		const string kLocalAssetBundles = "LocalAssetBundles";
		const string kSingleVariant = "SingleVariant";
		const string kSelectedDefaultVariant = "EditorDefaultVariant";
		static bool m_editorBundlesLoaded = false;
		#endif
	
		static Dictionary<string, LoadedAssetBundle> m_LoadedAssetBundles = new Dictionary<string, LoadedAssetBundle> ();
		static Dictionary<string, WWW> m_DownloadingWWWs = new Dictionary<string, WWW> ();
		static Dictionary<string, string> m_DownloadingErrors = new Dictionary<string, string> ();
		static List<AssetBundleLoadOperation> m_InProgressOperations = new List<AssetBundleLoadOperation> ();
		static Dictionary<string, string[]> m_Dependencies = new Dictionary<string, string[]> ();
	
		public static LogMode logMode
		{
			get { return m_LogMode; }
			set { m_LogMode = value; }
		}
	
		// The base downloading url which is used to generate the full downloading url with the assetBundle names.
		public static string BaseDownloadingURL
		{
			get { return m_BaseDownloadingURL; }
			set { m_BaseDownloadingURL = value; }
		}
	
		// Variants which is used to define the active variants.
		public static string[] ActiveVariants
		{
			get { return m_ActiveVariants; }
			set { 
				m_ActiveVariants = value;
				Log(LogType.Info, "Active Variants: " + m_ActiveVariants[0]);
			}
		}
	
		// AssetBundleManifest object which can be used to load the dependecies and check suitable assetBundle variants.
		public static AssetBundleManifest AssetBundleManifestObject
		{
			set {
				m_AssetBundleManifest = value; 
			#if UNITY_EDITOR	
				LoadShadersWorkaround ();
			#endif

			}
		}
	
		private static void Log(LogType logType, string text)
		{
//			if (logType == LogType.Error)
//				Debugger.Error("[AssetBundleManager] " + text, (int)WaffleSystems.Systems.ASSET_BUNDLE_MANAGER );
//			else if (m_LogMode == LogMode.All)
//				Debugger.Log("[AssetBundleManager] " + text, (int)WaffleSystems.Systems.ASSET_BUNDLE_MANAGER);
		}
	
		#if UNITY_EDITOR
		// Flag to indicate if we want to simulate assetBundles in Editor without building them actually.
		public static bool SimulateAssetBundleInEditor 
		{
			get
			{
				if (m_SimulateAssetBundleInEditor == -1)
					m_SimulateAssetBundleInEditor = EditorPrefs.GetBool(kSimulateAssetBundles, true) ? 1 : 0;
				
				return m_SimulateAssetBundleInEditor != 0;
			}
			set
			{
				int newValue = value ? 1 : 0;
				if (newValue != m_SimulateAssetBundleInEditor)
				{
					m_SimulateAssetBundleInEditor = newValue;
					EditorPrefs.SetBool(kSimulateAssetBundles, value);
				}
			}
		}
		
	
		// Flag to indicate if we want to simulate assetBundles in Editor without building them actually.
		public static bool LocalAssetBundlesInEditor 
		{
			get
			{
				if (m_LocalAssetBundlesInEditor == -1)
					m_LocalAssetBundlesInEditor = EditorPrefs.GetBool(kLocalAssetBundles, true) ? 1 : 0;

				return m_LocalAssetBundlesInEditor != 0;
			}
			set
			{
				int newValue = value ? 1 : 0;
				if (newValue != m_LocalAssetBundlesInEditor)
				{
					m_LocalAssetBundlesInEditor = newValue;
					EditorPrefs.SetBool(kLocalAssetBundles, value);
				}
			}
		}

		// Flag to indicate if we want to simulate assetBundles in Editor without building them actually.
		public static bool SingleVariantSelectedInEditor 
		{
			get
			{
				if (m_SingleVariantSelectedInEditor == -1)
					m_SingleVariantSelectedInEditor = EditorPrefs.GetBool(kSingleVariant, true) ? 1 : 0;

				return m_SingleVariantSelectedInEditor != 0;
			}
			set
			{
				int newValue = value ? 1 : 0;
				if (newValue != m_SingleVariantSelectedInEditor)
				{
					m_SingleVariantSelectedInEditor = newValue;
					EditorPrefs.SetBool(kSingleVariant, value);
				}
			}
		}

		public static string SelectedDefaultVariant 
		{
			get
			{
				m_SelectedDefaultVariantInEditor = EditorPrefs.GetString (kSelectedDefaultVariant);

				return m_SelectedDefaultVariantInEditor;
			}
			set
			{
				string newValue = value;
				if (newValue != m_SelectedDefaultVariantInEditor)
				{
					m_SelectedDefaultVariantInEditor = newValue;
					EditorPrefs.SetString(kSelectedDefaultVariant, value);
				}
			}
		}

		#endif
	
		private static string GetStreamingAssetsPathURL()
		{
			if (Application.isEditor)
				return "file://" +  GetStreamingAssetsPath(); // Use the build output folder directly.
			else if (Application.isWebPlayer)
				return GetStreamingAssetsPath();
			else if (Application.isMobilePlatform || Application.isConsolePlatform)
				return GetStreamingAssetsPath();
			else // For standalone player.
				return GetStreamingAssetsPath();
		}

		private static string GetStreamingAssetsPath()
		{
			if (Application.isEditor)
				return System.Environment.CurrentDirectory.Replace("\\", "/") + "/Assets/StreamingAssets/" + Utility.GetPlatformName(); // Use the build output folder directly.
			else if (Application.isWebPlayer)
				return System.IO.Path.GetDirectoryName(Application.absoluteURL).Replace("\\", "/")+ "/StreamingAssets/" + Utility.GetPlatformName();
			else if (Application.isMobilePlatform || Application.isConsolePlatform)
				#if UNITY_ANDROID
				return Application.dataPath + "!assets" + "/" + Utility.GetPlatformName() + "/";
				#else 
				return Application.streamingAssetsPath + "/" + Utility.GetPlatformName();
				#endif
			else // For standalone player.
				return Application.streamingAssetsPath + "/" + Utility.GetPlatformName();
		}
	
		public static void SetSourceAssetBundleDirectory(string relativePath)
		{
			BaseDownloadingURL = GetStreamingAssetsPathURL() + relativePath;
			Log (LogType.Info, BaseDownloadingURL);
		}
		
		public static void SetSourceAssetBundleURL(string absolutePath)
		{
			BaseDownloadingURL = absolutePath + Utility.GetPlatformName() + "/";
		}
	
		public static void SetDevelopmentAssetBundleServer()
		{			
			TextAsset urlFile = Resources.Load("AssetBundleServerURL") as TextAsset;
			string url = (urlFile != null) ? urlFile.text.Trim() : null;
			if (url == null || url.Length == 0)
			{
				Log (LogType.Error, "Development Server URL could not be found.");
				//AssetBundleManager.SetSourceAssetBundleURL("http://localhost:7888/" + UnityHelper.GetPlatformName() + "/");
			}
			else
			{
				AssetBundleManager.SetSourceAssetBundleURL(url);
			}
		}
		
		// Get loaded AssetBundle, only return vaild object when all the dependencies are downloaded successfully.
		static public LoadedAssetBundle GetLoadedAssetBundle (string assetBundleName, out string error)
		{
			if (m_DownloadingErrors.TryGetValue(assetBundleName, out error) )
				return null;
		
			LoadedAssetBundle bundle = null;
			m_LoadedAssetBundles.TryGetValue(assetBundleName, out bundle);
			if (bundle == null)
				return null;
			
			// No dependencies are recorded, only the bundle itself is required.
			string[] dependencies = null;
			if (!m_Dependencies.TryGetValue(assetBundleName, out dependencies) )
				return bundle;
			
			// Make sure all dependencies are loaded
			foreach(var dependency in dependencies)
			{
				if (m_DownloadingErrors.TryGetValue(assetBundleName, out error) )
					return bundle;
	
				// Wait all the dependent assetBundles being loaded.
				LoadedAssetBundle dependentBundle;
				m_LoadedAssetBundles.TryGetValue(dependency, out dependentBundle);
				if (dependentBundle == null)
					return null;
			}
	
			return bundle;
		}

		// Remaps the asset bundle name to the best fitting asset bundle variant.
		static protected string RemapVariantName(string assetBundleName)
		{
			#if UNITY_EDITOR
			if (!UnityEditor.EditorApplication.isPlaying)
			{
				return assetBundleName;
			}
			#endif



			string[] bundlesWithVariant = m_AssetBundleManifest.GetAllAssetBundlesWithVariant();

			string[] split = assetBundleName.Split('.');

			int bestFit = int.MaxValue;
			int bestFitIndex = -1;
			// Loop all the assetBundles with variant to find the best fit variant assetBundle.
			for (int i = 0; i < bundlesWithVariant.Length; i++)
			{
				string[] curSplit = bundlesWithVariant[i].Split('.');
				if (curSplit[0] != split[0])
					continue;

				int found = System.Array.IndexOf(m_ActiveVariants, curSplit[1]);

				// If there is no active variant found. We still want to use the first 
				if (found == -1)
					found = int.MaxValue-1;

				if (found < bestFit)
				{
					bestFit = found;
					bestFitIndex = i;
				}
			}

			if (bestFit == int.MaxValue-1)
			{
//				Debugger.Warning("[AssetBundleManager] " + "Ambigious asset bundle variant chosen because there was no matching active variant: " + bundlesWithVariant[bestFitIndex], (int)WaffleSystems.Systems.ASSET_BUNDLE_MANAGER );

			}

			if (bestFitIndex != -1)
			{
				return bundlesWithVariant[bestFitIndex];
			}
			else
			{
				return assetBundleName;
			}
		}

		// Unload assetbundle and its dependencies.
		static public void UnloadAssetBundle(string assetBundleName)
		{
			UnloadAssetBundleInternal(assetBundleName);
			UnloadDependencies(assetBundleName);
		}

		static protected void UnloadDependencies(string assetBundleName)
		{
			string[] dependencies = null;
			if (!m_Dependencies.TryGetValue(assetBundleName, out dependencies) )
				return;

			// Loop dependencies.
			foreach(var dependency in dependencies)
			{
				UnloadAssetBundleInternal(dependency);
			}

			m_Dependencies.Remove(assetBundleName);
		}

		static protected void UnloadAssetBundleInternal(string assetBundleName)
		{
			string error;
			LoadedAssetBundle bundle = GetLoadedAssetBundle(assetBundleName, out error);
			if (bundle == null)
				return;

			if (--bundle.m_ReferencedCount == 0)
			{
				bundle.m_AssetBundle.Unload(false);
				m_LoadedAssetBundles.Remove(assetBundleName);

				Log(LogType.Info, assetBundleName + " has been unloaded successfully");
			}
		}

////////////////////////////////				ASYNC ZONE


		static public AssetBundleLoadManifestOperation Initialize ()
		{
			return Initialize(Utility.GetPlatformName());
		}
			
		// Load AssetBundleManifest.
		static public AssetBundleLoadManifestOperation Initialize (string manifestAssetBundleName)
		{
			var go = new GameObject("AssetBundleManager", typeof(AssetBundleManager));
			DontDestroyOnLoad(go);
	
			LoadAssetBundle(manifestAssetBundleName, true);
			var operation = new AssetBundleLoadManifestOperation (manifestAssetBundleName, "AssetBundleManifest", typeof(AssetBundleManifest));
			m_InProgressOperations.Add (operation);
			return operation;
		}
		
		// Load AssetBundle and its dependencies.
		static protected void LoadAssetBundle(string assetBundleName, bool isLoadingAssetBundleManifest = false)
		{
			Log(LogType.Info, "Loading Asset Bundle " + (isLoadingAssetBundleManifest ? "Manifest: " : ": ") + assetBundleName);
	
			if (!isLoadingAssetBundleManifest)
			{
				if (m_AssetBundleManifest == null)
				{
					Log (LogType.Error, "Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");
					return;
				}
			}
	
			// Check if the assetBundle has already been processed.
			bool isAlreadyProcessed = LoadAssetBundleInternal(assetBundleName, isLoadingAssetBundleManifest);
	
			// Load dependencies.
			if (!isAlreadyProcessed && !isLoadingAssetBundleManifest)
				LoadDependencies(assetBundleName);
		}
	
		// Where we actuall call WWW to download the assetBundle.
		static protected bool LoadAssetBundleInternal (string assetBundleName, bool isLoadingAssetBundleManifest)
		{
			// Already loaded.
			LoadedAssetBundle bundle = null;
			m_LoadedAssetBundles.TryGetValue(assetBundleName, out bundle);
			if (bundle != null)
			{
				bundle.m_ReferencedCount++;
				return true;
			}
	
			if (m_DownloadingWWWs.ContainsKey(assetBundleName) )
				return true;
	
			WWW download = null;
			string url = m_BaseDownloadingURL + assetBundleName;
		
			// For manifest assetbundle, always download it as we don't have hash for it.
			if (isLoadingAssetBundleManifest)
				download = new WWW(url);
			else
				download = WWW.LoadFromCacheOrDownload(url, m_AssetBundleManifest.GetAssetBundleHash(assetBundleName), 0); 
	
			m_DownloadingWWWs.Add(assetBundleName, download);
	
			return false;
		}
	
		// Where we get all the dependencies and load them all.
		static protected void LoadDependencies(string assetBundleName)
		{
			if (m_AssetBundleManifest == null)
			{
				Log (LogType.Error, "Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");
				return;
			}
	
			// Get dependecies from the AssetBundleManifest object..
			string[] dependencies = m_AssetBundleManifest.GetAllDependencies(assetBundleName);
			if (dependencies.Length == 0)
				return;
				
			for (int i=0;i<dependencies.Length;i++)
				dependencies[i] = RemapVariantName (dependencies[i]);
				
			// Record and load all dependencies.
			m_Dependencies.Add(assetBundleName, dependencies);
			for (int i=0;i<dependencies.Length;i++)
				LoadAssetBundleInternal(dependencies[i], false);
		}
	
	
		void Update()
		{
			// Collect all the finished WWWs.
			var keysToRemove = new List<string>();
			foreach (var keyValue in m_DownloadingWWWs)
			{
				WWW download = keyValue.Value;
	
				// If downloading fails.
				if (download.error != null)
				{
					m_DownloadingErrors.Add(keyValue.Key, string.Format("Failed downloading bundle {0} from {1}: {2}", keyValue.Key, download.url, download.error));
					keysToRemove.Add(keyValue.Key);
					continue;
				}
	
				// If downloading succeeds.
				if(download.isDone)
				{
					AssetBundle bundle = download.assetBundle;
					if (bundle == null)
					{
						m_DownloadingErrors.Add(keyValue.Key, string.Format("{0} is not a valid asset bundle.", keyValue.Key));
						keysToRemove.Add(keyValue.Key);
						continue;
					}
				
					m_LoadedAssetBundles.Add(keyValue.Key, new LoadedAssetBundle(download.assetBundle) );
					keysToRemove.Add(keyValue.Key);
				}
			}
	
			// Remove the finished WWWs.
			foreach( var key in keysToRemove)
			{
				WWW download = m_DownloadingWWWs[key];
				m_DownloadingWWWs.Remove(key);
				download.Dispose();
			}
	
			// Update all in progress operations
			for (int i=0;i<m_InProgressOperations.Count;)
			{
				if (!m_InProgressOperations[i].Update())
				{
					m_InProgressOperations.RemoveAt(i);
				}
				else
					i++;
			}
		}
	
		// Load asset from the given assetBundle.
		static public AssetBundleLoadAssetOperation LoadAssetAsync (string assetBundleName, string assetName, System.Type type)
		{
			Log(LogType.Info, "Loading " + assetName + " from " + assetBundleName + " bundle");
	
			AssetBundleLoadAssetOperation operation = null;

			{
				assetBundleName = RemapVariantName (assetBundleName);
				LoadAssetBundle (assetBundleName);
				operation = new AssetBundleLoadAssetOperationFull (assetBundleName, assetName, type);
	
				m_InProgressOperations.Add (operation);
			}
	
			return operation;
		}
	
		// Load level from the given assetBundle.
		static public AssetBundleLoadOperation LoadLevelAsync (string assetBundleName, string levelName, bool isAdditive)
		{
			Log(LogType.Info, "Loading " + levelName + " from " + assetBundleName + " bundle");
	
			AssetBundleLoadOperation operation = null;

			{
				assetBundleName = RemapVariantName(assetBundleName);
				LoadAssetBundle (assetBundleName);
				operation = new AssetBundleLoadLevelOperation (assetBundleName, levelName, isAdditive);
	
				m_InProgressOperations.Add (operation);
			}
	
			return operation;
		}


///////////////////////////////////////////////	Sync ZONE

		#if UNITY_EDITOR
		static protected void LoadEditorAssetBundles () {
			if (UnityEditor.EditorApplication.isPlaying && !m_editorBundlesLoaded) {
				string[] activeVariants = new string[1];
				activeVariants[0] = AssetBundleManager.SelectedDefaultVariant;
				AssetBundleManager.ActiveVariants = activeVariants;
				m_editorBundlesLoaded = true;
				AssetBundleManager.InitializeSync();
			}
		}
		#endif

		static public bool InitializeSync ()
		{
			return InitializeSync(Utility.GetPlatformName());
		}

		// Load AssetBundleManifest.
		static public bool InitializeSync (string manifestAssetBundleName)
		{
			#if UNITY_EDITOR
			if (!UnityEditor.EditorApplication.isPlaying)
			{
				return true;
			}
			else
			#endif
			{
				var go = new GameObject ("AssetBundleManager", typeof(AssetBundleManager));
				DontDestroyOnLoad (go);
			}

			LoadAssetBundleSync(manifestAssetBundleName, true);

			string 	m_DownloadingError;

			LoadedAssetBundle bundle = AssetBundleManager.GetLoadedAssetBundle (manifestAssetBundleName, out m_DownloadingError);

			string[] AssetList = bundle.m_AssetBundle.GetAllAssetNames();
			AssetBundleManifest manifest = bundle.m_AssetBundle.LoadAsset<AssetBundleManifest>(AssetList[0]);
			AssetBundleManifestObject = manifest;
			return true;

		}

		// Where we get all the dependencies and load them all.
		static protected void LoadDependenciesSync(string assetBundleName)
		{
			if (m_AssetBundleManifest == null)
			{
				Log (LogType.Error, "Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");
				return;
			}

			// Get dependecies from the AssetBundleManifest object..
			string[] dependencies = m_AssetBundleManifest.GetAllDependencies(assetBundleName);
			if (dependencies.Length == 0)
				return;

			for (int i=0;i<dependencies.Length;i++)
				dependencies[i] = RemapVariantName (dependencies[i]);

			// Record and load all dependencies.
			m_Dependencies.Add(assetBundleName, dependencies);
			for (int i=0;i<dependencies.Length;i++)
				LoadAssetBundleInternalSync(dependencies[i], false);
		}

		// Load AssetBundle and its dependencies.
		static protected void LoadAssetBundleSync(string assetBundleName, bool isLoadingAssetBundleManifest = false)
		{
			#if UNITY_EDITOR
			if (!UnityEditor.EditorApplication.isPlaying)
			{
				return;
			}
			#endif
			Log(LogType.Info, "Loading Asset Bundle " + (isLoadingAssetBundleManifest ? "Manifest: " : ": ") + assetBundleName);


			#if UNITY_EDITOR
			if (LocalAssetBundlesInEditor) {
				LoadEditorAssetBundles();
			}
			#endif

			if (!isLoadingAssetBundleManifest)
			{
				if (m_AssetBundleManifest == null)
				{
					Log (LogType.Error, "Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");
					return;
				}
			}

			// Check if the assetBundle has already been processed.
			bool isAlreadyProcessed = LoadAssetBundleInternalSync(assetBundleName, isLoadingAssetBundleManifest);

			// Load dependencies.
			if (!isAlreadyProcessed && !isLoadingAssetBundleManifest) {
				LoadDependenciesSync(assetBundleName);
			}
		}

		static protected bool LoadAssetBundleInternalSync (string assetBundleName, bool isLoadingAssetBundleManifest)
		{
			// Already loaded.
			LoadedAssetBundle bundle = null;
			m_LoadedAssetBundles.TryGetValue(assetBundleName, out bundle);
			if (bundle != null)
			{
				bundle.m_ReferencedCount++;
				return true;
			}

			#if UNITY_EDITOR
			if (!UnityEditor.EditorApplication.isPlaying)
			{
				return true;
			}
			else 
			#endif
			{
				string filePath = System.IO.Path.Combine (GetStreamingAssetsPath (), assetBundleName);
				AssetBundle loadedBundle = AssetBundle.LoadFromFile (filePath);
				if (loadedBundle == null)
				{
					return false;
				}

				m_LoadedAssetBundles.Add (assetBundleName, new LoadedAssetBundle (loadedBundle));
				return false;
			}

		}
			
		// Load scene from the given assetBundle.
		static public bool LoadScene (string assetBundleName, string sceneName, bool isAdditive)
		{
			#if UNITY_EDITOR
			if (LocalAssetBundlesInEditor) {
				LoadEditorAssetBundles();
			}
			#endif

			Log(LogType.Info, "Loading " + sceneName + " from " + assetBundleName + " bundle");

			assetBundleName = RemapVariantName(assetBundleName);
			LoadAssetBundleSync (assetBundleName);

			SceneManager.LoadScene (sceneName);

			return true;
		}

		static public Object LoadTexture(string textureName)
		{
			return LoadAsset("waffle-ios", textureName);
		}

		// Load asset from the given assetBundle.
		static public Object LoadAsset (string assetBundleName, string assetName)
		{
			Log(LogType.Info, "Loading " + assetName + " from " + assetBundleName + " bundle");

			#if UNITY_EDITOR
			if (LocalAssetBundlesInEditor) {
				LoadEditorAssetBundles();
			}
			#endif

			assetBundleName = RemapVariantName (assetBundleName);

			#if UNITY_EDITOR
			if (!UnityEditor.EditorApplication.isPlaying)
			{
				string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName (assetBundleName, assetName);
				if (assetPaths.Length == 0)
				{
					Log(LogType.Error, "There is no asset with name \"" + assetName + "\" in " + assetBundleName);
					return null;
				}

				// @TODO: Now we only get the main object from the first asset. Should consider type also.
				return AssetDatabase.LoadMainAssetAtPath (assetPaths [0]);
			}
			else 
			#endif
			{
				string m_DownloadingError;

				LoadAssetBundleSync (assetBundleName);

				LoadedAssetBundle bundle = AssetBundleManager.GetLoadedAssetBundle (assetBundleName, out m_DownloadingError);
				if (bundle != null)
				{
					return bundle.m_AssetBundle.LoadAsset (assetName);
				}
				else
				{
					return null;
				}
			}
		}

		static protected bool LoadShadersWorkaround ()
		{
			// Already loaded.
			LoadedAssetBundle bundle = null;
			m_LoadedAssetBundles.TryGetValue ("shaders", out bundle);
			if (bundle != null)
			{
				bundle.m_ReferencedCount++;
				return true;
			}
				
			string filePath = System.IO.Path.Combine(System.Environment.CurrentDirectory.Replace("\\", "/") + "/Assets/StreamingAssets/OSX", "shaders");
			AssetBundle loadedBundle = AssetBundle.LoadFromFile (filePath);

			if (loadedBundle == null)
			{
				return false;
			}

			m_LoadedAssetBundles.Add ("shaders", new LoadedAssetBundle (loadedBundle));
			LoadDependenciesSync("shaders");

			return false;
		}
	}
}