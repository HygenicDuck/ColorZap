using UnityEngine;
using System.Collections;

public class AnimationChain : Core.BaseBehaviour
{

	[System.Serializable]
	public struct AnimatableElement
	{
		public UI.UIAnimator Animator;
		public string Animation;
	}
	[SerializeField]
	private AnimatableElement[] m_animatables;

	public void PlayAnimationFromChainList(int i)
	{
		if (i < m_animatables.Length)
		{ 
			AnimatableElement element = m_animatables[i];
			if (element.Animator.gameObject.activeInHierarchy)
			{
				element.Animator.PlayAnimation(element.Animation);
			}
		}
	}
}
