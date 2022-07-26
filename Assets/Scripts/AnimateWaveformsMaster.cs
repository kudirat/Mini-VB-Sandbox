using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace VBTesting
{
    public class AnimateWaveformsMaster : MonoBehaviour
    {
        [SerializeField]
        List<AnimateWaveforms> _slaves;
        List<AnimateWaveforms> Slaves => _slaves;

        [SerializeField]
        AudioSource _masterChord;
        AudioSource MasterChord => _masterChord;

        [SerializeField]
        [HideInInspector]
        List<AudioSource> _masterNotes;
        List<AudioSource> MasterNotes => _masterNotes;

        IEnumerator CurrentChordCoroutine = null;

        IEnumerator CurrentNoteCoroutine = null;

        public void PlayChord()
        {
            foreach(AnimateWaveforms aw in Slaves)
            {
                aw.PlayChord();
            }

            if(CurrentChordCoroutine != null)
            {
                StopCoroutine(CurrentChordCoroutine);
                MasterChord.Stop();
            }
            CurrentChordCoroutine = PlayChordCoroutine();
            StartCoroutine(CurrentChordCoroutine);
        }

        public void PlayNotes()
        {
            foreach(AnimateWaveforms aw in Slaves)
            {
                aw.PlayNotes();
            }

            if(CurrentNoteCoroutine != null)
            {
                StopCoroutine(CurrentNoteCoroutine);
                foreach(AudioSource note in MasterNotes) note.Stop();
            }
            CurrentNoteCoroutine = PlayNotesCoroutine();
            StartCoroutine(CurrentNoteCoroutine);
        }

        IEnumerator PlayChordCoroutine()
        {
            yield return new WaitForSeconds(Slaves[0].FadeTime);

            //play chord audio clip
            MasterChord.Play();
            yield return new WaitForSeconds(Slaves[0].NotePlayInterval * 1.5f);
        }

        IEnumerator PlayNotesCoroutine()
        {
            yield return new WaitForSeconds(Mathf.Max(Slaves[0].FadeTime, Slaves[0].NotesExpandTime));

            //play all notes
            foreach(AudioSource note in MasterNotes)
            {
                note.Play();
                yield return new WaitForSeconds(Slaves[0].NotePlayInterval);
            }
        }

#if UNITY_EDITOR
        [CanEditMultipleObjects]
        [CustomEditor(typeof(AnimateWaveformsMaster))]
        class Editor : UnityEditor.Editor
        {
            ReorderableList MasterNotes;

            protected virtual void OnEnable()
            {
                MasterNotes = new ReorderableList(serializedObject, serializedObject.FindProperty(nameof(_masterNotes)),
                                            true, true, true, true);

                MasterNotes.drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(rect, "Master Notes (will be heard but not visualized)");
                };

                MasterNotes.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    SerializedProperty element = MasterNotes.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(position: new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                                            property: element,
                                            label: new GUIContent(index.ToString()));
                };
            }

            public override void OnInspectorGUI()
            {
                serializedObject.Update();

                DrawDefaultInspector();

                MasterNotes.DoLayoutList();

                serializedObject.ApplyModifiedProperties();

                if(!Application.IsPlaying(this)) GUI.enabled = false;

                GUILayout.BeginHorizontal();

                if(GUILayout.Button("Play Chord"))
                {
                    foreach(AnimateWaveformsMaster awm in targets)
                    {
                        awm.PlayChord();
                    }
                }

                if(GUILayout.Button("Play Notes"))
                {
                    foreach(AnimateWaveformsMaster awm in targets)
                    {
                        awm.PlayNotes();
                    }
                }

                GUILayout.EndHorizontal();

                if(!Application.IsPlaying(this)) GUI.enabled = true;
            }
        }
#endif
    }
}