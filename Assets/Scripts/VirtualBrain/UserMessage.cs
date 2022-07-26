using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserMessage : MonoBehaviour
{
	bool _showingMessage = false;
	
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	public void StartShowMessage(string message)
	{
		if(!_showingMessage)
		{
			_showingMessage = true;
			StartCoroutine(ShowMessage(message, 2f));
		}
	}
	
	IEnumerator ShowMessage(string message, float duration)
	{
		transform.GetChild(0).GetComponent<TMPro.TextMeshPro>().text = message;
		
		yield return new WaitForSeconds(duration);
		
		//clear text 
		transform.GetChild(0).GetComponent<TMPro.TextMeshPro>().text = " ";
		
		_showingMessage = false;
	}
}
