using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIDVE.Utilities;

namespace VBTesting
{
	[CreateAssetMenu(fileName = nameof(LessonSettings), menuName = nameof(VBTesting) + "/" + nameof(LessonSettings), order = 2000)]
	public class LessonSettings : ScriptableObject
	{
		public enum StationModes { Replayable, NotReplayable }

		public enum ClosedCaptionsModes { SyncedToAudio, Manual }

		public enum ClosedCaptionsDisplayModes { Disabled, HandDisplay, StaticDisplay, HandAndStaticDisplay }

		[Header("Settings")]

		[SerializeField]
		StationModes _stationMode;
		public StationModes StationMode
		{
			get => _stationMode;
			set
			{
				_stationMode = value;
				//notify that setting has changed
				//nothing uses this for replayable, but putting it here in case that changes in the future...
				StationSettingsChangedEvent.Invoke();
			}

		}

		[SerializeField]
		bool _stopAtStations = false;
		public bool StopAtStations
		{
			get => _stopAtStations;
			set
			{
				_stopAtStations = value;
				//notify that setting has changed
				StationSettingsChangedEvent.Invoke();
			}
		}

		[SerializeField]
		ClosedCaptionsModes _closedCaptionsMode;
		public ClosedCaptionsModes ClosedCaptionsMode
		{
			get => _closedCaptionsMode;
			set => _closedCaptionsMode = value;
		}

		[SerializeField]
		ClosedCaptionsDisplayModes _closedCaptionsDisplayMode;
		public ClosedCaptionsDisplayModes ClosedCaptionsDisplayMode
		{
			get => _closedCaptionsDisplayMode;
			set => _closedCaptionsDisplayMode = value;
		}

		[SerializeField]
		string _handClosedCaptionsTag = "HandClosedCaptions";
		public string HandClosedCaptionsTag
		{
			get => _handClosedCaptionsTag;
			set => _handClosedCaptionsTag = value;
		}

		[SerializeField]
		float _captionsFadeTime = 2;
		public float CaptionsFadeTime
		{
			get => _captionsFadeTime;
			set => _captionsFadeTime = value;
		}

		[Header("Events")]

		[SerializeField]
		BoolEvent _freezePlayerEvent;
		public BoolEvent FreezePlayerEvent
		{
			get => _freezePlayerEvent;
			set => _freezePlayerEvent = value;
		}

		[SerializeField]
		public ScriptableEvent MenuOpenedEvent;

		[SerializeField]
		public ScriptableEvent MenuClosedEvent;

		[SerializeField]
		public ScriptableEvent StationSettingsChangedEvent;

		ClosedCaptions _handClosedCaptions;
		public ClosedCaptions HandClosedCaptions
		{
			get
			{
				//make this not use tags later
				if(!_handClosedCaptions)
				{
					GameObject handClosedCaptions_go = GameObject.FindGameObjectWithTag(HandClosedCaptionsTag);
					if(handClosedCaptions_go) _handClosedCaptions = handClosedCaptions_go.GetComponentInChildren<ClosedCaptions>();
				}
				return _handClosedCaptions;
			}
		}

		public void FreezePlayer(bool freeze)
		{
			FreezePlayerEvent.Invoke(freeze);
		}

		public void NotifyMenu(bool open)
		{
			if(open) MenuOpenedEvent?.Invoke();
			else MenuClosedEvent?.Invoke();
		}

		/// <summary>
		/// Overwrite this LessonSettings object with the settings from the source object.
		/// </summary>
		public void Clone(LessonSettings source)
		{
			StationMode = source.StationMode;
			StopAtStations = source.StopAtStations;
			ClosedCaptionsMode = source.ClosedCaptionsMode;
			ClosedCaptionsDisplayMode = source.ClosedCaptionsDisplayMode;
			HandClosedCaptionsTag = source.HandClosedCaptionsTag;
			CaptionsFadeTime = source.CaptionsFadeTime;
			FreezePlayerEvent = source.FreezePlayerEvent;
			MenuOpenedEvent = source.MenuOpenedEvent;
			MenuClosedEvent = source.MenuClosedEvent;
			StationSettingsChangedEvent = source.StationSettingsChangedEvent;
		}
	}
}