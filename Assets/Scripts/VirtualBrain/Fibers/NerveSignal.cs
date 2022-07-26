using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIDVE.Utilities;

namespace VBTesting
{
	public class NerveSignal : MonoBehaviour
	{
		[SerializeField]
		Interpolator _interpolator;
		Interpolator Interpolator => _interpolator;

		[SerializeField]
		[Range(0, 1)]
		float _attack = .05f;
		float Attack => _attack;

		[SerializeField]
		[Range(0, 1)]
		float _hold = .05f;
		float Hold => _hold;

		[SerializeField]
		[Range(0, 1)]
		float _decay = .1f;
		float Decay => _decay;

		public void Activate()
		{
			StopAllCoroutines();

			Interpolator.LerpTo1(Attack);

			StartCoroutine(DeactivateAfterWaiting());
		}

		IEnumerator DeactivateAfterWaiting()
		{
			//wait for the attack and hold times to complete
			yield return new WaitForSeconds(Attack + Hold);

			//deactivate after the wait is done
			Interpolator.LerpTo0(Decay);
		}
	}
}