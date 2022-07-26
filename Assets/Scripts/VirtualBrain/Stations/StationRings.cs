using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIDVE.Utilities;

namespace VBTesting
{
	public class StationRings : MonoBehaviour
	{
		[SerializeField]
		Interpolator _interpolator;
		Interpolator Interpolator => _interpolator;

		[SerializeField]
		[Range(0, 5)]
		float _activationTime = 1f;
		float ActivationTime => _activationTime;

		[SerializeField]
		[Range(0, 5)]
		float _deactivationTime = 2f;
		float DeactivationTime => _deactivationTime;

		public void Activate()
		{
			Activate(ActivationTime);
		}

		public void Activate(float activationTime)
		{
			Interpolator.LerpTo1(activationTime);
		}

		public void Deactivate()
		{
			Deactivate(DeactivationTime);
		}

		public void Deactivate(float deactivationTime)
		{
			Interpolator.LerpTo0(deactivationTime);
		}
	}
}