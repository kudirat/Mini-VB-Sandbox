using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIDVE.Utilities;

namespace VBTesting
{
    public class StationAction : MonoBehaviour
    {
        [SerializeField]
        Station Station;

        [SerializeField]
        ButtonFloat Button;

        void AdvanceTimeline()
		{
			if(Station.IsPlaying)
			{
				Station.PlayNextTimeline();
			}
		}

		void Update()
		{
			if(!Station) return;
			if(!Button) return;

			if(Button.GetUp()) AdvanceTimeline();
		}
	}
}