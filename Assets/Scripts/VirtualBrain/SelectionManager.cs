using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/*
 * I'm following this tutorial: https://www.youtube.com/watch?v=_yf5vzZ2sYE
 * */

public class SelectionManager : MonoBehaviour
{
    public GraphicRaycaster raycast;
    PointerEventData m_PointerEventData;
    public EventSystem m_EventSystem;

        void Update()
        {
            //Check if the left Mouse button is clicked
            if (Input.GetKey(KeyCode.Mouse0))
            {
                //Set up the new Pointer Event
                m_PointerEventData = new PointerEventData(m_EventSystem);
                //Set the Pointer Event Position to that of the mouse position
                m_PointerEventData.position = Input.mousePosition;

                //Create a list of Raycast Results
                List<RaycastResult> results = new List<RaycastResult>();

                //Raycast using the Graphics Raycaster and mouse click position
                raycast.Raycast(m_PointerEventData, results);

                //For every result returned, output the name of the GameObject on the Canvas hit by the Ray
                foreach (RaycastResult result in results)
                {
                    Debug.Log("Hit " + result.gameObject.name);
                }
            }
        }

        
}
