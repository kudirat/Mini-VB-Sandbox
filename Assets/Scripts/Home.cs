using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;

public class Home : MonoBehaviour
{
    public void ReturnHome()
    {
        Scene currScene = SceneManager.GetActiveScene();
        string currSystem = currScene.name;
        //string currSystem = "SomatosensorySystemPC";
      
            SceneManager.UnloadScene(currSystem);
            SceneManager.LoadScene("VirtualBrainPC");
        
    }
}
