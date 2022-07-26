using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace VBTesting
{
    public class ClosedCaptionsMirror : MonoBehaviour
    {
        [SerializeField]
        TextMeshProUGUI _text;
        TextMeshProUGUI Text => _text;

        [SerializeField]
        ClosedCaptions _captions;
        public ClosedCaptions Captions => _captions;

        ClosedCaptions _master;
        public ClosedCaptions Master
        {
            get => _master;
            private set => _master = value;
        }

        public void SetMaster(ClosedCaptions master)
        {
            if(Master) Master.OnCaptionChanged -= SetText;
            Master = master;
            if(Master) Master.OnCaptionChanged += SetText;
        }

        public void SetText(string text)
        {
            Text.gameObject.SetActive(false);
            Text.text = text;
            Text.gameObject.SetActive(true);
        }
    }
}