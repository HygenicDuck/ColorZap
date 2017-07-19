using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class RectTransformExtension
{

	private const float DISTANCE_PLANE_TO_CAMERA = 10f;

	/// <summary>
	/// EXTENSION- Return the transforms size based in the rect size.
	/// </summary>
	public static Vector2 GetSize(this RectTransform obj)
	{
		Rect rect = obj.rect;
		return rect.size;
	}

	public static RectTransform GetRectTransform(this RectTransform obj)
	{
		return (RectTransform)obj;
	}

	public static bool IsTouchInTransform(this RectTransform obj)
	{
		Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		position.z = DISTANCE_PLANE_TO_CAMERA;

		Vector3 localPosition = position - obj.position;

		localPosition.x = localPosition.x / obj.lossyScale.x;
		localPosition.y = localPosition.y / obj.lossyScale.y;

		Vector2 size = obj.GetSize();

		localPosition.y = -localPosition.y;

		localPosition.x += size.x * 0.5f;
		localPosition.y += size.y * 0.5f;

		localPosition.x /= size.x;
		localPosition.y /= size.y;

		if (localPosition.x > 1f || localPosition.x < 0f || localPosition.y > 1f || localPosition.y < 0f)
		{
			return false;
		}

		return true;
	}

	public static Vector3 GetLocalTouchPosition(this RectTransform obj)
	{
		Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		position.z = DISTANCE_PLANE_TO_CAMERA;

		Vector3 localPosition = position - obj.position;

		localPosition.x = localPosition.x / obj.lossyScale.x;
		localPosition.y = localPosition.y / obj.lossyScale.y;

		Vector2 size = obj.GetSize();

		localPosition.x += size.x * 0.5f;
		localPosition.y += size.y * 0.5f;

		return localPosition;
	}

	public static void FitTextureToParent(this RectTransform obj, Texture texture)
	{
		if (texture == null)
		{
			return;
		}

		RectTransform parent = (RectTransform)obj.parent;
		Vector2 parentSize = parent.GetSize();
		Vector2 size = new Vector2 (texture.width, texture.height);

		parentSize = obj.localRotation * parentSize;
		parentSize.x = Mathf.Abs(parentSize.x);
		parentSize.y = Mathf.Abs(parentSize.y);

		if (size.x > size.y)
		{
			float aspect = size.x / size.y;
			float parentAspect = parentSize.x / parentSize.y;

			if (parentAspect <= aspect)
			{
				obj.FitTextureToParentHeight(texture);
			}
			else
			{
				obj.FitTextureToParentWidth(texture);
			}
		}
		else
		{
			float aspect = size.y / size.x;
			float parentAspect = parentSize.y / parentSize.x;

			if (parentAspect <= aspect)
			{
				obj.FitTextureToParentWidth(texture);
			}
			else
			{
				obj.FitTextureToParentHeight(texture);
			}
		}
	}

	public static void FitTextureToParentHeight(this RectTransform obj, Texture texture)
	{
		RectTransform parent = (RectTransform)obj.parent;
		Vector2 parentSize = parent.GetSize();
		Vector2 size = new Vector2 (texture.width, texture.height);

		parentSize = obj.localRotation * parentSize;
		parentSize.x = Mathf.Abs(parentSize.x);
		parentSize.y = Mathf.Abs(parentSize.y);

		float aspectRatio = size.x / size.y;

		obj.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, parentSize.y);
		obj.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, parentSize.y * aspectRatio);
	}

	public static void FitTextureToParentWidth(this RectTransform obj, Texture texture)
	{
		RectTransform parent = (RectTransform)obj.parent;
		Vector2 parentSize = parent.GetSize();
		Vector2 size = new Vector2 (texture.width, texture.height);

		parentSize = obj.localRotation * parentSize;
		parentSize.x = Mathf.Abs(parentSize.x);
		parentSize.y = Mathf.Abs(parentSize.y);

		float aspectRatio = size.y / size.x;

		obj.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, parentSize.x);
		obj.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, parentSize.x * aspectRatio);
	}

	public static RectTransform GetRectTransform(this MonoBehaviour obj)
	{
		return (RectTransform)obj.transform;
	}

	public static RectTransform GetRectTransform(this GameObject obj)
	{
		return (RectTransform)obj.transform;
	}

	public static Transform FindChildByName(this Transform parent, string name, bool includeInactive = false)
	{
		if ((parent.gameObject.activeInHierarchy || includeInactive) && parent.name.Equals(name))
		{
			return parent;
		}
		foreach (Transform child in parent)
		{
			Transform result = FindChildByName(child, name, includeInactive);
			if (result != null)
				return result;
		}
		return null;
	}

	public static List<Transform> FindAllChildrenByName(this Transform parent, string name, bool includeInactive = false)
	{
		List<Transform> ret = new List<Transform> ();
		FindAllChildrenByNameRecurse(ret, parent, name, includeInactive);
		return ret;
	}

	private static void FindAllChildrenByNameRecurse(List<Transform> children, Transform parent, string name, bool includeInactive = false)
	{
		if ((parent.gameObject.activeInHierarchy || includeInactive) && parent.name.Equals(name))
		{
			children.Add(parent);
		}

		foreach (Transform child in parent)
		{
			FindAllChildrenByNameRecurse(children, child, name, includeInactive);
		}
	}

	public static Transform FindTopChildByName(this Transform parent, string name, bool includeInactive = false)
	{
		if ((parent.gameObject.activeInHierarchy || includeInactive) && parent.name.Equals(name))
		{
			return parent;
		}

		Transform result = null;
		foreach (Transform child in parent)
		{
			Transform temp = FindTopChildByName(child, name, includeInactive);
			if (temp != null)
			{
				if (result == null || temp.transform.position.y > result.transform.position.y)
				{
					result = temp;
				}
			}
		}
		return result;
	}
}
