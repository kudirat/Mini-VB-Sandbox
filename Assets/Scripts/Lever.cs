using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIDVE.Graphics;
using WIDVE.Utilities;
using WIDVE.Paths;

namespace VBTesting
{
    public class Lever : MonoBehaviour
    {
        [SerializeField]
        Interpolator _interpolator;
        Interpolator Interpolator => _interpolator;

        [SerializeField]
        [Range(0, 5)]
        float _activationTime = 3;
        float ActivationTime => _activationTime;

        [SerializeField]
        [Range(0, 5)]
        float _deactivationTime = 1.5f;
        float DeactivationTime => _deactivationTime;

        [SerializeField]
        PathPosition PathPosition;

        [SerializeField]
        [Tooltip("Height of lever above platform")]
        [Range(0, 2)]
        float LeverHeight = .9f;

        [SerializeField]
        GameObject Platform;

        [SerializeField]
        [HideInInspector]
        [Range(.001f, 100f)]
        float TravelSpeed = 5f;

        [SerializeField]
        Animator LeverAnimator;

        [SerializeField]
        GameObject ElevatorTrackPath;

        [SerializeField]
        ButtonVector2 _movementButton;
        public ButtonVector2 MovementButton
        {
            get { return _movementButton; }
            set { _movementButton = value; }
        }

        bool Active;
        bool forward;
        bool backward;

        public void Toggle()
        {
            if (Active) Deactivate();
            else Activate();
        }

        public void Toggle(float time)
        {
            if (Active) Deactivate(time);
            else Activate(time);
        }

        public void Activate()
        {
            Activate(ActivationTime);
        }

        public void Activate(float time)
        {
            Active = true;
            Interpolator.LerpTo1(time);
        }

        public void Deactivate()
        {
            if (!forward)
            {
                Deactivate((float) 0.02); // prevents the lever from appearing in the same position as the player
                // value is hard coded because the time needed to fade out before appearing in the player's position is independent of activation time
            }
            else { Deactivate(DeactivationTime); }
        }

        public void Deactivate(float time)
        {
            Interpolator.LerpTo0(time);
            Active = false;
            LeverAnimator.SetBool("forward", false);
            LeverAnimator.SetBool("backward", false);
        }


        void Start()
        {
            //set initial active state based on interpolator value
            Active = Mathf.Approximately(Interpolator.Value, 1);
        }

		void LateUpdate() 
        {
            //go to correct height
            Vector3 leverPosition = Platform.transform.position + (Vector3.up * LeverHeight);
            Vector3 pathPosition = Vector3.MoveTowards(transform.position, leverPosition, TravelSpeed);
            PathPosition.SetWorldPosition(pathPosition);
            transform.position = new Vector3(ElevatorTrackPath.transform.position.x, leverPosition.y, ElevatorTrackPath.transform.position.z); //to make it smooth - can remove lerp later

            //point at elevator
            Vector3 lookDirection = (leverPosition - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = lookRotation;

            //check if moving forwards
            forward = MovementButton.GetValue()[1] > 0.75 ? true : false;
            backward = MovementButton.GetValue()[1] < -0.75 ? true : false;

            // if forward changes, change the boolean on the animator controller, but only when it is active
            if (Active)
            {
                LeverAnimator.SetBool("forward", forward);
                LeverAnimator.SetBool("backward", backward);
            }
        }
	}
}