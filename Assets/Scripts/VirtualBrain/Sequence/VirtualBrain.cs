using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using WIDVE.Patterns;
using WIDVE.Utilities;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VBTesting
{
	[ExecuteAlways]
    public class VirtualBrain : MonoBehaviour
    {
		[SerializeField]
		string _playerTag = "Player";
		public string PlayerTag => _playerTag;

		[SerializeField]
		string _systemTag = "System";
		public string SystemTag => _systemTag;

		[SerializeField]
		string _systemStartTextTag = "SystemStartText";
		public string SystemStartTextTag => _systemStartTextTag;

		[SerializeField]
		string _systemEndTextTag = "SystemEndText";
		public string SystemEndTextTag => _systemEndTextTag;
		
		[SerializeField]
		string _systemStartScreenTag = "PCStartScreen";
		public string SystemStartScreenTag => _systemStartScreenTag;

		[SerializeField]
		CursorSettings CursorSettings;

		[SerializeField]
		VBSystemLoader SystemLoader;

		[SerializeField]
		public SceneObject DefaultSystem;

		[SerializeField]
		VBMenu Menu;

		[SerializeField]
		LessonSettings PlaymodeSettings;

		[SerializeField]
		[Tooltip("If Testing Mode is enabled, pressing Play in the Editor will skip the usual app startup sequence.")]
		bool TestingMode = false;

		public bool InLesson
		{
			get
			{
				if(Player) return Player.CurrentSystem != null && Player.CurrentSystem.SceneObject != DefaultSystem;
				else return false;
			}
		}

		Player _player;
		public Player Player
		{
			get
			{
				if(!_player) _player = GetPlayer(PlayerTag);
				return _player;
			}
		}

		bool PlayerIsSetup = false;

		public PlayerMovement PlayerMovement => Player ? Player.Movement : null;

		/// <summary>
		/// Returns the first active Player.
		/// </summary>
		public static Player GetPlayer(string playerTag)
		{
			Player player = null;

			GameObject player_go = GameObject.FindWithTag(playerTag);
			if(player_go)
			{
				player = player_go.GetComponentInChildren<Player>();
			}

			return player;
		}

		/// <summary>
		/// Returns the first active VBSystem.
		/// </summary>
		public static VBSystem GetSystem(string systemTag)
		{
			VBSystem system = null;

			GameObject system_go = GameObject.FindWithTag(systemTag);
			if(system_go)
			{
				system = system_go.GetComponentInChildren<VBSystem>();
			}

			return system;
		}

		VBSystem GetSystemInScene(string systemTag, Scene scene)
		{
			VBSystem system = null;

			GameObject[] rootObjects = scene.GetRootGameObjects();
			for(int i = 0; i < rootObjects.Length; i++)
			{
				system = rootObjects[i].GetComponentInChildren<VBSystem>();

				if(system) break;
			}

			return system;
		}
		
		/// <summary>
		/// When a scene is loaded, sets up the current system and player
		/// </summary>
		void SetUpScene(Scene scene, LoadSceneMode mode)
		{
			//if the scene has a system, set up the system and player
			VBSystem system = GetSystemInScene(SystemTag, scene);
			if(system)
			{
				//set this as the active scene
				SceneManager.SetActiveScene(scene);

				//set up system objects
				SetUpSystem(system);

				//set up player objects
				SetUpPlayer(Player, system);
			}
		}

		/// <summary>
		/// Actions that need to be taken when loading a new system.
		/// </summary>
		/// <param name="system"></param>
		void SetUpSystem(VBSystem system)
		{
			//show the system start text and show cursor
			GameObject systemStartText = GameObject.FindGameObjectWithTag(SystemStartTextTag);
			if(systemStartText)
			{
				VBTextDisplay vbtd = systemStartText.GetComponent<VBTextDisplay>();
				//Command showCursor = new CursorSettings.Commands.ShowCursor(CursorSettings, true);
				//vbtd.Open(showCursor);
				vbtd.Open(null); //actually, don't show cursor
			}
		}

		/// <summary>
		/// Sets up the player in the current system.
		/// <para>Sets up movement mode and starting position.</para>
		/// </summary>
		void SetUpPlayer(Player player, VBSystem system)
		{
			Debug.Log($"Setting up player in {system.SceneObject.Name}.");

			//update the current system for the player
			if(system != null)
			{
				player.SetSystem(system);

				//set starting position and rotation
				player.Movement.SetMovement(player.Movement.Mode, system);
				player.Movement.SetStartingPosition(system); //this will set the rotation after setting the position
				
				GameObject g = GameObject.FindWithTag(_systemStartScreenTag);
				if(g != null)
				{
					for(int i = 0; i < g.transform.childCount; ++i)
					{
						g.transform.GetChild(i).gameObject.SetActive(true);
					}
				}
			}

			//done
			PlayerIsSetup = true;
		}

		public void SetMovementMode(int mode)
		{
			SetMovementMode((PlayerMovement.Modes)mode);
		}

		public void SetMovementMode(PlayerMovement.Modes mode)
		{
			if(Player)
			{
				Player.Movement.SetMovement(mode, Player.CurrentSystem);
			}
		}

		void OnEnable()
		{
			SceneObject.SceneObjectLoaded += SetUpScene;
		}

		void Start()
		{
#if UNITY_EDITOR
			if(TestingMode)
			{
				//run any editor-only startup code here, then return before starting the usual sequence
				//hide the menu
				if(Menu)
				{
					Menu.Close();
				}

				//turn off station guards
				if(PlaymodeSettings)
				{
					PlaymodeSettings.StopAtStations = false;
				}
				return;
			}
#endif
			if(Application.isPlaying)
			{
				//load the empty scene
				SystemLoader.LoadSystem(DefaultSystem);
				//DefaultSystem.Load();
			}
		}

		void OnDisable()
		{
			SceneObject.SceneObjectLoaded -= SetUpScene;
		}

#if UNITY_EDITOR
		void Update()
		{
			if(Application.isPlaying && !PlayerIsSetup)
			{
				//try to setup the player in a loaded system
				VBSystem[] allSystems = FindObjectsOfType<VBSystem>();
				if(allSystems.Length > 0)
				{
					SetUpPlayer(Player, allSystems[0]);
				}
				else
				{
					//don't try to find systems every frame, just give up
					PlayerIsSetup = true;
				}
			}
		}

		[CanEditMultipleObjects]
		[CustomEditor(typeof(VirtualBrain))]
		class Editor : UnityEditor.Editor
		{
			public override void OnInspectorGUI()
			{
				serializedObject.Update();

				SerializedProperty playerTag = serializedObject.FindProperty(nameof(_playerTag));
				playerTag.stringValue = EditorGUILayout.TagField(playerTag.displayName, playerTag.stringValue) ;

				SerializedProperty systemTag = serializedObject.FindProperty(nameof(_systemTag));
				systemTag.stringValue = EditorGUILayout.TagField(systemTag.displayName, systemTag.stringValue);

				SerializedProperty systemStartTextTag = serializedObject.FindProperty(nameof(_systemStartTextTag));
				systemStartTextTag.stringValue = EditorGUILayout.TagField(systemStartTextTag.displayName, systemStartTextTag.stringValue);

				SerializedProperty systemEndTextTag = serializedObject.FindProperty(nameof(_systemEndTextTag));
				systemEndTextTag.stringValue = EditorGUILayout.TagField(systemEndTextTag.displayName, systemEndTextTag.stringValue);

				SerializedProperty systemStartScreenTag = serializedObject.FindProperty(nameof(_systemStartScreenTag));
				systemStartScreenTag.stringValue = EditorGUILayout.TagField(systemStartScreenTag.displayName, systemStartScreenTag.stringValue);
				
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(CursorSettings)));

				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SystemLoader)));

				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(DefaultSystem)));

				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(Menu)));

				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(PlaymodeSettings)));

				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(TestingMode)));

				serializedObject.ApplyModifiedProperties();

				if(GUILayout.Button("Set Up Player"))
				{
					foreach(VirtualBrain vb in targets)
					{
						VBSystem system = VBSystem.CurrentSystem;

						vb.SetUpPlayer(vb.Player, system);
					}
				}
			}
		}
#endif
	}
}