using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ScaleWithParent : MonoBehaviour {

	[SerializeField] private float defaultParentHeight;

	// Use this for initialization
	void Update () {
		RectTransform rt = transform.parent.GetComponent<RectTransform>();
		float height = rt.rect.height;
		float scale = height / defaultParentHeight;
		transform.localScale = new Vector3(scale,scale,1f);
	}
}
