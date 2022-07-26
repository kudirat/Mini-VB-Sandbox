using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace VBTesting
{
    public class VBButton : MonoBehaviour
    {
        [SerializeField]
        Button Button;

        [SerializeField]
        Image ButtonImage;

        [SerializeField]
        Image OutlineImage;

        [SerializeField]
        TextMeshProUGUI Text;

        public void SetSelected(bool selected)
		{
            //for now, just enable/disable the outline
            OutlineImage.enabled = selected;
		}
    }
}