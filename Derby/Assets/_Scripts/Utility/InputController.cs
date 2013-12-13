using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InputController : MonoBehaviour{
	
	#region public members
	public bool showInputInfo;
	#endregion
	
	#region private members
	private float m_leftStickHt;
	private float m_leftStickVt;
	private float m_rightStickHt;
	private float m_rightStickVt;
	private float m_leftTrigger;
	private float m_rightTrigger;
	private float m_dPadHt;
	private float m_dPadVt;
	private Dictionary<string, bool> m_buttonMap = new Dictionary<string, bool>();
	private Dictionary<string, bool> m_buttonPrev = new Dictionary<string, bool>();
	private string[] joystickNames;
	
	//private PhotonView myPhotonView;
	#endregion
	
	#region Public Accessors
	public float Left_X
	{
		get { return m_leftStickHt; }
		set { m_leftStickHt = value; }
	}
	public float Left_Y
	{
		get { return m_leftStickVt; }
		set { m_leftStickVt = value; }
	}
	public float Right_X
	{
		get { return m_rightStickHt; }
		set { m_rightStickHt = value; }
	}
	public float Right_Y
	{
		get { return m_rightStickVt; }
		set { m_rightStickVt = value; }
	}
	public float Left_Trigger
	{
		get { return m_leftTrigger; }	
	}
	public float Right_Trigger
	{
		get { return m_rightTrigger; }
	}
	public float Dpad_X
	{
		get { return m_dPadHt; }	
	}
	public float Dpad_Y
	{
		get { return m_dPadVt; }	
	}
	public Dictionary<string, bool> ButtonMap
	{
		get { return m_buttonMap; }	
	}
	public Dictionary<string, bool> PrevButtonState
	{
		get { return m_buttonPrev; }	
	}
	public string[] ConnectedJoysticks
	{
		get { return joystickNames; }	
	}
	
	#endregion
	
	void Awake()
	{
		//myPhotonView = gameObject.GetComponent<PhotonView>();	
	}
	
	void Start()
	{
		m_buttonMap.Add("A", false);
		m_buttonMap.Add("B", false);
		m_buttonMap.Add("X", false);
		m_buttonMap.Add("Y", false);
		m_buttonMap.Add("LB", false);
		m_buttonMap.Add("RB", false);
		m_buttonMap.Add("START", false);
		m_buttonMap.Add("BACK", false);
		
		foreach (string key in m_buttonMap.Keys)
		{
			m_buttonPrev.Add(key, false);
		}
		
		//if (myPhotonView.isMine)
		//{
			UpdateConnectedJoysticks();
		//}
	}
	
	// Update is called once per frame
	void Update () {
	
		//if (myPhotonView.isMine)
		//{
			UpdatePreviousState();
			
			m_leftStickHt = Input.GetAxis("LeftHorizontal");
			m_leftStickVt = Input.GetAxis("LeftVertical");
			m_rightStickHt = Input.GetAxis("RightHorizontal");
			m_rightStickVt = Input.GetAxis("RightVertical");
			m_leftTrigger = Input.GetAxis("LeftTrigger");
			m_rightTrigger = Input.GetAxis("RightTrigger");
			m_dPadHt = Input.GetAxis("DPadHorizontal");
			m_dPadVt = Input.GetAxis("DPadVertical");
			m_buttonMap["LB"] = Input.GetButton("LeftBumper");
			m_buttonMap["RB"] = Input.GetButton("RightBumper");
			m_buttonMap["A"] = Input.GetButton("AButton");
			m_buttonMap["B"] = Input.GetButton("BButton");
			m_buttonMap["X"] = Input.GetButton("XButton");
			m_buttonMap["Y"] = Input.GetButton("YButton");
			m_buttonMap["START"] = Input.GetButton("StartButton");
			m_buttonMap["BACK"] = Input.GetButton("BackButton");
		//}
	}
	
	private void UpdatePreviousState()
	{
		foreach (string key in m_buttonMap.Keys)
		{
			m_buttonPrev[key] = m_buttonMap[key];	
		}
	}
	
	public void UpdateConnectedJoysticks()
	{
		joystickNames = Input.GetJoystickNames();
	}
	
	void OnGUI()
	{
		if (showInputInfo)
		{
			Rect htRect = new Rect(25, 50, 250, 25);
			Rect vtRect = new Rect(25, 75, 250, 25);
			Rect jRect = new Rect(25, 25, 350, 25);
			
			if (joystickNames != null)
			{
				GUI.Box(jRect, "Connected Joystick: " + joystickNames[0]);
			}
			
			GUI.Box(htRect, "Left Stick Horizontal = " + m_leftStickHt.ToString());
			GUI.Box(vtRect, "Left Stick Vertical = " + m_leftStickVt.ToString());
			
			htRect = new Rect(25, 100, 250, 25);
			vtRect = new Rect(25, 125, 250, 25);
			
			GUI.Box(htRect, "Right Stick Horizontal = " + m_rightStickHt.ToString());
			GUI.Box(vtRect, "Right Stick Vertical = " + m_rightStickVt.ToString());
			
			htRect = new Rect(300, 50, 250, 25);
			vtRect = new Rect(300, 75, 250, 25);
			
			GUI.Box(htRect, "Right Trigger = " + m_rightTrigger.ToString());
			GUI.Box(vtRect, "Left Trigger = " + m_leftTrigger.ToString());
			
			htRect = new Rect(300, 100, 250, 25);
			vtRect = new Rect(300, 125, 250, 25);
			
			GUI.Box(htRect, "Right Bumper = " + m_buttonMap["RB"].ToString());
			GUI.Box(vtRect, "Left Bumper = " + m_buttonMap["LB"].ToString());
			
			htRect = new Rect(575, 50, 250, 25);
			vtRect = new Rect(575, 75, 250, 25);
			
			GUI.Box(htRect, "A Button = " + m_buttonMap["A"].ToString());
			GUI.Box(vtRect, "B Button = " + m_buttonMap["B"].ToString());
			
			htRect = new Rect(575, 100, 250, 25);
			vtRect = new Rect(575, 125, 250, 25);
			
			GUI.Box(htRect, "X Button = " + m_buttonMap["X"].ToString());
			GUI.Box(vtRect, "Y Button = " + m_buttonMap["Y"].ToString());
			
			htRect = new Rect(850, 50, 250, 25);
			vtRect = new Rect(850, 75, 250, 25);
			
			GUI.Box(htRect, "DPad Horizontal = " + m_dPadHt.ToString());
			GUI.Box(vtRect, "DPad Vertical = " + m_dPadVt.ToString());
			
			htRect = new Rect(850, 100, 250, 25);
			vtRect = new Rect(850, 125, 250, 25);
			
			GUI.Box(htRect, "Start Button = " + m_buttonMap["START"].ToString());
			GUI.Box(vtRect, "Back Button = " + m_buttonMap["BACK"].ToString());
		}
	}
}
