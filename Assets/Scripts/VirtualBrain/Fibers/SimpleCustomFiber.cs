using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using WIDVE.Patterns;
using WIDVE.Utilities;
using WIDVE.Graphics;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VBTesting
{
    public class SimpleCustomFiber : MonoBehaviour, IObserver<PathCreator>
    {
        [SerializeField]
        PathCreator _path;
        PathCreator Path => _path;

        [SerializeField]
        GameObject _fiberPrefab;
        GameObject FiberPrefab => _fiberPrefab;

        enum ColoringModes { Single, Striped, Random }

        [SerializeField]
        ColoringModes _coloringMode = ColoringModes.Single;
        ColoringModes ColoringMode => _coloringMode;

        [SerializeField]
        List<Color> _colors;
        List<Color> Colors => _colors ?? (_colors = new List<Color>());

        [SerializeField]
        Material _fiberMaterial;
        Material FiberMaterial => _fiberMaterial;

        [SerializeField]
        ShaderProperties _fiberProperties;
        ShaderProperties FiberProperties => _fiberProperties;

        [SerializeField]
        string _colorName = "_BaseColor";
        string ColorName => _colorName;

        [SerializeField]
        [Range(.01f, 1f)]
        float _overallRadius = .1f;
        float OverallRadius => _overallRadius;

        [SerializeField]
        [Range(.01f, 1f)]
        float _fiberRadius = .01f;
        float FiberRadius => _fiberRadius;

        [SerializeField]
        bool _autoRadius = true;
        bool AutoRadius => _autoRadius;

        [SerializeField]
        [Range(1, 64)]
        int _bundleSize = 1;
        int BundleSize => _bundleSize;

        static float AutoRadiusFudge = .2f;

        public void Generate()
        {
            if(!Path) return;
            if(!FiberPrefab) return;

            //initialize fibers
            TubeRenderer[] fibers = GetFibers(gameObject, FiberPrefab, BundleSize);
            Vector3[][] fiberPoints = new Vector3[fibers.Length][];
            for(int i = 0; i < fibers.Length; i++)
            {
                fiberPoints[i] = new Vector3[Path.path.NumPoints];
            }

            //set points on each fiber
            for(int i = 0; i < Path.path.NumPoints; i++)
            {
                //assume that the path, custom fiber, and individual fibers have the same local transform
                Vector3 pathPoint = transform.InverseTransformPoint(Path.path.GetPoint(i));
                Vector3 pathDirection = transform.InverseTransformDirection(Path.path.GetTangent(i));
                Vector3 pathNormal = transform.InverseTransformVector(Path.path.GetNormal(i)).normalized;

                //set point on each fiber
                Vector3[] bundlePoints = GetBundlePoints(pathPoint, pathDirection, pathNormal, OverallRadius, BundleSize);
                for(int j = 0; j < fibers.Length; j++)
                {
                    fiberPoints[j][i] = bundlePoints[j];
                }
            }

            //set radius for each fiber
            float fiberRadius;
            if(AutoRadius) fiberRadius = CalculateFiberRadius(OverallRadius, BundleSize);
            else fiberRadius = FiberRadius;

            for(int i = 0; i < fibers.Length; i++)
            {
                fibers[i].radius = fiberRadius;
            }

            //set material for each fiber
            ColorFibers(fibers, ColoringMode);

            //set points on tubes
            for(int i = 0; i < fibers.Length; i++)
            {
                fibers[i].points = fiberPoints[i];

                try
                {
                    fibers[i].ForceUpdate();
                }
                catch(UnassignedReferenceException) { }
                catch(System.NullReferenceException) { }

                fibers[i].SetDirty();
            }
        }

        static float CalculateFiberRadius(float overallRadius, int bundleSize)
        {
            //https://www.mathopenref.com/polygonsides.html
            if(bundleSize == 1) return overallRadius;
            else return ((2 * overallRadius * (Mathf.Sin(Mathf.Deg2Rad * 180 / (float)bundleSize))) / 2f) * (1 + AutoRadiusFudge);
        }

        static TubeRenderer[] GetFibers(GameObject root, GameObject fiberPrefab, int bundleSize)
        {
            //get any fibers that already exist
            TubeRenderer[] existingFibers = root.GetComponentsInChildren<TubeRenderer>(true);

            for(int i = 0; i < existingFibers.Length; i++)
            {
                //set all fibers active to start
                GameObject g = existingFibers[i].gameObject;
                if(!g.activeSelf) g.SetActive(true);

                //also make sure they are dynamic
                try
                {
                    existingFibers[i].MarkDynamic();
                }
                //errors can happen if the TubeRenderer has not created its mesh yet
                catch(UnassignedReferenceException) { }
                catch(System.NullReferenceException) { }
            }

            if(existingFibers.Length < bundleSize)
            {
                //need to spawn more fibers
                for(int i = existingFibers.Length; i < bundleSize; i++)
                {
                    root.InstantiatePrefab(fiberPrefab);
                }            
            }
            else if(existingFibers.Length > bundleSize)
            {
                //need to deactivate or delete extra fibers
                for(int i = existingFibers.Length - 1; i >= bundleSize; i--)
                {
                    existingFibers[i].gameObject.SetActive(false);

                    try
                    {
                        DestroyImmediate(existingFibers[i].gameObject);
                    }
                    catch(System.InvalidOperationException) { }
                }
            }

            //now have the correct number of active fibers
            return root.GetComponentsInChildren<TubeRenderer>();
        }

        static Vector3[] GetBundlePoints(Vector3 bundleCenter, Vector3 direction, Vector3 normal, float bundleRadius, int bundleSize)
        {
            Vector3[] points = new Vector3[bundleSize];

            if(bundleSize <= 1)
            {
                //just use the center point
                points[0] = bundleCenter;
            }
            else
            {
                //offset each fiber from center by its angle
                for(int i = 0; i < bundleSize; i++)
                {
                    float fiberAngle = (360f / bundleSize) * i;
                    Quaternion rotation = Quaternion.AngleAxis(fiberAngle, direction);
                    points[i] = bundleCenter + (rotation * normal * bundleRadius);
                }
            }

            return points;
        }

        void ColorFibers(TubeRenderer[] fibers, ColoringModes mode)
        {
            //apply the same material to all fibers
            if(FiberMaterial)
            {
                for(int i = 0; i < fibers.Length; i++)
                {
                    Renderer r = fibers[i].GetComponent<Renderer>();
                    if(r) r.sharedMaterial = FiberMaterial;
                }
            }

            //apply a color to each fiber
            if(Colors.Count > 0)
            {
                switch(mode)
                {
                    default:
                    case ColoringModes.Single:
                        //apply same color to all fibers
                        for(int i = 0; i < fibers.Length; i++)
						{
                            ColorFiber(fibers[i], Colors[0]);
						}
                        break;

                    case ColoringModes.Striped:
                        //apply colors in the order they appear in the list
                        for(int i = 0; i < fibers.Length; i++)
                        {
                            ColorFiber(fibers[i], Colors[i % Colors.Count]);
                        }
                        break;

                    case ColoringModes.Random:
                        //apply a random color from the list to each fiber
                        for(int i = 0; i < fibers.Length; i++)
                        {
                            ColorFiber(fibers[i], Colors[Random.Range(0, Colors.Count)]);
                        }
                        break;
                }
            }
        }

        void ColorFiber(TubeRenderer fiber, Color color)
		{
            PerRendererColor prc = fiber.GetComponent<PerRendererColor>();
            if(!prc) prc = fiber.gameObject.AddComponent<PerRendererColor>();

            prc.Properties = FiberProperties;
            prc.ColorName = ColorName;

            prc.Color = color;
            prc.SetColor();
            //now, the PerRendererColor will set the color it was given on its own when the application runs
		}

        void RemoveFibers()
        {
            //delete the existing fiber prefab instances so new ones can be created
            for(int i = transform.childCount - 1; i >= 0; i --)
            {
                GameObject g = transform.GetChild(i).gameObject;

                try
                {
                    DestroyImmediate(g);
                }
                catch(System.InvalidOperationException)
                {
                    //tried to destroy an object from the parent prefab
                    //just disable it and all of its components instead
                    TubeRenderer tr = g.GetComponent<TubeRenderer>();
                    MeshFilter mf = g.GetComponent<MeshFilter>();
                    MeshRenderer mr = g.GetComponent<MeshRenderer>();

                    if(tr) DestroyImmediate(tr);
                    if(mf) DestroyImmediate(mf);
                    if(mr) DestroyImmediate(mr);

                    g.SetActive(false);
                }
            }
        }

        public void OnNotify()
        {
            Generate();
        }

#if UNITY_EDITOR
        [CanEditMultipleObjects]
        [CustomEditor(typeof(SimpleCustomFiber))]
        class Editor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                serializedObject.Update();

                //draw properties
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_path)));
                bool changed = EditorGUI.EndChangeCheck();

                //separate change check for the prefab
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_fiberPrefab)));
                bool prefabChanged = EditorGUI.EndChangeCheck();
                changed |= prefabChanged;

                EditorGUI.BeginChangeCheck();

                //show material
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_fiberMaterial)));

                SerializedProperty fiberProperties = serializedObject.FindProperty(nameof(_fiberProperties));
                EditorGUILayout.PropertyField(fiberProperties);

                SerializedProperty colorName = serializedObject.FindProperty(nameof(_colorName));
                ShaderProperties fp = fiberProperties.objectReferenceValue as ShaderProperties;
                if(fp)
                {
                    colorName.stringValue = fp.DrawPropertyMenu(ShaderProperties.PropertyTypes.Color,
                                                                                colorName.stringValue,
                                                                                "Color Property");
                }

                SerializedProperty coloringMode = serializedObject.FindProperty(nameof(_coloringMode));
                EditorGUILayout.PropertyField(coloringMode);

                //show list of colors
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_colors)), true);

                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_bundleSize)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_overallRadius)));

                //different radius modes
                SerializedProperty autoRadius = serializedObject.FindProperty(nameof(_autoRadius));
                EditorGUILayout.PropertyField(autoRadius);
                if(!autoRadius.boolValue)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_fiberRadius)));
                }
                changed |= EditorGUI.EndChangeCheck();

                serializedObject.ApplyModifiedProperties();

                if(prefabChanged)
                {
                    //remove the old fibers so they will be remade
                    foreach(SimpleCustomFiber cf in targets)
                    {
                        cf.RemoveFibers();
                    }
                }

                if(GUILayout.Button("Generate") || changed)
                {
                    //some setting changed -> update the fibers
                    foreach(SimpleCustomFiber cf in targets)
                    {
                        cf.Generate();
                    }
                }
            }
        }
#endif
    }
}