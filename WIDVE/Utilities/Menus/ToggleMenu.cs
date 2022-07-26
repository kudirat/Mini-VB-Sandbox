using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WIDVE.Utilities
{
    public class ToggleMenu : MonoBehaviour
    {
        [SerializeField]
        ButtonFloat _toggleButton;
        ButtonFloat ToggleButton => _toggleButton;

        [SerializeField]
        Menu _menu;
        Menu Menu => _menu;

        private string _sceneName;
        private Scene _currScene;
        

        public void Toggle()
        {
            if(Menu.IsOpen) Menu.Close();
            else Menu.Open();
        }

        void Update()
        {
            _currScene = SceneManager.GetActiveScene();
            _sceneName = _currScene.name;

            if (!ToggleButton) return;

            if(ToggleButton.GetDown() && !(_sceneName == "EmptySystem"))
            {
                Toggle();
            }
        }
    }
}