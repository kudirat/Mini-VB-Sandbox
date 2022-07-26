using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowUserMessage : MonoBehaviour
{
    // Start is called before the first frame update
	Camera _mainCamera = null;
	UserMessage _userMessage = null;
	
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	public void ShowTurnAroundMessage()
	{
		if(_mainCamera == null)
		{
			_mainCamera = Camera.main;
			_userMessage = _mainCamera.gameObject.transform.GetChild(0).gameObject.GetComponent<UserMessage>();
		}
		
		if(_userMessage != null)
		{
			_userMessage.StartShowMessage("Please turn around");
		}
	}
}
