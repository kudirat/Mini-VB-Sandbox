using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VBTesting
{
    public class StationTester : MonoBehaviour
    {
        [SerializeField]
        LessonSettings _testSettings;
        LessonSettings TestSettings => _testSettings;

        Station Station => GetComponent<Station>();

        void Play()
        {
            Station.Play(TestSettings);
        }

        void Pause()
        {
            Station.Pause(TestSettings);
        }

        void Stop()
        {
            Station.Stop(TestSettings);
        }

#if UNITY_EDITOR
        [CanEditMultipleObjects]
        [CustomEditor(typeof(StationTester))]
        class Editor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                GUI.enabled = Application.isPlaying;

                GUILayout.BeginHorizontal();

                if(GUILayout.Button("Play"))
                {
                    foreach(StationTester st in targets)
                    {
                        st.Play();
                    }
                }

                if(GUILayout.Button("Pause"))
                {
                    foreach(StationTester st in targets)
                    {
                        st.Pause();
                    }
                }

                if(GUILayout.Button("Stop"))
                {
                    foreach(StationTester st in targets)
                    {
                        st.Stop();
                    }
                }

                GUILayout.EndHorizontal();

                GUI.enabled = true;
            }
        }
#endif
    }
}