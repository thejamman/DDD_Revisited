using UnityEngine;
using System.Collections;

public class TestButton : MonoBehaviour 
{

	// Use this for initialization
	void Start () 
    {
	
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}

    void OnMouseDown()
    {
        Debug.Log("LAME");

    }

    private IEnumerator ScaleButton()
    {
         yield return new WaitForSeconds(0.2f);
    }
}
