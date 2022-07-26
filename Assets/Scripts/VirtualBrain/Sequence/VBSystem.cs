using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIDVE.Paths;
using WIDVE.Utilities;
using PathCreation;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VBTesting
{
    [ExecuteAlways]
    public class VBSystem : MonoBehaviour
    {
        [SerializeField]
        SceneObject _sceneObject;
        public SceneObject SceneObject => _sceneObject;

        [SerializeField]
        string _trackTag = "Track";
        public string TrackTag => _trackTag;

        [SerializeField]
        Transform _startPosition;
        public Transform StartPosition => _startPosition;

        [SerializeField]
        PathPosition _trackStartPosition;
        public PathPosition TrackStartPosition => _trackStartPosition;

        PathCreator _track;
        public PathCreator Track
		{
			get
			{
                if(!_track) _track = GetTrack(TrackTag);
                return _track;
			}
		}

        static List<VBSystem> ActiveSystems = new List<VBSystem>();

        /// <summary>
        /// Returns the most recently loaded system.
        /// <para>Returns null if no system is currently loaded.</para>
        /// </summary>
        public static VBSystem CurrentSystem
		{
			get
			{
                if(ActiveSystems.Count > 0) return ActiveSystems[ActiveSystems.Count - 1];
                else return null;
			}
		}

        public static PathCreator GetTrack(string trackTag)
		{
            PathCreator track = null;

            GameObject track_go = GameObject.FindWithTag(trackTag);
			if(track_go)
			{
                track = track_go.GetComponentInChildren<PathCreator>();
			}

            return track;
		}

        public static void UnloadAllSystems()
		{
            foreach(VBSystem system in ActiveSystems)
			{
                system.SceneObject.Unload();
			}
		}

		void OnEnable()
		{
            ActiveSystems.Add(this);
		}

		void OnDisable()
		{
            ActiveSystems.Remove(this);
		}

#if UNITY_EDITOR
		[CanEditMultipleObjects]
        [CustomEditor(typeof(VBSystem))]
        class Editor : UnityEditor.Editor
		{
			public override void OnInspectorGUI()
			{
                serializedObject.Update();

                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_sceneObject)));

                SerializedProperty trackTag = serializedObject.FindProperty(nameof(_trackTag));
                trackTag.stringValue = EditorGUILayout.TagField(label: trackTag.displayName, tag: trackTag.stringValue);

                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_startPosition)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_trackStartPosition)));

                serializedObject.ApplyModifiedProperties();
			}
		}
#endif
    }
}