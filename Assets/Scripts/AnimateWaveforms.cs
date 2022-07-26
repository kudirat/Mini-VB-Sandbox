using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIDVE.Utilities;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace VBTesting
{
    public class AnimateWaveforms : MonoBehaviour
    {
        [SerializeField]
        Interpolator _chordInterpolator;
        Interpolator ChordInterpolator => _chordInterpolator;

        [SerializeField]
        Interpolator _notesInterpolator;
        Interpolator NotesInterpolator => _notesInterpolator;

        [SerializeField]
        [Range(0, 2)]
        float _fadeTime = 1;
        public float FadeTime => _fadeTime;

        [SerializeField]
        [Range(0, 5)]
        float _notesExpandTime = 2;
        public float NotesExpandTime => _notesExpandTime;

        [SerializeField]
        [Range(0, 45)]
        float _notesExpandAngle = 10;
        public float NotesExpandAngle => _notesExpandAngle;

        [SerializeField]
        [Range(0, 10)]
        float _notesExpandDistance = 5;
        public float NotesExpandDistance => _notesExpandDistance;

        [SerializeField]
        [Range(0, 5)]
        float _notePlayInterval = 1;
        public float NotePlayInterval => _notePlayInterval;

        [SerializeField]
        AudioSource _chord;
        AudioSource Chord => _chord;

        [SerializeField]
        [HideInInspector]
        List<AudioSource> _notes;
        List<AudioSource> Notes => _notes ?? (_notes = new List<AudioSource>());

        [SerializeField]
        [HideInInspector]
        [Range(0, 1)]
        float _noteExpansion = 1;
        float NoteExpansion => _noteExpansion;

        float ChordDuration => (FadeTime * 2) + NotePlayInterval;

        float NotesDuration => (Mathf.Max(FadeTime, NotesExpandTime) * 2) + (Notes.Count * NotePlayInterval);

        bool NotesHidden = false;

        IEnumerator CurrentChordCoroutine = null;

        IEnumerator CurrentNoteCoroutine = null;

        public void PlayChord()
        {
            if(CurrentChordCoroutine != null)
            {
                StopCoroutine(CurrentChordCoroutine);
                Chord.Stop();
            }

            CurrentChordCoroutine = PlayChordCoroutine();
            StartCoroutine(CurrentChordCoroutine);
        }

        public void PlayNotes()
        {
            if(CurrentNoteCoroutine != null)
            {
                StopCoroutine(CurrentNoteCoroutine);
                foreach(AudioSource note in Notes) note.Stop();
            }

            CurrentNoteCoroutine = PlayNotesCoroutine();
            StartCoroutine(CurrentNoteCoroutine);
        }

        IEnumerator PlayChordCoroutine()
        {
            //fade in
            ChordInterpolator.LerpOverTime(0, 1, FadeTime);
            yield return new WaitForSeconds(FadeTime);

            //play chord audio clip
            Chord.Play();
            yield return new WaitForSeconds(NotePlayInterval * 1.5f);

            //fade out
            ChordInterpolator.LerpOverTime(1, 0, FadeTime);
            yield return new WaitForSeconds(FadeTime);
        }

        IEnumerator PlayNotesCoroutine()
        {
            //fade in and expand
            NotesInterpolator.LerpOverTime(0, 1, FadeTime);
            StartCoroutine(ExpandNotes(true));
            yield return new WaitForSeconds(Mathf.Max(FadeTime, NotesExpandTime));

            //play all notes
            foreach(AudioSource note in Notes)
            {
                note.Play();
                yield return new WaitForSeconds(NotePlayInterval);
            }

            //fade out and shrink
            NotesInterpolator.LerpOverTime(1, 0, FadeTime);
            StartCoroutine(ExpandNotes(false));
            yield return new WaitForSeconds(Mathf.Max(FadeTime, NotesExpandTime));
        }

        IEnumerator ExpandNotes(bool expand)
        {
            float startTime = Time.time;
            float endTime = startTime + NotesExpandTime;

            while(Time.time < endTime)
            {
                float timeElapsed = Time.time - startTime;
                float t = timeElapsed / NotesExpandTime;

                //if shrinking, just expand in the opposite direction
                if(!expand) t = 1 - t;

                PositionNotes(t);

                yield return new WaitForEndOfFrame();
            }

            //finalize position at end
            PositionNotes(expand ? 1 : 0);
        }

        void PositionNotes(float t)
        {
            for(int i = 0; i < Notes.Count; i++)
            {
                Transform note = Notes[i].transform;

                float note_t = i / (float)(Notes.Count - 1);
                float distance = Mathf.Lerp(-NotesExpandDistance, NotesExpandDistance, note_t);
                float angle = Mathf.Lerp(-NotesExpandAngle, NotesExpandAngle, note_t);

                note.localPosition = new Vector3(0, Mathf.Lerp(0, distance, t), 0);
                note.localRotation = Quaternion.Euler(new Vector3(0, 0, Mathf.Lerp(0, angle, t)));
            }
        }

        void Update()
        {
            if(!NotesHidden)
            {
                //start notes hidden
                ChordInterpolator.LerpTo0(0);
                NotesInterpolator.LerpTo0(0);
                PositionNotes(0);
                NotesHidden = true;
            }
        }

#if UNITY_EDITOR
        [CanEditMultipleObjects]
        [CustomEditor(typeof(AnimateWaveforms))]
        class Editor : UnityEditor.Editor
        {
            ReorderableList Notes;

            protected virtual void OnEnable()
            {
                Notes = new ReorderableList(serializedObject, serializedObject.FindProperty(nameof(_notes)),
                                            true, true, true, true);

                Notes.drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(rect, "Notes (low to high)");
                };

                Notes.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    SerializedProperty element = Notes.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(position: new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                                            property: element,
                                            label: new GUIContent(index.ToString()));
                };
            }

            public override void OnInspectorGUI()
            {
                serializedObject.Update();

                EditorGUI.BeginChangeCheck();

                DrawDefaultInspector();

                Notes.DoLayoutList();

                AnimateWaveforms animateWaveforms = target as AnimateWaveforms;

                SerializedProperty noteExpansion = serializedObject.FindProperty(nameof(_noteExpansion));
                if(!Application.IsPlaying(this))
                {
                    //show slider to test
                    EditorGUILayout.PropertyField(noteExpansion);
                }

                bool changed = EditorGUI.EndChangeCheck();

                if(changed && !Application.isPlaying)
                {
                    animateWaveforms.PositionNotes(noteExpansion.floatValue);
                    EditorUtility.SetDirty(animateWaveforms.Chord.transform);
                    foreach(AudioSource note in animateWaveforms.Notes)
                    {
                        EditorUtility.SetDirty(note.transform);
                    }
                }

                if(!Application.IsPlaying(this)) GUI.enabled = false;

                GUILayout.BeginHorizontal();

                if(GUILayout.Button("Play Chord"))
                {
                    foreach(AnimateWaveforms aw in targets)
                    {
                        aw.PlayChord();
                    }
                }

                if(GUILayout.Button("Play Notes"))
                {
                    foreach(AnimateWaveforms aw in targets)
                    {
                        aw.PlayNotes();
                    }
                }

                GUILayout.EndHorizontal();

                if(!Application.IsPlaying(this)) GUI.enabled = true;

                //show total duration
                GUI.enabled = false;

                EditorGUILayout.LabelField(label: "Chord Duration:", label2: $"{animateWaveforms.ChordDuration}");
                EditorGUILayout.LabelField(label: "Notes Duration:", label2: $"{animateWaveforms.NotesDuration}");

                GUI.enabled = true;

                serializedObject.ApplyModifiedProperties();
            }
        }
#endif
    }
}