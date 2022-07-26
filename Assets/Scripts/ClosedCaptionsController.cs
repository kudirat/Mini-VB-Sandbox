using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIDVE.Utilities;

namespace VBTesting
{
    public class ClosedCaptionsController : MonoBehaviour
    {
        [SerializeField]
        ClosedCaptions _captions;
        ClosedCaptions Captions => _captions;

        [SerializeField]
        ButtonFloat _nextButton;
        ButtonFloat NextButton => _nextButton;

        [SerializeField]
        ButtonFloat _previousButton;
        ButtonFloat PreviousButton => _previousButton;

        private void Update()
        {
            if(NextButton && NextButton.GetDown())
            {
                Captions.DisplayNextCaption();
            }

            if(PreviousButton && PreviousButton.GetDown())
            {
                Captions.DisplayPreviousCaption();
            }
        }
    }
}