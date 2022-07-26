using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using WIDVE.IO;
using WIDVE.Utilities;
using WIDVE.Graphics;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VBTesting
{
    public class ClosedCaptions : MonoBehaviour
    {
        [SerializeField]
        Canvas _canvas;
        Canvas Canvas => _canvas;

        [SerializeField]
        TextMeshProUGUI _text;
        TextMeshProUGUI Text => _text;

        [SerializeField]
        Interpolator _interpolator;
        Interpolator Interpolator => _interpolator;

        [SerializeField]
        DataFileTXT _captionsFile;
        public DataFileTXT CaptionsFile
        {
            get => _captionsFile;
            set => _captionsFile = value;
        }

        [SerializeField]
        //[HideInInspector]
        List<string> _captions;
        public List<string> Captions
        {
            get => _captions;
            private set => _captions = value;
        }

        public event System.Action<string> OnCaptionChanged;

        int _currentCaption = 0;
        int CurrentCaption
        {
            get => _currentCaption;
            set => _currentCaption = Captions.Count > 0 ? Mathf.Clamp(value, 0, Captions.Count - 1) : 0;
        }

        public void LoadCaptions()
        {
            if(!CaptionsFile) return;

            using(StreamReader sr = CaptionsFile.GetReader())
            {
                Captions = new List<string>();
                CurrentCaption = 0;

                //each line of captions is on its own line in the file
                while(sr.Peek() >= 0)
                {
                    string line = sr.ReadLine();

                    if(string.IsNullOrEmpty(line)) continue;

                    Captions.Add(line);
                }
            }

            DisplayCaption();
        }

        public void DisplayCaption()
        {
            DisplayCaption(CurrentCaption);
        }

        public void DisplayCaption(int captionIndex)
        {
            if(!Text) return;
            if(Captions == null || Captions.Count == 0) return;

            captionIndex = Mathf.Clamp(captionIndex, 0, Captions.Count - 1);
            string caption = Captions[captionIndex];
			
			SetText("");
			StartCoroutine(PauseBetween(0.004f, caption, captionIndex));
        }
		
		IEnumerator PauseBetween(float duration, string caption, int captionIndex)
		{
			yield return new WaitForSeconds(duration);
			
			SetText(caption);

            CurrentCaption = captionIndex;

            OnCaptionChanged?.Invoke(caption);
		}
		
        public void DisplayNextCaption()
        {
            CurrentCaption++;
            DisplayCaption(CurrentCaption);
        }

        public void DisplayPreviousCaption()
        {
            CurrentCaption--;
            DisplayCaption(CurrentCaption);
        }

        void SetText(string text)
        {
            Text.gameObject.SetActive(false);
            Text.text = text;
            Text.gameObject.SetActive(true);
#if UNITY_EDITOR
            EditorUtility.SetDirty(Text);
#endif
        }

        public void ShowText(bool visible)
        {
            Canvas.gameObject.SetActive(visible);
        }

        public void FadeIn(float t)
		{
            Interpolator.LerpTo1(t);
		}

        public void FadeOut(float t)
		{
            Interpolator.LerpTo0(t);
        }

        private void OnEnable()
        {
            CurrentCaption = 0;
            DisplayCaption();
        }

#if UNITY_EDITOR
        [CanEditMultipleObjects]
        [CustomEditor(typeof(ClosedCaptions))]
        class Editor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                EditorGUI.BeginChangeCheck();

                base.OnInspectorGUI();

                bool changed = EditorGUI.EndChangeCheck();

                GUILayout.BeginHorizontal();

                if(GUILayout.Button("Load Captions") || changed)
                {
                    foreach(ClosedCaptions cc in targets)
                    {
                        cc.LoadCaptions();
                        EditorUtility.SetDirty(cc);
                    }
                }

                if(GUILayout.Button("Display Previous"))
                {
                    foreach(ClosedCaptions cc in targets)
                    {
                        cc.DisplayPreviousCaption();
                    }
                }

                if(GUILayout.Button("Display Next"))
                {
                    foreach(ClosedCaptions cc in targets)
                    {
                        cc.DisplayNextCaption();
                    }
                }

                GUILayout.EndHorizontal();
            }
        }
#endif
    }
}