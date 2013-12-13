using UnityEngine;
using System.Collections;

public class TireControl : MonoBehaviour {
	
	public Transform m_frontLeft;
	public Transform m_frontRight;
	
	private InputController inputController;
	
	void Awake()
	{
		inputController = gameObject.GetComponent<InputController>();	
	}
	
	// Update is called once per frame
	void Update () {
		
		m_frontLeft.localRotation = Quaternion.Euler(0.0f, 35 * inputController.Left_X, 0.0f);
		m_frontRight.localRotation = Quaternion.Euler(0.0f, 35 * inputController.Left_X, 0.0f);
	}
}
