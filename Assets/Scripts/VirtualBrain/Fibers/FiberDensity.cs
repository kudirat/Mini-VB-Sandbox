using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VBTesting
{
	public class FiberDensity : MonoBehaviour
	{
		[SerializeField]
		[Range(0, 1)]
		float Density = 1;

		/// <summary>
		/// Disables random fibers until the desired density is reached.
		/// </summary>
		/// <param name="parent">Parent object that contains fibers.</param>
		/// <param name="density">Percentage of fibers to keep active (range 0-1).</param>
		public void SetDensity(float density)
		{
			//get fibers
			MeshRenderer[] allFibers = gameObject.GetComponentsInChildren<MeshRenderer>(true);

			//turn on/off some fibers randomly
			density = Mathf.Clamp01(density);
			foreach(MeshRenderer fiber in allFibers)
			{
				bool active = Random.Range(0f, 1f) < density;
				fiber.gameObject.SetActive(active);
#if UNITY_EDITOR
				EditorUtility.SetDirty(fiber.gameObject);
#endif
			}
		}

#if UNITY_EDITOR
		[CustomEditor(typeof(FiberDensity))]
		[CanEditMultipleObjects]
		class Editor : UnityEditor.Editor
		{
			public override void OnInspectorGUI()
			{
				EditorGUI.BeginChangeCheck();

				base.OnInspectorGUI();

				bool changed = EditorGUI.EndChangeCheck();

				if(changed)
				{
					foreach(FiberDensity fd in targets)
					{
						fd.SetDensity(fd.Density);
					}
				}
			}
		}
#endif
	}
}