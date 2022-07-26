using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIDVE.Utilities;
using WIDVE.Paths;
using WIDVE.Patterns;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CheckMovement : MonoBehaviour
{
	[SerializeField]
	ButtonVector2 _movementButton;
	public ButtonVector2 MovementButton
	{
		get { return _movementButton; }
		set { _movementButton = value; }
	}
	
	[SerializeField]
	VBTesting.PlayerMovement _movement;
	
	public VBTesting.PlayerMovement Movement => _movement;
	
	[SerializeField]
	UserMessage _message;
	
	float _timeSinceEntered = 0f;
	
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		if(Movement.CurrentPlayerState() != "VBTesting.PlayerMovement+States+FrozenTrack")
		{
			_timeSinceEntered = 0f;
		}
		else
		{
			_timeSinceEntered += Time.deltaTime;
		}
		
        if(MovementButton && Movement && Movement.CurrentPlayerState() == "VBTesting.PlayerMovement+States+FrozenTrack")
        {
			//don't display unless we've been in the station for > 3 seconds.
			if(MovementButton.GetValue()[1] != 0.0f && _timeSinceEntered > 3f)
			{
				_message.StartShowMessage("You can move after the station has ended"); 
			}
		}
    }
}
