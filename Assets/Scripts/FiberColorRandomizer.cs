using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIDVE.Graphics;
#if UNITY_EDITOR
using UnityEditor;
#endif
using WIDVE.Utilities;

namespace VBTesting
{
    public class FiberColorRandomizer : MonoBehaviour
    {
        [SerializeField]
        Material _fiberMaterial;
        Material FiberMaterial => _fiberMaterial;

        [SerializeField]
        ShaderProperties _materialProperties;
        ShaderProperties MaterialProperties => _materialProperties;

        [SerializeField]
        int _seed = 0;
        int Seed => _seed;

        [SerializeField]
        string _colorName = "_BaseColor";
        string ColorName => _colorName;

        [SerializeField]
        List<Color> _colors;
        public List<Color> Colors => _colors ?? (_colors = new List<Color>());

        public void SetColors()
        {
            if(!FiberMaterial) return;
            if(!MaterialProperties) return;
            if(Colors.Count <= 0) return;

            Renderer[] renderers = GetComponentsInChildren<Renderer>();

            //use the same seed everytime so the randomly selected colors are saved
            System.Random rng = new System.Random(Seed);

            foreach(Renderer r in renderers)
            {
                r.sharedMaterial = FiberMaterial;

                PerRendererColor prc = r.GetComponent<PerRendererColor>();
                if(!prc) prc = r.gameObject.AddComponent<PerRendererColor>();

                prc.Properties = MaterialProperties;
                prc.ColorName = ColorName;

                int colorIndex = rng.Next(0, Colors.Count);

                prc.Color = Colors[colorIndex];
                prc.SetColor();

                prc.SetDirty();
            }
        }

#if UNITY_EDITOR
        [CanEditMultipleObjects]
        [CustomEditor(typeof(FiberColorRandomizer))]
        class Editor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                EditorGUI.BeginChangeCheck();

                base.OnInspectorGUI();

                bool changed = EditorGUI.EndChangeCheck();

                if(changed || GUILayout.Button("Set Colors"))
                {
                    foreach(FiberColorRandomizer fcr in targets)
                    {
                        fcr.SetColors();
                    }
                }
            }
        }
#endif
    }
}