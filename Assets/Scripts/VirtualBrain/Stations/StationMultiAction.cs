using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIDVE.Utilities;

namespace VBTesting
{
    public class StationMultiAction : MonoBehaviour
    {
        [SerializeField]
        Station Station;

        [SerializeField]
        ButtonFloat Button;

		[SerializeField]
		int _numSignals = 3;
		
		bool _firstSignal = false;
		bool _secondSignal = false;
		bool _thirdSignal = false;
		bool _fourthSignal = false;
		bool _fifthSignal = false;
		bool _sixthSignal = false;
		
		public void SetFirstSignal(bool b)
		{
			_firstSignal = b;
		}
		
		public void SetSecondSignal(bool b)
		{
			_secondSignal = b;
		}
		
		public void SetThirdSignal(bool b)
		{
			_thirdSignal = b;
		}
		
		public void SetFourthSignal(bool b)
		{
			_fourthSignal = b;
		}
		
		public void SetFifthSignal(bool b)
		{
			_fifthSignal = b;
		}
		
		public void SetSixthSignal(bool b)
		{
			_sixthSignal = b;
		}
		
		bool AllSignalsSet()
		{
			if(_numSignals == 3)
			{
				return (_firstSignal && _secondSignal && _thirdSignal);
			}
			else if(_numSignals == 2)
			{
				return (_firstSignal && _secondSignal);
			}
			else if(_numSignals == 6)
			{
				return (_firstSignal && _secondSignal && _thirdSignal && _fourthSignal && _fifthSignal && _sixthSignal);
			}
			
			return false;
		}
		
		void ResetAllSignals()
		{
			if(_numSignals == 3)
			{
				_firstSignal = false;
				_secondSignal = false;
				_thirdSignal = false;
			}
			else if(_numSignals == 2)
			{
				_firstSignal = false;
				_secondSignal = false;
			}
			else if(_numSignals == 6)
			{
				_firstSignal = false;
				_secondSignal = false;
				_thirdSignal = false;
				_fourthSignal = false;
				_fifthSignal = false;
				_sixthSignal = false;
			}
		}
		
        void AdvanceTimeline()
		{
			//if(Station.IsPlaying)
			{
				Station.PlayNextTimeline();
				
			}
		}

		void Update()
		{
			if(!Station) return;
			
			if(!Button) return;

			if(AllSignalsSet())
			{
				AdvanceTimeline();
				ResetAllSignals();
			}
		}
	}
}