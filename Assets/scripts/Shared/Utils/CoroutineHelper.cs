using UnityEngine;
using System.Collections;

namespace Utils
{
	/// <summary>
	/// Coroutine helper.
	/// This class needs to inherit from MonoSingleton as it requires the StartCoroutine function (from Monobehaviour).
	/// Other classes which require StartCoroutine but dont inherit from Monobehaviour can therefore use this helper class.
	/// </summary>
	public class CoroutineHelper : MonoSingleton<CoroutineHelper>
	{
		protected override void Init()
		{
		}

		public void Run(IEnumerator enumerator)
		{
			this.StartCoroutine(enumerator);
		}

        public void Stop(IEnumerator enumerator)
        {
            this.StopCoroutine(enumerator);
        }

    }
}
