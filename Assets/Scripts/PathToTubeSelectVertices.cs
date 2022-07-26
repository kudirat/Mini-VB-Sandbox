
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIDVE.Patterns;
using WIDVE.Utilities;
using PathCreation;
using WIDVE.Paths;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VBTesting
{
    public class PathToTubeSelectVertices : MonoBehaviour, IObserver<PathCreator>
    {
        [SerializeField]
        PathCreator _path;
        public PathCreator Path => _path;

        [SerializeField]
        TubeRenderer _tube;
        public TubeRenderer Tube => _tube; // C# property, Tube references _tube, easier to change values from Editor to Unity

        [SerializeField]
        public int startIndex = 0;

        [SerializeField]
        public int endIndex = 10;

        public void MakeTube()
        {
            MakeTube(Path, Tube, startIndex, endIndex);
        }

        public void OnDrawGizmosSelected()
        {
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(Path.path.GetPoint(startIndex), 0.5f);
            Gizmos.DrawSphere(Path.path.GetPoint(endIndex), 0.5f);
        }

        public void OnValidate() // temporary solution
        {
            // run when you change value in the inspector and other times
            MakeTube();
        }

        public static void MakeTube(PathCreator path, TubeRenderer tube, int startIndex, int endIndex)
        {
            if (!path) return;
            if (!tube) return;

            //set up points array for the tube
            int totalPoints = endIndex - startIndex; // add path render path parameters (pass in start index and end index)
            Vector3[] tubePoints = new Vector3[totalPoints];

            //transform points from path space to tube space
            for (int i = startIndex; i < totalPoints + startIndex; i++)
            {
                Vector3 pathLocalPoint = path.path.localPoints[i];
                Vector3 worldPoint = path.transform.TransformPoint(pathLocalPoint);
                Vector3 tubeLocalPoint = tube.transform.InverseTransformPoint(worldPoint);
                tubePoints[i - startIndex] = tubeLocalPoint;
            }

            //assign points to the tube
            try
            {
                tube.points = tubePoints;
                tube.ForceUpdate();
                tube.SetDirty();
            }
            catch (UnassignedReferenceException) { }
        }

        public void OnNotify()
        {
            MakeTube();
        }

#if UNITY_EDITOR // make sure index values change here if the editor isn't updating to live changes
        [CanEditMultipleObjects]
        [CustomEditor(typeof(PathToTube))]
        class Editor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                EditorGUI.BeginChangeCheck();

                base.OnInspectorGUI();

                bool changed = EditorGUI.EndChangeCheck();

                if (GUILayout.Button("Create Tube") || changed)
                {
                    foreach (PathToTube ptt in targets)
                    {
                        ptt.MakeTube();
                    }
                }
            }
        }
#endif
    }
}
