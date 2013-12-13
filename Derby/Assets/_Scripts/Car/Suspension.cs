using UnityEngine;
using System.Collections;

public class Suspension : MonoBehaviour {

	#region Private Members
	//Wheels
	[SerializeField]
	private WheelCollider m_FrontLeftWheel;
	[SerializeField]
	private WheelCollider m_FrontRightWheel;
	[SerializeField]
	private WheelCollider m_BackLeftWheel;
	[SerializeField]
	private WheelCollider m_BackRightWheel;
	
	[SerializeField]
	private float m_WheelRadius = 0.33f;
	[SerializeField]
	private float m_SpringLength = 0.1f;
	[SerializeField]
	private float m_DamperForce = 50f;
	[SerializeField]
	private float m_SpringForceFront = 8500f;
	[SerializeField]
	private float m_SpringForceRear = 5500f;
	
	
	private JointSpring m_FrontLeftSpring;
	private JointSpring m_FrontRightSpring;
	private JointSpring m_BackLeftSpring;
	private JointSpring m_BackRightSpring;
	#endregion Private Members
	
	
	#region Public Properties
	//Wheels
	public WheelCollider FrontLeftWheel
	{
		get{ return m_FrontLeftWheel; }
		set{ m_FrontLeftWheel = value; }
	}
	public WheelCollider FrontRightWheel
	{
		get{ return m_FrontRightWheel; }
		set{ m_FrontRightWheel = value; }
	}
	public WheelCollider BackLeftWheel
	{
		get{ return m_BackLeftWheel; }
		set{ m_BackLeftWheel = value; }
	}
	public WheelCollider BackRightWheel
	{
		get{ return m_BackRightWheel; }
		set{ m_BackRightWheel = value; }
	}
	
	public float WheelRadius
	{
		get{ return m_WheelRadius; }
		set{ m_WheelRadius = value; }
	}
	public float SpringLength
	{
		get{ return m_SpringLength; }
		set{ m_SpringLength = value; }
	}
	public float DamperForce
	{
		get{ return m_DamperForce; }
		set{ m_DamperForce = value; }
	}
	public float SpringForceFront
	{
		get{ return m_SpringForceFront; }
		set{ m_SpringForceFront = value; }
	}
	public float SpringForceRear
	{
		get{ return m_SpringForceRear; }
		set{ m_SpringForceRear = value; }
	}
	#endregion Public Properties
	
	void  Update () 
	{
		// Forward Suspension
		
		this.FrontLeftWheel.radius= this.WheelRadius;	
		this.FrontLeftWheel.suspensionDistance = this.SpringLength;
		
		m_FrontLeftSpring = this.FrontLeftWheel.suspensionSpring;
		m_FrontLeftSpring.spring = this.SpringForceFront;
		m_FrontLeftSpring.damper = this.DamperForce;
		this.FrontLeftWheel.suspensionSpring = m_FrontLeftSpring;
		
		this.FrontRightWheel.radius= this.WheelRadius;
		this.FrontRightWheel.suspensionDistance = this.SpringLength;
		
		m_FrontRightSpring = this.FrontRightWheel.suspensionSpring;
		m_FrontRightSpring.spring = this.SpringForceFront;
		m_FrontRightSpring.damper = this.DamperForce;
		this.FrontRightWheel.suspensionSpring = m_FrontRightSpring;
		
		// Rear Suspension	
		this.BackLeftWheel.radius= this.WheelRadius;	
		this.BackLeftWheel.suspensionDistance = this.SpringLength;
		
		m_BackLeftSpring = this.BackLeftWheel.suspensionSpring;
		m_BackLeftSpring.spring = this.SpringForceRear;
		m_BackLeftSpring.damper = this.DamperForce;
		this.BackLeftWheel.suspensionSpring = m_BackLeftSpring;
		
		this.BackRightWheel.radius= this.WheelRadius;
		this.BackRightWheel.suspensionDistance = this.SpringLength;
		
		m_BackRightSpring = this.BackRightWheel.suspensionSpring;
		m_BackRightSpring.spring = this.SpringForceRear;
		m_BackRightSpring.damper = this.DamperForce;
		this.BackRightWheel.suspensionSpring = m_BackRightSpring;
	}
}
