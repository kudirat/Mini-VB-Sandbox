using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIDVE.Utilities;

public class PressAny : MonoBehaviour
{
	public GameObject _nextScreen;
	
	public bool _isPC;
		
	[SerializeField]
	VBTesting.LessonSettings _currentSettings;
	
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		
		if(Input.GetMouseButtonDown(0) ||
			Input.GetMouseButtonDown(1) ||
			(OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) > 0.5f) ||
			(OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) > 0.5f) ||
			(OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger) > 0.5f) ||
			(OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger) > 0.5f))
			{
				/*AudioSource ac = GetComponent<AudioSource>();
				if(ac != null)
				{
					Debug.Log("Playing");
					ac.Play();
				}*/
				
				
				if(_isPC)
				{
					gameObject.transform.GetChild(0).gameObject.SetActive(false);
					gameObject.transform.GetChild(1).gameObject.SetActive(false);
					_nextScreen.SetActive(true);
					StartCoroutine(UnfreezeDelay(1f));
				}
				else
				{
					gameObject.SetActive(false);
					_nextScreen.SetActive(true);
				
				}
			}
    }
	
	IEnumerator UnfreezeDelay(float delay)
	{
		yield return new WaitForSeconds(delay);
		
		if(_isPC && _currentSettings != null)
		{
			_currentSettings.FreezePlayer(false);
		}
	}
}
