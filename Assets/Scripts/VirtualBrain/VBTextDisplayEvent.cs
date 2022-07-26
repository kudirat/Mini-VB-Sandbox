using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VBTesting
{
    public class VBTextDisplayEvent : MonoBehaviour
    {
        [SerializeField]
        VBTextDisplay TextDisplay;

        public void Open()
		{
            TextDisplay.Open(null);
        }

        public void Close()
		{
            TextDisplay.Close();
		}

        public void Toggle()
		{
            TextDisplay.Toggle(null);
		}
		
		public void CloseSpecific(int page)
		{
			TextDisplay.CloseSpecific(page);
		}
    }
}