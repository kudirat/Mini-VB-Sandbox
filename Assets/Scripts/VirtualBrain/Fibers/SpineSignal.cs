using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpineSignal : MonoBehaviour
{
    [SerializeField]
    ParticleSystem ParticleSystem;

	[SerializeField]
	[Range(0, 10)]
	float TimeActive = 3;

    public void Play()
	{
		ParticleSystem.Play();
		StopAllCoroutines();
		StartCoroutine(StopParticleSystemAfterTime(TimeActive));
	}

    IEnumerator StopParticleSystemAfterTime(float time)
	{
		yield return new WaitForSeconds(Mathf.Max(time, 0));

		ParticleSystem.Stop();
	}
}
