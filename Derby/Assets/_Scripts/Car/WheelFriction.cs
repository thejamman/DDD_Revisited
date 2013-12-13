using UnityEngine;
using System.Collections;

public class WheelFriction : MonoBehaviour {

	public WheelCollider FrontLeftWheel;
	public WheelCollider FrontRightWheel;
	public WheelCollider BackLeftWheel;
	public WheelCollider BackRightWheel;
	
	// Wheel Friction curves
	public float ForwardExtremumSlip = 10f;//1.0;
	public float ForwardExtremumValue = 100f;//20000.0;
	public float ForwardAsymptoteSlip = 100f;//2.0;
	public float ForwardAsymptoteValue = 1f;//10000.0;
	public float ForwardStiffness = 1.0f;
	
	public float SidewaysExtremumSlip = 0.02f;//1.0;
	public float SidewaysExtremumValue = 10000f;//20000.0;
	public float SidewaysAsymptoteSlip = 2f;//2.0;
	public float SidewaysAsymptoteValue = 5000f;//10000.0;
	public float SidewaysStiffness = 0.02f;

	private WheelFrictionCurve forwardLeftFriction;
	private WheelFrictionCurve forwardRightFriction;
	private WheelFrictionCurve backLeftFriction;
	private WheelFrictionCurve backRightFriction;
	
	private WheelFrictionCurve forwardLeftSideFriction;
	private WheelFrictionCurve forwardRightSideFriction;
	private WheelFrictionCurve backLeftSideFriction;
	private WheelFrictionCurve backRightSideFriction;
	
	
	void Start()
	{
		forwardLeftFriction = FrontLeftWheel.forwardFriction;
		forwardRightFriction = FrontRightWheel.forwardFriction;
		backLeftFriction = BackLeftWheel.forwardFriction;
		backRightFriction = BackRightWheel.forwardFriction;
		
		forwardLeftSideFriction = FrontLeftWheel.sidewaysFriction;
		forwardRightSideFriction = FrontRightWheel.sidewaysFriction;
		backLeftSideFriction = BackLeftWheel.sidewaysFriction;
		backRightSideFriction = BackRightWheel.sidewaysFriction;
	}
	void Update () {
		
		// Forward Friction
		forwardLeftFriction.extremumSlip = ForwardExtremumSlip;
		forwardLeftFriction.extremumValue = ForwardExtremumValue;
		forwardLeftFriction.asymptoteSlip = ForwardAsymptoteSlip;
		forwardLeftFriction.asymptoteValue = ForwardAsymptoteValue;
		forwardLeftFriction.stiffness = ForwardStiffness;
		
		forwardRightFriction.extremumSlip = ForwardExtremumSlip;
		forwardRightFriction.extremumValue = ForwardExtremumValue;
		forwardRightFriction.asymptoteSlip = ForwardAsymptoteSlip;
		forwardRightFriction.asymptoteValue = ForwardAsymptoteValue;
		forwardRightFriction.stiffness = ForwardStiffness;
	
		backLeftFriction.extremumSlip = ForwardExtremumSlip;
		backLeftFriction.extremumValue = ForwardExtremumValue;
		backLeftFriction.asymptoteSlip = ForwardAsymptoteSlip;
		backLeftFriction.asymptoteValue = ForwardAsymptoteValue;
		backLeftFriction.stiffness = ForwardStiffness;
			
		backRightFriction.extremumSlip = ForwardExtremumSlip;
		backRightFriction.extremumValue = ForwardExtremumValue;
		backRightFriction.asymptoteSlip = ForwardAsymptoteSlip;
		backRightFriction.asymptoteValue = ForwardAsymptoteValue;
		backRightFriction.stiffness = ForwardStiffness;
		
		FrontLeftWheel.forwardFriction = forwardLeftFriction;
		FrontRightWheel.forwardFriction = forwardRightFriction;
		BackLeftWheel.forwardFriction = backLeftFriction;
		BackRightWheel.forwardFriction = backRightFriction;
		
		//Sideways Friction
		forwardLeftSideFriction.extremumSlip = SidewaysExtremumSlip;
		forwardLeftSideFriction.extremumValue = SidewaysExtremumValue;
		forwardLeftSideFriction.asymptoteSlip = SidewaysAsymptoteSlip;
		forwardLeftSideFriction.asymptoteValue = SidewaysAsymptoteValue;
		forwardLeftSideFriction.stiffness = SidewaysStiffness;
			
		forwardRightSideFriction.extremumSlip = SidewaysExtremumSlip;
		forwardRightSideFriction.extremumValue = SidewaysExtremumValue;
		forwardRightSideFriction.asymptoteSlip = SidewaysAsymptoteSlip;
		forwardRightSideFriction.asymptoteValue = SidewaysAsymptoteValue;
		forwardRightSideFriction.stiffness = SidewaysStiffness;
		
		backLeftSideFriction.extremumSlip = SidewaysExtremumSlip;
		backLeftSideFriction.extremumValue = SidewaysExtremumValue;
		backLeftSideFriction.asymptoteSlip = SidewaysAsymptoteSlip;
		backLeftSideFriction.asymptoteValue = SidewaysAsymptoteValue;
		backLeftSideFriction.stiffness = SidewaysStiffness;
			
		backRightSideFriction.extremumSlip = SidewaysExtremumSlip;
		backRightSideFriction.extremumValue = SidewaysExtremumValue;
		backRightSideFriction.asymptoteSlip = SidewaysAsymptoteSlip;
		backRightSideFriction.asymptoteValue = SidewaysAsymptoteValue;
		backRightSideFriction.stiffness = SidewaysStiffness;
		
		FrontLeftWheel.sidewaysFriction = forwardLeftSideFriction;
		FrontRightWheel.sidewaysFriction = forwardRightSideFriction;
		BackLeftWheel.sidewaysFriction = backLeftSideFriction;
		BackRightWheel.sidewaysFriction = backRightSideFriction;

		// The rigidbody velocity is always given in world space, but in order to work in local space of the car model we need to transform it first.
		Vector3 relativeVelocity = transform.InverseTransformDirection(rigidbody.velocity);
		UpdateFriction(relativeVelocity);
	}

	void UpdateFriction(Vector3 relativeVelocity)
	{
		float sqrVel = relativeVelocity.x * relativeVelocity.x;
	
		// Forward
		float maxFStif = 300f;
		float minFStif = 200f;
		
		forwardLeftFriction.extremumValue = Mathf.Clamp(maxFStif - sqrVel, 0, maxFStif);
		forwardLeftFriction.asymptoteValue = Mathf.Clamp(minFStif - (sqrVel / 2), 0, minFStif);
		
		forwardRightFriction.extremumValue = Mathf.Clamp(maxFStif - sqrVel, 0, maxFStif);
		forwardRightFriction.asymptoteValue = Mathf.Clamp(minFStif - (sqrVel / 2), 0, minFStif);
		
		backLeftFriction.extremumValue = Mathf.Clamp(maxFStif - sqrVel, 10, maxFStif);
		backLeftFriction.asymptoteValue = Mathf.Clamp(minFStif - (sqrVel / 2), 10, minFStif);
			
		backRightFriction.extremumValue = Mathf.Clamp(maxFStif - sqrVel, 10, maxFStif);
		backRightFriction.asymptoteValue = Mathf.Clamp(minFStif - (sqrVel / 2), 10, minFStif);
		
		FrontLeftWheel.forwardFriction = forwardLeftFriction;
		FrontRightWheel.forwardFriction = forwardRightFriction;
		BackLeftWheel.forwardFriction = backLeftFriction;
		BackRightWheel.forwardFriction = backRightFriction;
		// Add extra sideways friction based on the car's turning velocity to avoid slipping
		float maxSStif = 10000f;
		float minSStif = 5000f;
			
		forwardLeftSideFriction.extremumValue = Mathf.Clamp(maxSStif- sqrVel, 1000, maxSStif);
		forwardLeftSideFriction.asymptoteValue = Mathf.Clamp(minSStif - (sqrVel / 200), 1000, minSStif);
			
		forwardRightSideFriction.extremumValue = Mathf.Clamp(maxSStif - sqrVel, 1000, maxSStif);
		forwardRightSideFriction.asymptoteValue = Mathf.Clamp(minSStif - (sqrVel / 200), 1000, minSStif);
			
		backLeftSideFriction.extremumValue = Mathf.Clamp(maxSStif - sqrVel, 1000, maxSStif);
		backLeftSideFriction.asymptoteValue = Mathf.Clamp(minSStif - (sqrVel / 200), 1000, minSStif);
			
		backRightSideFriction.extremumValue = Mathf.Clamp(maxSStif - sqrVel, 1000, maxSStif);
		backRightSideFriction.asymptoteValue = Mathf.Clamp(minSStif - (sqrVel / 200), 1000, minSStif);
		
		FrontLeftWheel.sidewaysFriction = forwardLeftSideFriction;
		FrontRightWheel.sidewaysFriction = forwardRightSideFriction;
		BackLeftWheel.sidewaysFriction = backLeftSideFriction;
		BackRightWheel.sidewaysFriction = backRightSideFriction;

	}
}
