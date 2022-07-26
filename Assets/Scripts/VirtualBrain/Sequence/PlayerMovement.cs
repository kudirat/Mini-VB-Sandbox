using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIDVE.Utilities;
using WIDVE.Paths;
using WIDVE.Patterns;
using PathCreation;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VBTesting
{
	[ExecuteAlways]
	public class PlayerMovement : MonoBehaviour
	{
		public enum Modes { Frozen, Track, Fly }

		[SerializeField]
		Player _player;
		public Player Player => _player;

		[SerializeField]
		Modes _mode = Modes.Fly;
		public Modes Mode
		{
			get => _mode;
			private set => _mode = value;
		}

		[SerializeField]
		GameObject _flyMovement;
		public GameObject FlyMovement => _flyMovement;

		[SerializeField]
		PathPosition _trackPosition;
		public PathPosition TrackPosition => _trackPosition;

		[SerializeField]
		PathMovement _trackMovement;
		public PathMovement TrackMovement => _trackMovement;

		FiniteStateMachine _fsm;
		StateMachine SM => _fsm ?? (_fsm = new FiniteStateMachine(new States.Fly(this, FlyMovement)));

		PathCreator MostRecentTrack = null;

		public string CurrentPlayerState() { return SM.CurrentState.ToString(); }
		
		public void Freeze(VBSystem system)
		{
			SetMovement(Modes.Frozen, system);
		}

		public void Unfreeze(VBSystem system)
		{
			if(SM.CurrentState is States.FrozenFly ff) SM.SetState(ff.i_State);
			else if(SM.CurrentState is States.FrozenTrack ft) SM.SetState(ft.i_State);
		}

		public void SetMovement(Modes mode, VBSystem system)
		{
			//just enter the correct movement state
			
			switch(mode)
			{
				default:
				case Modes.Frozen:
					if(Mode == Modes.Fly) SM.SetState(new States.FrozenFly(SM.CurrentState as States.Fly));
					else if(Mode == Modes.Track) SM.SetState(new States.FrozenTrack(SM.CurrentState as States.Track));
					break;

				case Modes.Fly:
					States.Fly flyState = new States.Fly(this, FlyMovement);
					if(SM.CurrentState is States.FrozenFly) SM.SetState(new States.FrozenFly(flyState));
					else SM.SetState(flyState);
					break;
					
				case Modes.Track:
					States.Track trackState = new States.Track(this, system ? system.Track : null, TrackPosition, TrackMovement);
					if(SM.CurrentState is States.FrozenTrack) SM.SetState(new States.FrozenTrack(trackState));
					else SM.SetState(trackState);
					break;
			}
			
			Debug.Log(CurrentPlayerState());
		}

		public void SetStartingPosition(VBSystem system, bool setRotation=true)
		{
			if(SM.CurrentState is States.PlayerMovementState pms)
			{
				pms.SetStartingPosition(system);
			}

			if(setRotation) SetStartingRotation(system);
		}

		public void SetStartingRotation(VBSystem system)
		{
			if(SM.CurrentState is States.PlayerMovementState pms)
			{
				pms.SetStartingRotation(system);
			}
		}

		void OnEnable()
		{
			//setup the current movement mode whenever this component is turned on
			SetMovement(Mode, VBSystem.CurrentSystem);
		}

		class States
		{
			public abstract class PlayerMovementState : State<PlayerMovement>
			{
				public PlayerMovementState(PlayerMovement target) : base(target) { }

				public virtual void SetStartingPosition(VBSystem system)
				{
					if(system && system.StartPosition)
					{
						//need to adjust for offset camera height
						Vector3 camOffset = Target.Player.CameraParent.position - Target.Player.transform.position;
						Target.Player.transform.position = system.StartPosition.position - camOffset;
						Debug.Log("Set starting position");
					}
				}

				public virtual void SetStartingRotation(VBSystem system)
				{
					if(system && system.StartPosition)
					{
						Target.Player.CameraParent.rotation = system.StartPosition.rotation;
						Debug.Log("Set starting rotation");
					}
				}
			}

			public class Fly : PlayerMovementState
			{
				public readonly GameObject FlyMovement;

				public Fly(PlayerMovement target, GameObject flyMovement) : base(target)
				{
					FlyMovement = flyMovement;
				}

				public override void Enter()
				{
					FlyMovement.SetActive(true);
				}

				public override void Exit()
				{
					FlyMovement.SetActive(false);
				}
			}

			public class Track : PlayerMovementState
			{
				public readonly PathCreator TrackPath;
				PathCreator i_TrackPath;
				public readonly PathPosition TrackPosition;
				public readonly PathMovement TrackMovement;

				public Track(PlayerMovement target, PathCreator trackPath, PathPosition trackPosition, PathMovement trackMovement) : base(target)
				{
					TrackPath = trackPath;
					TrackPosition = trackPosition;
					TrackMovement = trackMovement;
				}

				public override void Enter()
				{
					i_TrackPath = TrackPosition.Path;
					TrackPosition.SetPath(TrackPath);
					Target.MostRecentTrack = TrackPath;
					TrackPosition.enabled = true;
					TrackMovement.enabled = true;

					//place player on the track if they were off it
					if(Target.MostRecentTrack != TrackPath)
					{
						MoveOntoTrack(TrackPosition.Path, Target.Player.transform.position);
					}
				}

				public override void Exit()
				{
					TrackPosition.enabled = false;
					TrackMovement.enabled = false;
					TrackPosition.SetPath(i_TrackPath);
				}

				public override void SetStartingPosition(VBSystem system)
				{
					if(system.TrackStartPosition)
					{
						TrackPosition.SetPosition(system.TrackStartPosition.Position);
						Debug.Log("Set starting position");
					}
					else base.SetStartingPosition(system);
				}

				public override void SetStartingRotation(VBSystem system)
				{
					//rotate on Y axis to match track direction
					if(system.TrackStartPosition)
					{
						Quaternion trackRotation = system.TrackStartPosition.Rotation;
						Vector3 trackRotationAngles = new Vector3(0, trackRotation.eulerAngles.y, 0);
						Target.Player.CameraParent.rotation = Quaternion.Euler(trackRotationAngles);
						Debug.Log($"Set starting rotation {Target.Player.CameraParent.rotation}");
					}
					else base.SetStartingRotation(system);
				}

				public void MoveOntoTrack(PathCreator track, Vector3 currentPosition)
				{
					if(!track)
					{
						Debug.Log("Cannot move onto track; no track found!");
					}
					else
					{
						//make sure TrackPosition is on the correct track
						TrackPosition.SetPath(track);

						//move to the closest point on the track
						//float closestTrackPosition = track.path.GetClosestTimeOnPath(currentPosition);
						//TrackPosition.SetPosition(closestTrackPosition);

						//actually, just start at 0...
						TrackPosition.SetPosition(0);
					}
				}
			}

			//in frozen states, the movement components are disabled (upon exiting the movement state...)
			//however, setting starting position, etc, still happens
			public class FrozenFly : Fly
			{
				public readonly Fly i_State;

				public FrozenFly(Fly fly) : base(fly.Target, fly.FlyMovement)
				{
					i_State = fly;
				}

				public override void Enter() { }

				public override void Exit() { }
			}

			public class FrozenTrack : Track
			{
				public readonly Track i_State;
				PathCreator i_TrackPath;

				public FrozenTrack(Track track) : base(track.Target, track.TrackPath, track.TrackPosition, track.TrackMovement)
				{
					i_State = track;
				}

				public override void Enter()
				{
					i_TrackPath = TrackPosition.Path;
					TrackPosition.SetPath(TrackPath);
					Target.MostRecentTrack = TrackPath;
				}

				public override void Exit()
				{
					TrackPosition.SetPath(i_TrackPath);
				}
			}
		}

#if UNITY_EDITOR
		[CanEditMultipleObjects]
		[CustomEditor(typeof(PlayerMovement))]
		class Editor : UnityEditor.Editor
		{
			public override void OnInspectorGUI()
			{
				serializedObject.Update();

				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_player)));

				EditorGUI.BeginChangeCheck();

				SerializedProperty mode = serializedObject.FindProperty(nameof(_mode));
				EditorGUILayout.PropertyField(mode);

				bool modeChanged = EditorGUI.EndChangeCheck();

				//show different fields based on current mode
				if(mode.enumValueIndex == (int)Modes.Fly)
				{
					EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_flyMovement)));
				}
				else if(mode.enumValueIndex == (int)Modes.Track)
				{
					EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_trackPosition)));
					EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_trackMovement)));
				}

				serializedObject.ApplyModifiedProperties();

				if(modeChanged)
				{
					foreach(PlayerMovement pm in targets)
					{
						pm.SetMovement((Modes)mode.enumValueIndex, VBSystem.CurrentSystem);
					}
				}

				if(GUILayout.Button("Set Starting Position"))
				{
					foreach(PlayerMovement pm in targets)
					{
						pm.SetStartingPosition(VBSystem.CurrentSystem);
					}
				}
			}
		}
#endif
	}
}