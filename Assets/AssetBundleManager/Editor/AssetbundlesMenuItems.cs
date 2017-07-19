using UnityEngine;
using UnityEditor;
using System.Collections;

namespace AssetBundles
{
	public class AssetBundlesMenuItems
	{
		const string kSimulationMode =      "Kwalee/AssetBundles/Simulation Mode";
		const string kLocalAssetMode =      "Kwalee/AssetBundles/Local Assetbundles Mode";
		const string kSingleVariant =       "Kwalee/AssetBundles/Single Variant Mode";
		const string kVariantIphone6Plus =  "Kwalee/AssetBundles/AssetBundle Variant Selected/iPhone 6+   \t(2208x1242)";
		const string kVariantIphone6 =   	"Kwalee/AssetBundles/AssetBundle Variant Selected/iPhone 6    \t(1334x750)";
		const string kVariantIphone5 = 		"Kwalee/AssetBundles/AssetBundle Variant Selected/iPhone 5    \t(1136x640)";
		const string kVariantIphone4 = 		"Kwalee/AssetBundles/AssetBundle Variant Selected/iPhone 4    \t(960x640)";
		const string kVariantIPAD = 		"Kwalee/AssetBundles/AssetBundle Variant Selected/iPad        \t(1024x768)";
		const string kVariantIPADRetina = 	"Kwalee/AssetBundles/AssetBundle Variant Selected/iPad Retina \t(2048x1536)";
		const string kVariantIPADPro = 		"Kwalee/AssetBundles/AssetBundle Variant Selected/iPad Pro    \t(2732x2048)";


		[MenuItem(kLocalAssetMode, false, 1)]
		public static void ToggleLocallAssetBundlesnMode ()
		{
			AssetBundleManager.LocalAssetBundlesInEditor = !AssetBundleManager.LocalAssetBundlesInEditor;
		}

		[MenuItem(kLocalAssetMode, true, 1)]
		public static bool ToggleLocalAssetBundlesModeValidate ()
		{
			//Menu.SetChecked(kLocalAssetMode, AssetBundleManager.LocalAssetBundlesInEditor);
			return true;
		}


		[MenuItem(kSingleVariant, false, 2)]
		public static void ToggleSingleVariantMode ()
		{
			AssetBundleManager.SingleVariantSelectedInEditor = !AssetBundleManager.SingleVariantSelectedInEditor;
		}

		[MenuItem(kSingleVariant, true, 2)]
		public static bool ToggleSingleVariantModeValidate ()
		{
			//Menu.SetChecked(kSingleVariant, AssetBundleManager.SingleVariantSelectedInEditor);
			return true;
		}

		[MenuItem ("Kwalee/AssetBundles/Clean Streaming Assets", false, 3)]
		static public void CleanStreamingOSXAssets ()
		{
			//ExternalScripts.RunBashScript ("CleanStreamingAssets");
		}

		[MenuItem ("Kwalee/AssetBundles/Build AssetBundles", false, 4)]
		static public void BuildAssetBundles ()
		{
			BuildScript.BuildAssetBundles();
		}

		[MenuItem ("Kwalee/AssetBundles/Build OSX AssetBundles (Shaders)", false, 5)]
		static public void BuildOSXAssetBundles ()
		{
			BuildScript.BuildOSXAssetBundles();
		}

		[MenuItem ("Kwalee/AssetBundles/Build Android AssetBundles", false, 6)]
		static public void BuildAndroidAssetBundles ()
		{
			BuildScript.BuildAndroidAssetBundles();
		}

		///////// Variants

		[MenuItem(kVariantIphone6Plus, false, 1)]
		public static void ToggleIphone6Plus ()
		{
			AssetBundleManager.SelectedDefaultVariant = "iphone6+";
		}

		[MenuItem(kVariantIphone6Plus, true, 1)]
		public static bool ToggleIphone6PlusValidade ()
		{
			//Menu.SetChecked(kVariantIphone6Plus, (AssetBundleManager.SelectedDefaultVariant == "iphone6+"));
			return true;
		}


		[MenuItem(kVariantIphone6, false, 2)]
		public static void ToggleIphone6 ()
		{
			AssetBundleManager.SelectedDefaultVariant = "iphone6";
		}

		[MenuItem(kVariantIphone6, true, 2)]
		public static bool ToggleIphone6Validade ()
		{
			//Menu.SetChecked(kVariantIphone6, (AssetBundleManager.SelectedDefaultVariant == "iphone6"));
			return true;
		}


		[MenuItem(kVariantIphone5, false, 3)]
		public static void ToggleIphone5 ()
		{
			AssetBundleManager.SelectedDefaultVariant = "iphone5";
		}

		[MenuItem(kVariantIphone5, true, 3)]
		public static bool ToggleIphone5Validade ()
		{
			//Menu.SetChecked(kVariantIphone5, (AssetBundleManager.SelectedDefaultVariant == "iphone5"));
			return true;
		}


		[MenuItem(kVariantIphone4, false, 11)]
		public static void ToggleIphone4 ()
		{
			AssetBundleManager.SelectedDefaultVariant = "iphone4";
		}

		[MenuItem(kVariantIphone4, true, 11)]
		public static bool ToggleIphone4Validade ()
		{
			//Menu.SetChecked(kVariantIphone4, (AssetBundleManager.SelectedDefaultVariant == "iphone4"));
			return true;
		}


		[MenuItem(kVariantIPADPro, false, 31)]
		public static void ToggleiPADPro ()
		{
			AssetBundleManager.SelectedDefaultVariant = "ipadpro"; 
		}

		[MenuItem(kVariantIPADPro, true, 31)]
		public static bool ToggleiPADProValidade ()
		{
			//Menu.SetChecked(kVariantIPADPro, AssetBundleManager.SelectedDefaultVariant == "ipadpro");
			return true;
		}

		[MenuItem(kVariantIPADRetina, false, 32)]
		public static void ToggleiPADRetina ()
		{
			AssetBundleManager.SelectedDefaultVariant = "ipadretina"; 
		}

		[MenuItem(kVariantIPADRetina, true, 32)]
		public static bool ToggleiPADRetinaValidade ()
		{
			//Menu.SetChecked(kVariantIPADRetina, AssetBundleManager.SelectedDefaultVariant == "ipadretina");
			return true;
		}

		[MenuItem(kVariantIPAD, false ,33)]
		public static void ToggleiPAD ()
		{
			AssetBundleManager.SelectedDefaultVariant = "ipad"; 
		}

		[MenuItem(kVariantIPAD, true, 33)]
		public static bool ToggleiPADValidade ()
		{
			//Menu.SetChecked(kVariantIPAD, AssetBundleManager.SelectedDefaultVariant == "ipad");
			return true;
		}
			
		[InitializeOnLoad]
		class BuildOnPlay : object{
			static BuildOnPlay(){
				EditorApplication.playmodeStateChanged += PlayStateChanged;
			}

			static void PlayStateChanged(){
				if (!EditorApplication.isPlaying) {
					BuildScript.BuildAssetBundles();
				}
			}
		}
			
	}
}
