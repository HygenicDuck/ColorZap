using UnityEngine;
using Utils;
using AssetBundles;

public class AudioPlayer: MonoSingleton<AudioPlayer>
{
	GameObject m_audioManagerObject;
	AudioManager m_audioManager;

	protected override void Init()
	{
		GameObject itemPrefab = (GameObject)AssetBundleManager.LoadAsset("prefabs", "AudioManager");
		m_audioManagerObject = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity) as GameObject;
		m_audioManagerObject.transform.SetParent(gameObject.transform, false);
		m_audioManagerObject.name = "AudioManager";
		m_audioManagerObject.SetActive(true);
		m_audioManager = m_audioManagerObject.GetComponent<AudioManager>();
		m_audioManager.Initialise();
	}

}
