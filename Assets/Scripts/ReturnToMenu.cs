using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;

public class ReturnToMenu : MonoBehaviour
{



    void Update()
    {
        Scene currScene = SceneManager.GetActiveScene();
        string currSystem = currScene.name;
        //string currSystem = "SomatosensorySystemPC";
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.UnloadScene(currSystem);
            SceneManager.LoadScene("VirtualBrainPC");
        }

    }

    // void ReturnHome()
    // {
    //   SceneManager.LoadScene("VirtualBrainPC");
    // }
}

