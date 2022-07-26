using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIDVE.Graphics;
using WIDVE.Utilities;

namespace VBTesting
{
    public class Elevator : MonoBehaviour
    {
        [SerializeField]
        Interpolator _interpolator;
        Interpolator Interpolator => _interpolator;

        [SerializeField]
        [Range(0, 5)]
        float _activationTime = 3;
        float ActivationTime => _activationTime;

        [SerializeField]
        [Range(0, 5)]
        float _deactivationTime = 1.5f;
        float DeactivationTime => _deactivationTime;

        bool Active;

        public void Toggle()
        {
            if(Active) Deactivate();
            else Activate();
        }

        public void Toggle(float time)
		{
            if(Active) Deactivate(time);
            else Activate(time);
        }

        public void Activate()
		{
            Activate(ActivationTime);
		}

        public void Activate(float time)
		{
            Active = true;
            Interpolator.LerpTo1(time);
		}

        public void Deactivate()
		{
            Deactivate(DeactivationTime);
		}

        public void Deactivate(float time)
		{
            Active = false;
            Interpolator.LerpTo0(time);
		}

		void Start()
		{
            //set initial active state based on interpolator value
            Active = Mathf.Approximately(Interpolator.Value, 1);
		}
	}
}