using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using WIDVE.Utilities;
using WIDVE.Patterns;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VBTesting
{
	public class StationOrb : MonoBehaviour, IObserver<PathCreator>
	{
		enum OrbSides { Left, Right }

		[SerializeField]
		Interpolator _interpolator;
		Interpolator Interpolator => _interpolator;

		[SerializeField]
		[Range(0, 5)]
		float _activationTime = 1f;
		float ActivationTime => _activationTime;

		[SerializeField]
		[Range(0, 5)]
		float _deactivationTime = 2f;
		float DeactivationTime => _deactivationTime;

		[SerializeField]
		GameObject _orb;
		GameObject Orb => _orb;

		[SerializeField]
		TubeRenderer _connector;
		TubeRenderer Connector => _connector;

		[SerializeField]
		Vector2 _orbOffset = new Vector2(.5f, 1.2f);
		Vector2 OrbOffset => _orbOffset;

		[SerializeField]
		OrbSides _orbSide = OrbSides.Right;
		OrbSides OrbSide => _orbSide;

		public void Activate()
		{
			Activate(ActivationTime);
		}

		public void Activate(float activationTime)
		{
			Interpolator.LerpTo1(activationTime);
		}

		public void Deactivate()
		{
			Deactivate(DeactivationTime);
		}

		public void Deactivate(float deactivationTime)
		{
			Interpolator.LerpTo0(deactivationTime);
		}

		void PlaceOrb()
		{
			if(!Orb) return;
			if(!Connector) return;

			//adjust orb position - rotation is handled by the PathPosition component
			Vector3 orbOffset = OrbSide == OrbSides.Right ? OrbOffset : new Vector2(-OrbOffset.x, OrbOffset.y);
			Orb.transform.localPosition = orbOffset;

			//get points for the connector
			Vector3 pathPoint = transform.position;
			Vector3 orbPoint = Orb.transform.position;
			Vector3 cornerPoint = new Vector3(orbPoint.x, pathPoint.y, orbPoint.z);

			//set points on connector
			Vector3[] connectorPoints = { orbPoint, cornerPoint, pathPoint };
			for(int i = 0; i < connectorPoints.Length; i++)
			{
				connectorPoints[i] = Connector.transform.InverseTransformPoint(connectorPoints[i]);
			}

			Connector.points = connectorPoints;
			Connector.ForceUpdate();
#if UNITY_EDITOR
			EditorUtility.SetDirty(Connector);
#endif
		}

		public void OnNotify()
		{
			//update orb placement when path has updated
			PlaceOrb();
		}

#if UNITY_EDITOR
		[CanEditMultipleObjects]
		[CustomEditor(typeof(StationOrb))]
		class Editor : UnityEditor.Editor
		{
			public override void OnInspectorGUI()
			{
				EditorGUI.BeginChangeCheck();

				base.OnInspectorGUI();

				bool changed = EditorGUI.EndChangeCheck();

				if(changed)
				{
					//update orb position
					foreach(StationOrb so in targets)
					{
						so.PlaceOrb();
					}
				}
			}
		}
#endif
	}
}