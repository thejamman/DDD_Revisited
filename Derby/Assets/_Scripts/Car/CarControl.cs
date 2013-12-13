using UnityEngine;
using System.Collections;

public class CarControl : MonoBehaviour {
	
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
	
	//Gearing
	[SerializeField]
	private float[] m_GearRatio = new float[6]{ 3.62f, 2.61f, 1.88f, 1.41f, 1f, 0.7f};
	[SerializeField]
	private float m_DifferentialRatio = 4.10f;
	[SerializeField]
	private int m_CurrentGear = 0;
	
	//Engine
	[SerializeField]
	private float m_EngineHorsepower = 300f;
	[SerializeField]
	private float m_MaxEngineRPM = 6000f;
	[SerializeField]
	private float m_MinEngineRPM = 200f;
	[SerializeField]
	private float m_EngineRPM = 0f;
	[SerializeField]
	private float m_TopSpeed = 50;
	
	//Body
	[SerializeField]
	private float m_CenterOfMassX = 0f;
	[SerializeField]
	private float m_CenterOfMassY = -0.5f;
	[SerializeField]
	private float m_CenterOfMassZ = 0.5f;
	
	private Vector3 m_BodyCenterOfMass;
	
	private InputController inputController;
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
	
	//Gearing
	public float[] GearRatio
	{
		get{ return m_GearRatio; }
		set{ m_GearRatio = value; }
	}
	public float DifferentialRatio
	{
		get{ return m_DifferentialRatio; }
		set{ m_DifferentialRatio = value; }
	}
	public int CurrentGear
	{
		get{ return m_CurrentGear; }
		set{ m_CurrentGear = value; }
	}
	
	//Engine
	public float EngineHorsepower
	{
		get{ return m_EngineHorsepower; }
		set{ m_EngineHorsepower = value; }
	}
	public float MaxEngineRPM
	{
		get{ return m_MaxEngineRPM; }
		set{ m_MaxEngineRPM = value; }
	}
	public float MinEngineRPM
	{
		get{ return m_MinEngineRPM; }
		set{ m_MinEngineRPM = value; }
	}
	public float EngineRPM
	{
		get{ return m_EngineRPM; }
		set{ m_EngineRPM = value; }
	}
	public float TopSpeed
	{
		get{ return m_TopSpeed; }
		set{ m_TopSpeed = value; }
	}
	
	//Body
	public float CenterOfMassX
	{
		get{ return m_CenterOfMassX; }
		set{ m_CenterOfMassX = value; }
	}
	public float CenterOfMassY
	{
		get{ return m_CenterOfMassY; }
		set{ m_CenterOfMassY = value; }
	}
	public float CenterOfMassZ
	{
		get{ return m_CenterOfMassZ; }
		set{ m_CenterOfMassZ = value; }
	}
	
	//Input Controller
	public InputController InputController
	{
		get { return inputController; }
	}
	#endregion Public Properties
	
	void Start () 
	{
		InitCarRigidbodyProperties();
		inputController = gameObject.GetComponent<InputController>();
	}
	
	void InitCarRigidbodyProperties()
	{
		this.rigidbody.centerOfMass = new Vector3(this.CenterOfMassX, this.CenterOfMassY, this.CenterOfMassZ);
		m_BodyCenterOfMass = this.rigidbody.centerOfMass;
	}
	
	void FixedUpdate () 
	{
		this.rigidbody.centerOfMass = m_BodyCenterOfMass;
		rigidbody.drag = rigidbody.velocity.magnitude / 250;
		
		this.EngineRPM = Mathf.Abs(this.BackLeftWheel.rpm + this.BackRightWheel.rpm)/2 * this.GearRatio[m_CurrentGear] * this.DifferentialRatio;
		if ( this.EngineRPM>10000) {this.EngineRPM =10000;}
		if ( this.EngineRPM<0) {this.EngineRPM =0;}
		this.ShiftGears();
		
//		audio.pitch = Mathf.Abs(this.EngineRPM / this.MaxEngineRPM) + 0.5f ;
//		if ( audio.pitch > 1.5f ) {
//			audio.pitch = 1.5f;
//		}

		this.FrontLeftWheel.steerAngle = 35 * InputController.Left_X;
		this.FrontRightWheel.steerAngle = 35 * InputController.Left_X;
		
		if(rigidbody.velocity.magnitude > this.TopSpeed)
        {
			this.BackLeftWheel.motorTorque = 0;
			this.BackRightWheel.motorTorque = 0;
			this.FrontLeftWheel.motorTorque = 0;
			this.FrontRightWheel.motorTorque = 0;
			return;
		}
		
		this.BackLeftWheel.motorTorque = -this.EngineHorsepower * this.GearRatio[m_CurrentGear] * this.DifferentialRatio * InputController.Right_Y * 100;
		this.BackRightWheel.motorTorque = -this.EngineHorsepower * this.GearRatio[m_CurrentGear] * this.DifferentialRatio * InputController.Right_Y * 100;
		this.FrontLeftWheel.motorTorque = -(this.EngineHorsepower / 2) * this.GearRatio[m_CurrentGear] * this.DifferentialRatio * InputController.Right_Y * 100;
		this.FrontRightWheel.motorTorque = -(this.EngineHorsepower / 2) * this.GearRatio[m_CurrentGear] * this.DifferentialRatio * InputController.Right_Y * 100;
	}
	
	void ShiftGears()
	{
		int appropriateGear = 0;
		
		if ( this.EngineRPM >= this.MinEngineRPM ) 
		{
			appropriateGear = this.CurrentGear;
			
			for ( var i = 0; i < this.GearRatio.Length; i ++ ) {
				if ( Mathf.Abs(this.BackLeftWheel.rpm + this.BackRightWheel.rpm) * 0.5f * this.GearRatio[i] * this.DifferentialRatio < this.MaxEngineRPM ) {
					appropriateGear = i;
					break;
				}
			}
			
			this.CurrentGear = appropriateGear;
		}
		
		if ( this.EngineRPM <= this.MinEngineRPM ) 
		{
			appropriateGear = this.CurrentGear;
			
			for ( var j = this.GearRatio.Length-1; j >= 0; j -- ) 
			{
				if ( Mathf.Abs(this.BackLeftWheel.rpm + this.BackRightWheel.rpm) * 0.5f * this.GearRatio[j] * this.DifferentialRatio > this.MinEngineRPM ) 
				{
					appropriateGear = j;
					break;
				}
			}
			
			this.CurrentGear = appropriateGear;
		}
	}
}
