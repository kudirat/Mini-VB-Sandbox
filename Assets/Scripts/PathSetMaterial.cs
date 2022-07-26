using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathSetMaterial : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

	public void SetMaterial(Material materialToSet)
	{
		transform.parent.gameObject.GetComponent<MeshRenderer>().sharedMaterial = materialToSet;
	}

}
