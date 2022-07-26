using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIDVE.Utilities;

namespace VBTesting
{
	public class FreeMovement : MonoBehaviour
	{
		[SerializeField]
		Transform _mover;
		Transform Mover => _mover;

		[SerializeField]
		Transform _orientation;
		Transform Orientation => _orientation;

		[SerializeField]
		[Range(0, 100)]
		float _speed = 10f;
		float Speed => _speed;

		[SerializeField]
		ButtonVector2 _button;
		ButtonVector2 Button => _button;

		void Move()
		{
			if(!Mover) return;
			if(!Orientation) return;
			if(Mathf.Approximately(Speed, 0f)) return;
			if(!Button) return;

			Vector2 distance = Button.GetValue() * Speed * Time.deltaTime;
			Vector3 forwardDistance = Orientation.forward * distance.y;
			Vector3 rightDistance = Orientation.right * distance.x;
			Vector3 totalDistance = forwardDistance + rightDistance;

			Mover.position += totalDistance;
		}

		void Update()
		{
			Move();
		}
	}
}