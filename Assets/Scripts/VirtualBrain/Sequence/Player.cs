using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIDVE.Paths;
using WIDVE.Utilities;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VBTesting
{
	public class Player : MonoBehaviour
	{
		[SerializeField]
		LessonSettings _settings;
		public LessonSettings Settings => _settings;

		[SerializeField]
		PathTrigger _playerTrigger;
		public PathTrigger PlayerTrigger => _playerTrigger;

		[SerializeField]
		PlayerMovement _movement;
		public PlayerMovement Movement => _movement;

		[SerializeField]
		Transform _cameraParent;
		public Transform CameraParent => _cameraParent;

		[SerializeField]
		ClosedCaptionsMirror _handClosedCaptions;
		ClosedCaptionsMirror HandClosedCaptions => _handClosedCaptions;

		[SerializeField]
		[Range(0, 5)]
		float _captionsFadeTime = 1;
		float CaptionsFadeTime => _captionsFadeTime;

		public Station PlayingStation;
		
		[SerializeField]
		float _replayDistanceLimit = 2f;
		float ReplayDistanceLimit => _replayDistanceLimit;
		
		[SerializeField]
		float _ringFadeDistance = 0.2f;
		float RingFadeDistance => _ringFadeDistance;
		
		Vector3 _lastStationPosition = Vector3.zero;
		float _travelDist = 20f;
		float _replayDist = 20f;
		
		VBSystem _currentSystem;
		public VBSystem CurrentSystem
		{
			get => _currentSystem;
			private set => _currentSystem = value;
		}

		void FreezeMovement(bool freeze)
		{
			if(freeze) Movement.Freeze(CurrentSystem);
			else Movement.Unfreeze(CurrentSystem);
		}

		void ActivateStation(Station station)
		{
			//Ross:  6/23/2022 - adding a check here so that station doesn't restart (if guards off) unless user gets some distance away from the station
			//Debug.Log("ACTIVATING STATION: " + _replayDist);
			
			if(_replayDist > ReplayDistanceLimit)
			{
				if(station == PlayingStation)
				{
					//Debug.Log(_settings.StationMode.ToString());
					if(_settings.StationMode == LessonSettings.StationModes.Replayable)
					{
						if(PlayingStation)
						{
							PlayingStation.Stop();
						}

						if(HandClosedCaptions)
						{
							//setup closed captions on hand
							HandClosedCaptions.Captions.FadeIn(CaptionsFadeTime);
							HandClosedCaptions.SetMaster(station.ClosedCaptions);
						}
						
						//play the station
						station.Play();
						station.OnStop += ClearStation;
						PlayingStation = station;
					}
				}
				else
				{
					//stop current station
					if(PlayingStation)
					{
						PlayingStation.Stop();
					}

					if(HandClosedCaptions)
					{
						//setup closed captions on hand
						HandClosedCaptions.Captions.FadeIn(CaptionsFadeTime);
						HandClosedCaptions.SetMaster(station.ClosedCaptions);
					}
					
					
					//play the station
					station.Play();
					station.OnStop += ClearStation;
					PlayingStation = station;
				}
				
				_lastStationPosition = station.gameObject.transform.position;
				
				_replayDist = 0f;
				_travelDist = 0f;
			}
		}

		void ProcessPathObject(PathObject pathObject)
		{
			//activate stations
			Station station = pathObject.GetComponentInChildren<Station>();
			if(station) ActivateStation(station);
		}

		void ClearStation()
		{
			//Debug.Log("CLEARING STATION!");
			
			PlayingStation = null;

			if(HandClosedCaptions)
			{
				//fade captions out again
				HandClosedCaptions.Captions.FadeOut(CaptionsFadeTime);
			}
			
			//we set this to a big number (> ReplayDistanceLimit), so that we always play a next station...
			_replayDist = 20f;
			_travelDist = 0f;
		}

		public void SetSystem(VBSystem system)
		{
			CurrentSystem = system;
		}

		void OnEnable()
		{
			//subscribe to events 
			Settings.FreezePlayerEvent.Event += FreezeMovement;
			PlayerTrigger.OnTrigger += ProcessPathObject;
		}

		void OnDisable()
		{
			//unsubscribe from events
			Settings.FreezePlayerEvent.Event -= FreezeMovement;
			PlayerTrigger.OnTrigger -= ProcessPathObject;
		}
		
		void Update()
		{
			if(PlayingStation != null)
			{
				if(PlayingStation.IsPlaying)
				{
					float lastDist = Vector3.Distance(_lastStationPosition, transform.position);
					_travelDist = Mathf.Max(_travelDist, lastDist);
					_replayDist = Mathf.Max(_replayDist, lastDist);
					
					if(lastDist < RingFadeDistance)
					{
						PlayingStation.ActivateRings();
					}
					else if(_travelDist > RingFadeDistance)
					{
						PlayingStation.DeactivateRings();
					}

				}
				else
				{
					_replayDist = 20.0f;
					_travelDist = 0f;
				}
			}
		}

#if UNITY_EDITOR
		[CanEditMultipleObjects]
		[CustomEditor(typeof(Player))]
		class Editor : UnityEditor.Editor
		{
			public override void OnInspectorGUI()
			{
				base.OnInspectorGUI();
			}
		}
#endif
	}
}