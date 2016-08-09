using UnityEngine;
using System.Collections;

public class AntiRollBar : MonoBehaviour {
	
	public WheelCollider m_leftWheelCollider;
	public WheelCollider m_rightWheelCollider;
	public float m_antiRoll;
	
	// Update is called once per frame
	void FixedUpdate () {
	
		WheelHit hit = new WheelHit();
		float travelL = 1.0f;
		float travelR = 1.0f;
		
		bool groundedL = m_leftWheelCollider.GetGroundHit(out hit);
		if (groundedL)
		{
			travelL = (-m_leftWheelCollider.transform.InverseTransformPoint(hit.point).y - m_leftWheelCollider.radius) / m_leftWheelCollider.suspensionDistance;	
		}
		
		bool groundedR = m_rightWheelCollider.GetGroundHit(out hit);
		if (groundedR)
		{
			travelR = (-m_rightWheelCollider.transform.InverseTransformPoint(hit.point).y - m_rightWheelCollider.radius) / m_rightWheelCollider.suspensionDistance;	
		}
		
		float antiRollForce = (travelL - travelR) * m_antiRoll;
		
		if (groundedL)
		{
			GetComponent<Rigidbody>().AddForceAtPosition(m_leftWheelCollider.transform.up * -antiRollForce, m_leftWheelCollider.transform.position);	
		}
		if (groundedR)
		{
			GetComponent<Rigidbody>().AddForceAtPosition(m_rightWheelCollider.transform.up * antiRollForce, m_rightWheelCollider.transform.position);
		}
	}
}
