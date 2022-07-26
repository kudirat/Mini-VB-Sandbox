using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIDVE.Paths;
using WIDVE.Utilities;

namespace VBTesting
{
    [ExecuteAlways]
    public class ParticleNerve : MonoBehaviour
    {
        [SerializeField]
        PathTrigger _trigger;
        PathTrigger Trigger => _trigger;

        [SerializeField]
        ParticleSystem _system;
        ParticleSystem System => _system;

        [SerializeField]
        Vector3 _extraRotation = Vector3.zero;
        Vector3 ExtraRotation => _extraRotation;

        void LateUpdate()
        {
            if(!gameObject.ExistsInScene()) return;
            if(!Trigger) return;
            if(!System) return;

			//match particle system's rotation to the current rotation along path
            Vector3 rotationAngles = Trigger.transform.rotation.eulerAngles;
            ParticleSystem.MainModule main = System.main;
            main.startRotationX = Mathf.Deg2Rad * (rotationAngles.x + ExtraRotation.x);
            main.startRotationY = Mathf.Deg2Rad * (rotationAngles.y + ExtraRotation.y);
            main.startRotationZ = Mathf.Deg2Rad * (rotationAngles.z + ExtraRotation.z);
        }
	}
}