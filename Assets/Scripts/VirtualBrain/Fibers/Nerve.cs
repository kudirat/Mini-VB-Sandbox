using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using WIDVE.Graphics;
using WIDVE.Paths;
using WIDVE.Patterns;
using WIDVE.Utilities;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VBTesting
{
	[ExecuteAlways]
	public class Nerve : MonoBehaviour, IObserver<PathCreator>
	{
		[Header("Nerve Components")]

		[SerializeField]
		PathCreator _path;
		PathCreator Path => _path;

		[SerializeField]
		bool _reversePath = false;
		bool ReversePath => _reversePath;

		[SerializeField]
		PathSpacer _layout;
		PathSpacer Layout => _layout;

		[SerializeField]
		PathTrigger _trigger;
		PathTrigger Trigger => _trigger;

		[SerializeField]
		Interpolator _triggerInterpolator;
		Interpolator TriggerInterpolator => _triggerInterpolator;

		[Header("Signal Settings")]

		[SerializeField]
		GameObject _signalPrefab;
		GameObject SignalPrefab => _signalPrefab;

		[SerializeField]
		GameObject _gapPrefab;
		GameObject GapPrefab => _gapPrefab;

		[SerializeField]
		[Range(0, 50)]
		float _signalDistance = .1f;
		float SignalDistance => _signalDistance;

		[SerializeField]
		Vector3 _signalScale = Vector3.one;
		Vector3 SignalScale => _signalScale;

		[SerializeField]
		[Range(0, 50)]
		float _signalSpeed = 30f;
		float SignalSpeed => _signalSpeed;

		[SerializeField]
		Color _signalColor = Color.white;
		Color SignalColor => _signalColor;
		
		[SerializeField]
		bool _doThreeTimes = false;
		bool DoThreeTimes => _doThreeTimes;

		int _playCount = 0;
		bool _wasActivated = false;
		
		void ConstructNerve(bool force=false)
		{
			if(!Path) return;
			if(!Layout) return;
			if(!SignalPrefab) return;
			if(!GapPrefab) return;
			if(Mathf.Approximately(SignalDistance, 0f) || SignalDistance < 0f) return;

			//turn off nerve object to stop reating signals
			if(!gameObject.activeSelf) return;

			//don't make signals in a prefab asset
			if(!gameObject.ExistsInScene()) return;

			//calculate how many signals to create
			int numSignals;
			int numObjects = Mathf.FloorToInt(Path.path.length / SignalDistance);
			if(numObjects > 2) numSignals = numObjects - 2;
			else numSignals = 0;

			//check if anything needs to be updated
			int currentObjects = transform.CountChildrenWithName(SignalPrefab.name, GapPrefab.name);
			if(force || currentObjects != numObjects)
			{
				//delete old signals
				transform.DeleteChildrenWithName(SignalPrefab.name, GapPrefab.name);

				//create new signals (with gaps at each end)
				List<GameObject> signals = new List<GameObject>();
				if(numSignals > 0)
				{
					//add start gap
					signals.Add(gameObject.InstantiatePrefab(GapPrefab) as GameObject);

					//add signals
					for(int i = 0; i < numSignals; i++)
					{
						//create signal and set it up
						GameObject newSignal = gameObject.InstantiatePrefab(SignalPrefab) as GameObject;
						
						//set signal color
						PerRendererColor prc = newSignal.GetComponentInChildren<PerRendererColor>();
						if(prc)
						{
							prc.Color = SignalColor;
							prc.SetColor();
						}

						//when setup is done, add it to the list
						signals.Add(newSignal);
					}

					//add end gap
					signals.Add(gameObject.InstantiatePrefab(GapPrefab) as GameObject);
				}

				foreach(GameObject go in signals)
				{
					//set signal scale
					go.transform.localScale = SignalScale;

					//give objects the nerve path
					PathPosition pathPosition = go.GetComponent<PathPosition>();
					if(pathPosition) pathPosition.SetPath(Path);
				}

				//update path layout
				Layout.LayoutObjects();

				//update path event sequence
				PathObjectSequence sequence = PathObjectSequence.FindFromPath(Path);
				sequence.Sort();
			}
		}

		public void Activate()
		{
			Trigger.ReturnToStart();//ReversePath);
			if(!ReversePath)
			{
				//lerp from start to end
				TriggerInterpolator.SetRawValue(0, notify: false);
				TriggerInterpolator.LerpTo1(Path.path.length / SignalSpeed);
			}
			else
			{
				//loop from end to start
				TriggerInterpolator.SetRawValue(1, notify: false);
				TriggerInterpolator.LerpTo0(Path.path.length / SignalSpeed);
			}
			
			_wasActivated = true;
		}

		void Update()
		{
			if(DoThreeTimes)
			{
				if(_wasActivated && !TriggerInterpolator.LerpActive() && _playCount < 2)
				{
					Activate();
					_playCount++;
				}
			}
		}
		
		[ContextMenu("Clear Signals")]
		void ClearSignals()
		{
			for(int i = transform.childCount - 1; i >= 0; i--)
			{
				DestroyImmediate(transform.GetChild(i).gameObject);
			}
		}

		public void OnNotify()
		{
			ConstructNerve();
		}

#if UNITY_EDITOR
		[CanEditMultipleObjects]
		[CustomEditor(typeof(Nerve))]
		class Editor : UnityEditor.Editor
		{
			void ConstructNerves()
			{
				foreach(Nerve n in targets)
				{
					n.ConstructNerve(true);
				}
			}

			void OnEnable()
			{
				Undo.undoRedoPerformed += ConstructNerves;
			}

			public override void OnInspectorGUI()
			{
				EditorGUI.BeginChangeCheck();

				base.OnInspectorGUI();

				bool changed = EditorGUI.EndChangeCheck();

				bool toggleGUI = !(target as Nerve).gameObject.ExistsInScene();
				if(toggleGUI) GUI.enabled = false;

				if(GUILayout.Button("Construct Nerve") || changed)
				{
					ConstructNerves();
				}

				if(toggleGUI) GUI.enabled = true;

				if(Application.isPlaying)
				{
					if(GUILayout.Button("Activate"))
					{
						foreach(Nerve n in targets)
						{
							n.Activate();
						}
					}
				}
			}

			void OnDisable()
			{
				Undo.undoRedoPerformed -= ConstructNerves;
			}
		}
#endif
	}
}