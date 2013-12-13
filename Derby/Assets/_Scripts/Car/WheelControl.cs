using UnityEngine;
using System.Collections;

public class WheelControl : MonoBehaviour {

	public WheelCollider m_ColliderForWheel;
	public GameObject m_SkidPrefab;
	private float m_Rotation;
	
	void Update()
	{
		RaycastHit hit = new RaycastHit();
		
		Vector3 centerOfCollider = m_ColliderForWheel.transform.TransformPoint(m_ColliderForWheel.center);
		
		if(Physics.Raycast(centerOfCollider, -m_ColliderForWheel.transform.up, out hit, m_ColliderForWheel.suspensionDistance + m_ColliderForWheel.radius))
		{
			transform.position = hit.point + (m_ColliderForWheel.transform.up * m_ColliderForWheel.radius);
		}
		else
		{
			transform.position = centerOfCollider - (m_ColliderForWheel.transform.up * m_ColliderForWheel.suspensionDistance);
		}
		
		transform.rotation = m_ColliderForWheel.transform.rotation * Quaternion.Euler(m_Rotation, m_ColliderForWheel.steerAngle, 0);
		m_Rotation += m_ColliderForWheel.rpm * (360/60) * Time.deltaTime;
		
		
		WheelHit wheelHit = new WheelHit();
		m_ColliderForWheel.GetGroundHit( out wheelHit );
		
		if ( Mathf.Abs( wheelHit.sidewaysSlip ) > 2.0 ) 
		{
			if ( m_SkidPrefab ) 
			{
				Instantiate( m_SkidPrefab, wheelHit.point, Quaternion.identity );
			}
		}
	}
	
}
