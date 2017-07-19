using UnityEngine;
using System.Collections;
using UnityEngine.UI;


namespace Utils
{
	public class RenderCamera : MonoSingleton<RenderCamera>
	{
		[SerializeField]
		private Camera m_CameraRenderer;

		[SerializeField]
		private RawImage m_rawImage;

		[SerializeField]
		private Canvas m_canvas;

		public int outputHeight { get; set; }
		public int outputWidth { get; set; }

		private float cameraHeight;
		private float cameraWidth;

		private GameObject m_gameobjectClone;

		private RenderTexture m_renderTexture;

		const float targetWidth = 1242f;
		const float targetHeight = 2208f;
		const float canvasRoundSize = 5f;

		protected override void Init()
		{
			// Default render size (matches puzzle images size)
			outputWidth = 640;
			outputHeight = 556;

			cameraHeight = m_CameraRenderer.pixelHeight;
			cameraWidth = m_CameraRenderer.pixelWidth;
			m_CameraRenderer.enabled = false;
			m_rawImage.enabled = false;
		}

		public void UpdateSize()
		{
			m_rawImage.GetComponent<RectTransform>().sizeDelta = new Vector2 (outputWidth, outputHeight);
			m_CameraRenderer.orthographicSize = (float)(outputHeight / 2.0);
			float cameraHeightRect = (float)((float)outputHeight / cameraHeight);
			float cameraWidthRect = (float)((float)outputWidth / cameraWidth);

			m_CameraRenderer.rect = new Rect (0.0f, 0.0f, cameraWidthRect, cameraHeightRect);
		}

		public void RestoreSize()
		{
			m_CameraRenderer.rect = new Rect (0.0f, 0.0f, 1.0f, 1.0f);
		}

		public void SetRenderTexture(RenderTexture renderTexture)
		{
			m_CameraRenderer.targetTexture = renderTexture;
		}

		public void SetTexture(Texture rawImageTexture)
		{
			m_rawImage.texture = rawImageTexture;
		}

		public void SetGameObject(GameObject clone)
		{
			SetGameObject(clone, false);
		}

		public void SetGameObject(GameObject clone, bool resizeCanvas)
		{
			if (resizeCanvas)
			{
				m_canvas.scaleFactor = (outputWidth / targetWidth) / (clone.GetRectTransform().rect.width / (targetWidth + canvasRoundSize));
			}
			m_gameobjectClone = clone;
			m_gameobjectClone.transform.SetParent(m_canvas.transform, false);
		}

		public IEnumerator SetupCapture(int width, int height)
		{
			m_renderTexture = new RenderTexture (width, height, 24);

			outputWidth = width;
			outputHeight = height;
			SetRenderTexture(m_renderTexture);

			yield return new WaitForEndOfFrame ();
		}

		public Texture2D RetrieveTexture()
		{
			m_CameraRenderer.Render();
			RenderTexture.active = m_renderTexture;

			Texture2D outputTexture = new Texture2D (outputWidth, outputHeight, TextureFormat.RGB24, false);
			outputTexture.ReadPixels(new Rect (0, 0, outputWidth, outputHeight), 0, 0); // you get the center section

			outputTexture.Apply();

			// Cleanup
			RenderTexture.active = null;
			RenderTexture.Destroy(m_renderTexture);
			m_renderTexture = null;

			m_CameraRenderer.targetTexture = null;
			m_CameraRenderer.enabled = false;

			m_gameobjectClone.transform.SetParent(null, false);
			m_gameobjectClone = null;

			return outputTexture;
		}
	}
}
