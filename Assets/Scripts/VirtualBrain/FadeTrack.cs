using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeTrack : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	public void DoFade()
	{
		MeshRenderer mr = GetComponent<MeshRenderer>();
		if(mr != null)
		{
			Material m = mr.sharedMaterial;
			if(m != null)
			{
				m.SetFloat("_FadeoutDistance", 15.0f);
				Debug.Log("Do fade");
			}
		}
	}
	
	public void StopFade()
	{
		MeshRenderer mr = GetComponent<MeshRenderer>();
		if(mr != null)
		{
			Material m = mr.sharedMaterial;
			if(m != null)
			{
				m.SetFloat("_FadeoutDistance", 0.75f);
				Debug.Log("Stop fade");
			}
		}
	}
}
