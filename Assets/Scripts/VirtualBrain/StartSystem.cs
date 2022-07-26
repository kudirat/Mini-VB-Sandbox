using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VBTesting
{
    public class StartSystem : MonoBehaviour
    {
		//script that runs when a system is first loaded

		[SerializeField]
		ClosedCaptions _handCaptions;
		ClosedCaptions HandCaptions => _handCaptions;

		void LoadSystem()
		{

		}

		void SetupCaptions()
		{
			//captions should be hidden at start
			HandCaptions.FadeOut(0);
		}

		void Start()
		{
			
		}
	}
}