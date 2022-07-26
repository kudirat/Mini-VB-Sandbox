using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using WIDVE.Utilities;

namespace VBTesting
{
	[ExecuteAlways]
	public class Label : MonoBehaviour
	{
		[SerializeField]
		Canvas _canvas;
		Canvas Canvas => _canvas;

		[SerializeField]
		RectTransform _background;
		RectTransform Background => _background;

		[SerializeField]
		Gizmo _backgroundGizmo;
		Gizmo BackgroundGizmo => _backgroundGizmo;

		[SerializeField]
		TMP_Text _tmpText;
		TMP_Text TMPText => _tmpText;

		[Header("Settings")]

		[SerializeField]
		[Range(0, 10)]
		float _backgroundPadding = 1f;
		float BackgroundPadding => _backgroundPadding;

		[SerializeField]
		Vector3 _offset = Vector3.zero;
		Vector3 Offset => _offset;

		[SerializeField]
		[TextArea]
		string _text;
		string Text => _text;

		void UpdateLabel()
		{
			UpdateText();

			UpdateBackground();

			UpdateOffset();
		}

		void UpdateText()
		{
			if(!TMPText) return;

			if(string.IsNullOrEmpty(TMPText.text)) return;

			TMPText.text = Text;
		}

		void UpdateBackground()
		{
			if(!TMPText) return;

			Bounds b = TMPText.bounds;
			Vector2 backgroundSize = new Vector2(b.size.x, b.size.y) + ((BackgroundPadding / transform.localScale.x) * Vector2.one);

			if(Background) Background.sizeDelta = backgroundSize;
			if(BackgroundGizmo) BackgroundGizmo.transform.localScale = backgroundSize;
		}

		void UpdateOffset()
		{
			if(!Canvas) return;

			Canvas.transform.localPosition = Offset;
		}

		void OnEnable()
		{
			UpdateLabel();
		}

		void OnValidate()
		{
			UpdateLabel();
		}
	}
}