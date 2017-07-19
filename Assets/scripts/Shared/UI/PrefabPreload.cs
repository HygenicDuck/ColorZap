using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UI;
using Kanga;
using Facebook.Unity;
using System;
using System.Collections.Generic;
using AssetBundles;

namespace WaffleUI
{
	public class PrefabPreload : Core.BaseBehaviour
	{
		private const string PREFAB_ASSET_BUNDLE_NAME = "prefabs";

		[SerializeField] private List<string> m_preloadPrefabs;
		[SerializeField] bool m_launchOnAwake = true;

		protected override void Awake()
		{
			base.Awake();

			if (m_launchOnAwake)
			{
				Launch();
			}
		}

		public void Launch()
		{
			#if UNITY_ANDROID
			foreach (string prefabName in m_preloadPrefabs)
			{
				AssetBundleManager.LoadAssetAsync(PREFAB_ASSET_BUNDLE_NAME, prefabName, null);
			}
			#endif
		}
	}
}
