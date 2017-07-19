using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace AssetBundles
{
	public class BuildScript
	{
		public static string overloadedDevelopmentServerURL = "";

		private const string BUILD_DIR_IOS = "iOS";
		private const string BUILD_DIR_ANDROID = "Android";
		private const string BUILD_DIR_WEBGL = "WebGL";
		private const string BUILD_DIR_OSX= "OSX";
	
		public static void BuildAssetBundles()
		{
			// Choose the output path according to the build target.
			string outputDir = GetBuildDirName();
			string outputPath = Path.Combine(Utility.AssetBundlesOutputPath, outputDir);
			if (!Directory.Exists(outputPath))
				Directory.CreateDirectory(outputPath);

			BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
		}

		public static void BuildIOSAssetBundles()
		{
			// Choose the output path according to the build target.
			string outputDir = GetBuildDirName();
			string outputPath = Path.Combine(Utility.AssetBundlesOutputPath, outputDir);
			if (!Directory.Exists(outputPath))
				Directory.CreateDirectory(outputPath);

			BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.iOS);
		}

		public static void CleanIOSAssetBundles()
		{
			string outputDir = GetBuildDirName();
			string outputPath = Path.Combine(Utility.AssetBundlesOutputPath, outputDir);

			FileUtil.DeleteFileOrDirectory(outputPath + "/waffle-ios.iphone6+");
			FileUtil.DeleteFileOrDirectory(outputPath + "/waffle-ios.iphone6+.manifest");
			FileUtil.DeleteFileOrDirectory(outputPath + "/waffle-ios.iphone6+.manifest.meta");
			FileUtil.DeleteFileOrDirectory(outputPath + "/waffle-ios.iphone6+.meta");
		}

		public static void BuildWebGLAssetBundles()
		{
			string outputPathWebGL = Path.Combine(Utility.AssetBundlesOutputPath, BUILD_DIR_WEBGL);
			if (!Directory.Exists(outputPathWebGL))
				Directory.CreateDirectory(outputPathWebGL);

			BuildPipeline.BuildAssetBundles(outputPathWebGL, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.WebGL);
		}

		public static void BuildAndroidAssetBundles()
		{
			string outputPathAndroid = Path.Combine(Utility.AssetBundlesOutputPath, BUILD_DIR_ANDROID);
			if (!Directory.Exists(outputPathAndroid))
				Directory.CreateDirectory(outputPathAndroid);

			BuildPipeline.BuildAssetBundles(outputPathAndroid, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.Android);

			FileUtil.DeleteFileOrDirectory(outputPathAndroid + "/waffle-ios.iphone6+");
			FileUtil.DeleteFileOrDirectory(outputPathAndroid + "/waffle-ios.iphone6+.manifest");
			FileUtil.DeleteFileOrDirectory(outputPathAndroid + "/waffle-ios.iphone6+.manifest.meta");
			FileUtil.DeleteFileOrDirectory(outputPathAndroid + "/waffle-ios.iphone6+.meta");
		}

		public static void BuildOSXAssetBundles()
		{
			// ADD COMMENTED OUT CODE TO BUILD SHADERS FOR OSX
			string outputPathOSX = Path.Combine(Utility.AssetBundlesOutputPath, BUILD_DIR_OSX);
			if (!Directory.Exists(outputPathOSX))
				Directory.CreateDirectory(outputPathOSX);

			BuildPipeline.BuildAssetBundles(outputPathOSX, BuildAssetBundleOptions.None, BuildTarget.StandaloneOSXUniversal);
		}
	
		public static void WriteServerURL()
		{
			string downloadURL;
			if (string.IsNullOrEmpty(overloadedDevelopmentServerURL) == false)
			{
				downloadURL = overloadedDevelopmentServerURL;
			}
			else
			{
				IPHostEntry host;
				string localIP = "";
				host = Dns.GetHostEntry(Dns.GetHostName());
				foreach (IPAddress ip in host.AddressList)
				{
					if (ip.AddressFamily == AddressFamily.InterNetwork)
					{
						localIP = ip.ToString();
						break;
					}
				}
				downloadURL = "http://" + localIP + ":7888/";
			}
			
			string assetBundleManagerResourcesDirectory = "Assets/AssetBundleManager/Resources";
			string assetBundleUrlPath = Path.Combine(assetBundleManagerResourcesDirectory, "AssetBundleServerURL.bytes");
			Directory.CreateDirectory(assetBundleManagerResourcesDirectory);
			File.WriteAllText(assetBundleUrlPath, downloadURL);
			AssetDatabase.Refresh();
		}
	
		public static void BuildPlayer()
		{
			var outputPath = EditorUtility.SaveFolderPanel("Choose Location of the Built Game", "", "");
			if (outputPath.Length == 0)
				return;
	
			string[] levels = GetLevelsFromBuildSettings();
			if (levels.Length == 0)
			{
				Debug.Log("Nothing to build.");
				return;
			}
	
			string targetName = GetBuildTargetName(EditorUserBuildSettings.activeBuildTarget);
			if (targetName == null)
				return;
	
			// Build and copy AssetBundles.
			BuildScript.BuildAssetBundles();
			WriteServerURL();
	
			BuildOptions option = EditorUserBuildSettings.development ? BuildOptions.Development : BuildOptions.None;
			BuildPipeline.BuildPlayer(levels, outputPath + targetName, EditorUserBuildSettings.activeBuildTarget, option);
		}
		
		public static void BuildStandalonePlayer()
		{
			var outputPath = EditorUtility.SaveFolderPanel("Choose Location of the Built Game", "", "");
			if (outputPath.Length == 0)
				return;
			
			string[] levels = GetLevelsFromBuildSettings();
			if (levels.Length == 0)
			{
				Debug.Log("Nothing to build.");
				return;
			}
			
			string targetName = GetBuildTargetName(EditorUserBuildSettings.activeBuildTarget);
			if (targetName == null)
				return;
			
			// Build and copy AssetBundles.
			BuildScript.BuildAssetBundles();
			BuildScript.CopyAssetBundlesTo(Path.Combine(Application.streamingAssetsPath, Utility.AssetBundlesOutputPath));
			AssetDatabase.Refresh();
			
			BuildOptions option = EditorUserBuildSettings.development ? BuildOptions.Development : BuildOptions.None;
			BuildPipeline.BuildPlayer(levels, outputPath + targetName, EditorUserBuildSettings.activeBuildTarget, option);
		}
	
		public static string GetBuildTargetName(BuildTarget target)
		{
			switch (target)
			{
			case BuildTarget.Android:
				return "/test.apk";
			case BuildTarget.StandaloneWindows:
			case BuildTarget.StandaloneWindows64:
				return "/test.exe";
			case BuildTarget.StandaloneOSXIntel:
			case BuildTarget.StandaloneOSXIntel64:
			case BuildTarget.StandaloneOSXUniversal:
				return "/test.app";
			case BuildTarget.WebGL:
				return "";
			// Add more build targets for your own.
			default:
				Debug.Log("Target not implemented.");
				return null;
			}
		}
	
		static void CopyAssetBundlesTo(string outputPath)
		{
			// Clear streaming assets folder.
			FileUtil.DeleteFileOrDirectory(Application.streamingAssetsPath);
			Directory.CreateDirectory(outputPath);
	
			string outputFolder = Utility.GetPlatformName();
	
			// Setup the source folder for assetbundles.
			var source = Path.Combine(Path.Combine(System.Environment.CurrentDirectory, Utility.AssetBundlesOutputPath), outputFolder);
			if (!System.IO.Directory.Exists(source))
				Debug.Log("No assetBundle output folder, try to build the assetBundles first.");
	
			// Setup the destination folder for assetbundles.
			var destination = System.IO.Path.Combine(outputPath, outputFolder);
			if (System.IO.Directory.Exists(destination))
				FileUtil.DeleteFileOrDirectory(destination);
			
			FileUtil.CopyFileOrDirectory(source, destination);
		}
	
		static string[] GetLevelsFromBuildSettings()
		{
			List<string> levels = new List<string> ();
			for (int i = 0; i < EditorBuildSettings.scenes.Length; ++i)
			{
				if (EditorBuildSettings.scenes[i].enabled)
					levels.Add(EditorBuildSettings.scenes[i].path);
			}
	
			return levels.ToArray();
		}

		static private string GetBuildDirName()
		{
			string outputDir = BUILD_DIR_IOS;

			switch (EditorUserBuildSettings.activeBuildTarget)
			{
			case BuildTarget.iOS:
				outputDir = BUILD_DIR_IOS;
				break;
			case BuildTarget.Android:
				outputDir = BUILD_DIR_ANDROID;
				break;
			case BuildTarget.WebGL:
				outputDir = BUILD_DIR_WEBGL;
				break;
			case BuildTarget.StandaloneOSXUniversal:
				outputDir = BUILD_DIR_OSX;
				break;
			}

			return outputDir;
		}
	}
}