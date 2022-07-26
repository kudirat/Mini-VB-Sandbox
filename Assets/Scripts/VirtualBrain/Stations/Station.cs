using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using WIDVE.Patterns;
using WIDVE.Utilities;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VBTesting
{
	public class Station : MonoBehaviour, ISelectable
	{
		[Header("Settings")]

		[SerializeField]
		LessonSettings _settings;
		public LessonSettings Settings
		{
			get => _settings;
			set => _settings = value;
		}

		[Header("Station Components")]

		[SerializeField]
		StationRings _rings;
		StationRings Rings => _rings;

		[SerializeField]
		StationOrb _orb;
		StationOrb Orb => _orb;

		[SerializeField]
		PlayableDirector _playableDirector;
		PlayableDirector PlayableDirector => _playableDirector;

		[SerializeField]
		List<PlayableDirector> _additionalTimelines;
		List<PlayableDirector> AdditionalTimelines => _additionalTimelines ?? (_additionalTimelines = new List<PlayableDirector>());

		[Header("Audio Components")]

		[SerializeField]
		AudioSource _audioSource;
		AudioSource AudioSource => _audioSource;

		[SerializeField]
		ClosedCaptions _closedCaptions;
		public ClosedCaptions ClosedCaptions => _closedCaptions;

		[SerializeField]
		ClosedCaptionsController _closedCaptionsController;
		ClosedCaptionsController ClosedCaptionsController => _closedCaptionsController;

		[SerializeField]
		SignalReceiver _closedCaptionsSignalReceiver;
		SignalReceiver ClosedCaptionsSignalReceiver => _closedCaptionsSignalReceiver;

		int CurrentTimelineIndex = 0;

		public PlayableDirector CurrentTimeline
		{
			get
			{
				//timeline 0 is the default timeline
				if(CurrentTimelineIndex <= 0) return PlayableDirector;

				//additional timelines are 1 through n
				else return AdditionalTimelines[CurrentTimelineIndex - 1];
			}
		}

		FiniteStateMachine _fsm;
		StateMachine SM => _fsm ?? (_fsm = new FiniteStateMachine(new States.Playable(this, null, false)));

		public bool IsPlaying => SM.CurrentState is States.Playing;
		public bool IsPaused => SM.CurrentState is States.Paused;
		public bool IsPlayable => SM.CurrentState is States.Playable;

		public event System.Action OnPlay;

		public event System.Action OnPause;

		public event System.Action OnUnpause;

		public event System.Action OnStop;

		public void Play()
		{
			Play(Settings);
		}

		public void Play(LessonSettings settings)
		{
			bool canPlay = !(SM.CurrentState is States.NotPlayable);
			if(canPlay)
			{
				SM.SetState(new States.Playing(this, settings));
			}
		}

		public void Pause()
		{
			Pause(Settings);
		}

		public void Pause(LessonSettings settings)
		{
			if(SM.CurrentState is States.Playing)
			{
				SM.SetState(new States.Paused(this, settings));
			}
		}

		public void Stop()
		{
			Stop(Settings, true, false);
		}

		public void Stop(LessonSettings settings)
		{
			Stop(settings, true, false);
		}

		void Stop(LessonSettings settings, bool notifyOnStop, bool remainPlayable)
		{
			//stations are replayable if the settings say they are or if the override bool is true
			bool replayable = (settings.StationMode == LessonSettings.StationModes.Replayable) | remainPlayable;
			if(replayable)
			{
				SM.SetState(new States.Playable(this, settings, notifyOnStop));
			}
			else
			{
				SM.SetState(new States.NotPlayable(this, settings, notifyOnStop));
			}

			//reset current timeline to the first timeline
			CurrentTimelineIndex = 0;
		}

		public void SwitchToNextTimeline()
		{
			SwitchToNextTimeline(Settings);
		}

		public void SwitchToNextTimeline(LessonSettings settings)
		{
			//save current timeline index
			int currentTimelineIndex = CurrentTimelineIndex;

			//stop the current timeline without triggering the OnStop event
			Stop(settings, false, true);

			//switch to the next timeline
			CurrentTimelineIndex = currentTimelineIndex + 1;
		}

		public void SwitchToPreviousTimeline()
		{
			SwitchToPreviousTimeline(Settings);
		}

		public void SwitchToPreviousTimeline(LessonSettings settings)
		{
			//save current timeline index
			int currentTimelineIndex = CurrentTimelineIndex;

			//stop the current timeline without triggering the OnStop event
			Stop(settings, false, true);

			//switch to the previous timeline
			CurrentTimelineIndex = Mathf.Max(currentTimelineIndex - 1, 0);
		}

		public void PlayNextTimeline()
		{
			PlayNextTimeline(Settings);
		}

		public void PlayNextTimeline(LessonSettings settings)
		{
			SwitchToNextTimeline(settings);
			Play(settings);
		}

		public void PlayPreviousTimeline()
		{
			PlayPreviousTimeline(Settings);
		}

		public void PlayPreviousTimeline(LessonSettings settings)
		{
			SwitchToPreviousTimeline(settings);
			Play(settings);
		}

		public void Select(Selector selector)
		{
			Play();
		}

		public void UpdateSettings()
		{
			if(IsPlaying || IsPaused)
			{
				//Debug.Log("UPDATING SETTINGS");
				//check the current settings and update rings/movement
				if(Settings.StopAtStations)
				{
					//activate rings
					Rings.Activate();
					//freeze player
					Settings.FreezePlayerEvent.Invoke(true);
				}
				else
				{
					//deactivate rings
					Rings.Deactivate();
					//unfreeze player
					Settings.FreezePlayerEvent.Invoke(false);
				}
			}
			//if not playing/paused, don't need to do anything
		}
		
		public void DeactivateRings()
		{
			if(Rings != null)
			{
				Rings.Deactivate();
			}
		}
		
		public void ActivateRings()
		{
			if(Rings != null)
			{
				Rings.Activate();
			}
		}
		
		void OnEnable()
		{
			Settings.StationSettingsChangedEvent.Event += UpdateSettings;
		}

		void Start()
		{
			//hide rings and captions until activated
			Rings.Deactivate(0);
			ClosedCaptions.ShowText(false);
		}

		void OnDisable()
		{
			Stop();
			Settings.StationSettingsChangedEvent.Event -= UpdateSettings;
		}

		class States
		{
			public abstract class StationState : State<Station>
			{
				protected readonly LessonSettings Settings;

				public StationState(Station target, LessonSettings settings) : base(target)
				{
					Settings = settings;
				}

				protected void EnterStation()
				{
					//activate rings
					Target.Rings.Activate();
					
					if(Settings.StopAtStations)
					{
						//freeze player
						Settings.FreezePlayerEvent.Invoke(true);
					}

					SetupClosedCaptions(true, Settings.ClosedCaptionsMode, Settings.ClosedCaptionsDisplayMode);
				}

				protected void ExitStation()
				{
					//deactivate rings
					Target.Rings.Deactivate();

					//unfreeze player
					Settings.FreezePlayerEvent.Invoke(false);

					SetupClosedCaptions(false, Settings.ClosedCaptionsMode, Settings.ClosedCaptionsDisplayMode);
				}

				protected void SetupClosedCaptions(bool active, LessonSettings.ClosedCaptionsModes mode,
																LessonSettings.ClosedCaptionsDisplayModes displayMode)
				{
					switch(mode)
					{
						case LessonSettings.ClosedCaptionsModes.Manual:
							//enable manual closed captions controller
							Target.ClosedCaptionsController.enabled = true;
							//disable closed captions signal receiver
							Target.ClosedCaptionsSignalReceiver.enabled = false;
							//turn closed captions on or off
							Target.ClosedCaptions.ShowText(active);
							break;

						default:
						case LessonSettings.ClosedCaptionsModes.SyncedToAudio:
							//disable manual closed captions controller
							Target.ClosedCaptionsController.enabled = false;
							//enable closed captions signal receiver
							Target.ClosedCaptionsSignalReceiver.enabled = true;
							//turn closed captions on or off
							Target.ClosedCaptions.ShowText(active);
							break;
					}

					switch(displayMode)
					{
						default: //todo later
						case LessonSettings.ClosedCaptionsDisplayModes.Disabled:
							Target.ClosedCaptions.ShowText(false);
							break;
					}
				}
			}

			public class Playable : StationState
			{
				readonly bool NotifyOnStop;

				public Playable(Station target, LessonSettings settings, bool notifyOnStop) : base(target, settings)
				{
					NotifyOnStop = notifyOnStop;
				}

				public override void Enter()
				{
					//stop station sequence
					Target.CurrentTimeline.Stop();
					if(NotifyOnStop) Target.OnStop?.Invoke();
				}
			}

			public class NotPlayable : Playable
			{
				public NotPlayable(Station target, LessonSettings settings, bool notifyOnStop) : base(target, settings, notifyOnStop) { }
			}

			public class Playing : StationState
			{
				public Playing(Station target, LessonSettings settings) : base(target, settings) { }

				public override void Enter()
				{
					EnterStation();

					//activate orb
					Target.Orb.Activate();

					//play current timeline (unless it is already playing)
					//if(Target.CurrentTimeline.state != PlayState.Playing)
					{
						Target.CurrentTimeline.Play();
						Target.OnPlay?.Invoke();
					}
				}

				public override void Exit()
				{
					ExitStation();

					//deactivate orb
					Target.Orb.Deactivate();
				}
			}

			public class Paused : StationState
			{
				public Paused(Station target, LessonSettings settings) : base(target, settings) { }

				public override void Enter()
				{
					EnterStation();

					//pause station sequence
					Target.CurrentTimeline.Pause();
					Target.OnPause?.Invoke();
				}

				public override void Exit()
				{
					ExitStation();

					//unpause
					if(Target.CurrentTimeline.state == PlayState.Paused)
					{
						Target.CurrentTimeline.Resume();
						Target.OnUnpause?.Invoke();
					}
				}
			}
		}
	}
}