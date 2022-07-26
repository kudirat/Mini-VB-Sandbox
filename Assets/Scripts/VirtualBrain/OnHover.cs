/*
 * Added to VB Project by Lauren Ciha from an online source:
 * https://tutorialsforvr.com/handling-ui-events-unity-vr/
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic; // for the List class
using UnityEngine.EventSystems;

public class OnHover : MonoBehaviour//, //IPointerEnterHandler, IPointerExitHandler
{
    GraphicRaycaster m_Raycaster;
    PointerEventData m_PointerEventData;
    EventSystem m_EventSystem;

    void Start()
    {
        //Fetch the Raycaster from the GameObject (the Canvas)
        m_Raycaster = GetComponent<GraphicRaycaster>();
        //Fetch the Event System from the Scene
        m_EventSystem = GetComponent<EventSystem>();
    }

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
            m_Raycaster.Raycast(m_PointerEventData, results);

            //For every result returned, output the name of the GameObject on the Canvas hit by the Ray
            foreach (RaycastResult result in results)
            {
                Debug.Log("Hit " + result.gameObject.name);
            }
        }
    }
}
    /*
    PointerEventData _pointereventdata;
    List<GameObject> hovering; // when I added this, the scene won't load (probably too much information at once that's constantly updating)

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("OnPointerEnter()");
        _pointereventdata = eventData;

        if (_pointereventdata != null)
        {
            Debug.Log("_pointereventdata = " + _pointereventdata); // right now, it's getting center, which is the highest possible gameobject it could find
            hovering = _pointereventdata.hovered;
            Debug.Log("selectObject = " + hovering);
        }

       // OnHoverEnter(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("OnPointerExit()");
        //OnHoverExit();
    }

    void OnHoverEnter(PointerEventData eventData)
    {
        Image image = eventData.selectedObject.GetComponent<Image>();
        Debug.Log("OnHoverEnter() Button: " + image);
        image.color = Color.white;
    }

    void OnHoverExit()
    {
       // selectedObject.image.color = Color.gray;
    }
    */
//}
