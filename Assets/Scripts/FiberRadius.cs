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
    public class FiberRadius : MonoBehaviour
    {
        [SerializeField]
        float _fiberRadius;
        float FiberRad => _fiberRadius;

        public void SetRadii()
        {
            TubeRenderer[] renderers = GetComponentsInChildren<TubeRenderer>();

            foreach(TubeRenderer r in renderers)
            {
                r.radius = _fiberRadius;
                r.SetDirty();
            }
        }

#if UNITY_EDITOR
        [CanEditMultipleObjects]
        [CustomEditor(typeof(FiberRadius))]
        class Editor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                EditorGUI.BeginChangeCheck();

                base.OnInspectorGUI();

                bool changed = EditorGUI.EndChangeCheck();

                if(changed || GUILayout.Button("Set Radii"))
                {
                    foreach(FiberRadius fcr in targets)
                    {
                        fcr.SetRadii();
                    }
                }
            }
        }
#endif
    }
}