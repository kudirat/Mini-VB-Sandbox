using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIDVE.Patterns;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace VBTesting {
	public class VBTextDisplay : MonoBehaviour
	{
		[SerializeField]
		GameObject Canvas;

		[SerializeField]
		LessonSettings CurrentSettings;

		[SerializeField]
		bool FreezePlayer = false;

		[SerializeField]
		bool FreezeMouseLook = false;

		[SerializeField]
		[HideInInspector]
		List<GameObject> Pages;

		int ActivePageIndex = 0;

		public bool IsOpen => Canvas.activeSelf;

		Command ShowCursorCommand;

		public void NextPage()
		{
			SetPage(ActivePageIndex + 1);
		}

		public void PreviousPage()
		{
			SetPage(ActivePageIndex - 1);
		}

		public void SetPage(GameObject page)
		{
			int pageIndex = Pages.IndexOf(page);
			SetPage(pageIndex);
		}

		public void SetPage(int pageIndex)
		{
			foreach(GameObject page in Pages)
			{
				page.SetActive(false);
			}
			ActivePageIndex = Mathf.Clamp(pageIndex, 0, Pages.Count - 1);
			Pages[ActivePageIndex].SetActive(true);
		}

		public void Open(Command showCursorCommand)
		{
			Canvas.SetActive(true);

			ShowCursorCommand = showCursorCommand;
			if(ShowCursorCommand != null) ShowCursorCommand.Execute();

			if(FreezePlayer) CurrentSettings.FreezePlayer(true);
			if(FreezeMouseLook) CurrentSettings.NotifyMenu(true);

			SetPage(ActivePageIndex);
		}

		public void Close()
		{
			if(ShowCursorCommand != null)
			{
				ShowCursorCommand.Undo();
				ShowCursorCommand = null;
			}

			CurrentSettings.FreezePlayer(false);
			CurrentSettings.NotifyMenu(false);

			Canvas.SetActive(false);
		}

		public void CloseSpecific(int page)
		{
			Pages[page].SetActive(false);
		}
		
		public void Toggle(Command showCursorCommand)
		{
			if(IsOpen) Close();
			else Open(showCursorCommand);
		}

#if UNITY_EDITOR
		[CustomEditor(typeof(VBTextDisplay))]
		[CanEditMultipleObjects]
		class Editor : UnityEditor.Editor
		{
			ReorderableList Pages;

			protected virtual void OnEnable()
			{
				Pages = new ReorderableList(serializedObject, serializedObject.FindProperty(nameof(Pages)),
											true, true, true, true);

				Pages.drawHeaderCallback = rect =>
				{
					EditorGUI.LabelField(rect, "Pages");
				};

				Pages.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
				{
					SerializedProperty element = Pages.serializedProperty.GetArrayElementAtIndex(index);
					EditorGUI.PropertyField(position: new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
											property: element,
											label: new GUIContent(index.ToString()));
				};
			}

			public override void OnInspectorGUI()
			{
				serializedObject.Update();
				DrawDefaultInspector();
				Pages.DoLayoutList();
				serializedObject.ApplyModifiedProperties();
			}
		}
#endif
	}
}