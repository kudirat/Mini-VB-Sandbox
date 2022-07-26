using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIDVE.Graphics;
using WIDVE.DataStructures;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VBTesting
{
    public class FiberColorFromMesh : MonoBehaviour
    {
        enum Modes { Tube, Mesh }

        [SerializeField]
        Modes FiberMode = Modes.Tube;

        [SerializeField]
        MeshOctree SourceMesh;

        [SerializeField]
        ShaderProperties MeshProperties;

        [SerializeField]
        string TextureName;

        [SerializeField]
        ShaderProperties FiberProperties;

        [SerializeField]
        string ColorName;

        public void SetFiberColor(TubeRenderer fiber, MeshOctree sourceMesh)
        {
            //set up per renderer color
            PerRendererColor prc = fiber.GetComponent<PerRendererColor>();
            if(!prc) prc = fiber.gameObject.AddComponent<PerRendererColor>();
            prc.Properties = FiberProperties;
            prc.ColorName = ColorName;

            //find closest point on mesh to fiber start or end
            Transform fiberTransform = fiber.transform;
            Vector3 fiberStart = fiberTransform.TransformPoint(fiber.points[0]);
            Vector3 fiberEnd = fiberTransform.TransformPoint(fiber.points[fiber.points.Length - 1]);

            int cvStart = sourceMesh.Vertices.GetClosestItem(fiberStart);
            int cvEnd = sourceMesh.Vertices.GetClosestItem(fiberEnd);

            Mesh mesh = sourceMesh.SourceMesh.sharedMesh;
            Transform meshTransform = sourceMesh.SourceMesh.transform;
            float dStart = (fiberStart - meshTransform.TransformPoint(mesh.vertices[cvStart])).sqrMagnitude;
            float dEnd = (fiberEnd - meshTransform.TransformPoint(mesh.vertices[cvEnd])).sqrMagnitude;

            int cV = dStart < dEnd ? cvStart : cvEnd;

            //color fiber based on closest vertex
            MeshRenderer meshRenderer = sourceMesh.SourceMesh.GetComponent<MeshRenderer>();
            if(meshRenderer)
            {
                //sample color from texture
                Material meshMaterial = meshRenderer.sharedMaterial;
                Texture2D texture = meshMaterial.GetTexture(TextureName) as Texture2D;
                Vector2 uv = sourceMesh.SourceMesh.sharedMesh.uv[cV];
                Color meshColor = texture.GetPixelBilinear(uv.x, uv.y);

                //apply color to fiber
                prc.Color = meshColor;
                prc.SetColor();
#if UNITY_EDITOR
                EditorUtility.SetDirty(prc);
#endif
            }
        }

        public void SetFiberColor(MeshRenderer fiber, MeshOctree sourceMesh)
		{
            //set up per renderer color
            PerRendererColor prc = fiber.GetComponent<PerRendererColor>();
			if(!prc) prc = fiber.gameObject.AddComponent<PerRendererColor>();
            prc.Properties = FiberProperties;
            prc.ColorName = ColorName;

            MeshRenderer meshRenderer = sourceMesh.GetComponent<MeshRenderer>();
            MeshFilter fiberMesh = fiber.GetComponent<MeshFilter>();

            //try either first or last fiber point
            //Vector3 meshPoint = sourceMesh.bounds.center;
            //Vector3 startPoint = fiber.transform.TransformPoint(fiberMesh.sharedMesh.vertices[0]);
            //Vector3 endPoint = fiber.transform.TransformPoint(fiberMesh.sharedMesh.vertices[fiberMesh.sharedMesh.vertices.Length - 1]);
            //float d_s = Vector3.Distance(meshPoint, startPoint);
            //float d_e = Vector3.Distance(meshPoint, endPoint);
            //Vector3 fiberPoint = d_s < d_e ? startPoint : endPoint;

            //this still needs to be updated to use the new octree
            int closestPoint_m = 0;
            int closestPoint_f = 0;
            float closest_d = float.MaxValue;
            float d;
            for(int i = 0; i < sourceMesh.SourceMesh.sharedMesh.vertices.Length; i++)
			{
                Vector3 m_p = sourceMesh.transform.TransformPoint(sourceMesh.SourceMesh.sharedMesh.vertices[i]);
                for(int j = 0; j < fiberMesh.sharedMesh.vertices.Length; j++)
				{
                    Vector3 f_p = fiberMesh.transform.TransformPoint(fiberMesh.sharedMesh.vertices[j]);
                    d = Vector3.Distance(f_p, m_p);
                    if(d < closest_d)
				    {
                        closest_d = d;
                        closestPoint_m = i;
                        closestPoint_f = j;
				    }
                }
			}
            //Debug.Log($"Closest point to fiber {fiber.name} is {closestPoint_m}.");

            //sample texture color from source mesh point
            Material meshMaterial = meshRenderer.sharedMaterial;
            Texture2D texture = meshMaterial.GetTexture(TextureName) as Texture2D;
            Vector2 uv = sourceMesh.SourceMesh.sharedMesh.uv[closestPoint_m];
            Color meshColor = texture.GetPixelBilinear(uv.x, uv.y);

            //apply color to fiber
            prc.Color = meshColor;
            prc.SetColor();
#if UNITY_EDITOR
            EditorUtility.SetDirty(prc);
#endif
        }

        public void SetColors()
		{
            if(FiberMode == Modes.Tube)
            {
                TubeRenderer[] fibers = GetComponentsInChildren<TubeRenderer>(false);
                for(int i = 0; i < fibers.Length; i++)
                {
                    SetFiberColor(fibers[i], SourceMesh);
                }
            }
            else if(FiberMode == Modes.Mesh)
            {
                MeshRenderer[] fiberMeshes = GetComponentsInChildren<MeshRenderer>(true);
                for(int i = 0; i < fiberMeshes.Length; i++)
                {
                    SetFiberColor(fiberMeshes[i], SourceMesh);
                }
            }
		}

#if UNITY_EDITOR
		[CustomEditor(typeof(FiberColorFromMesh))]
        [CanEditMultipleObjects]
        class Editor : UnityEditor.Editor
		{
			public override void OnInspectorGUI()
            {
                serializedObject.Update();

                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(FiberMode)));

                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SourceMesh)));

                SerializedProperty meshProperties = serializedObject.FindProperty(nameof(MeshProperties));
                EditorGUILayout.PropertyField(meshProperties);

                SerializedProperty textureName = serializedObject.FindProperty(nameof(TextureName));
                ShaderProperties mp = meshProperties.objectReferenceValue as ShaderProperties;
                if(mp)
                {
                    textureName.stringValue = mp.DrawPropertyMenu(ShaderProperties.PropertyTypes.Texture,
                                                                  textureName.stringValue,
                                                                  textureName.displayName);
                }

                SerializedProperty fiberProperties = serializedObject.FindProperty(nameof(FiberProperties));
                EditorGUILayout.PropertyField(fiberProperties);

                SerializedProperty colorName = serializedObject.FindProperty(nameof(ColorName));
                ShaderProperties fp = fiberProperties.objectReferenceValue as ShaderProperties;
				if(fp)
				{
                    colorName.stringValue = fp.DrawPropertyMenu(ShaderProperties.PropertyTypes.Color,
                                                                colorName.stringValue,
                                                                colorName.displayName);
				}

                serializedObject.ApplyModifiedProperties();

                if(GUILayout.Button("Set Colors"))
				{
                    foreach(FiberColorFromMesh fcfm in targets)
					{
                        fcfm.SetColors();
					}
				}
			}
		}
#endif
    }
}