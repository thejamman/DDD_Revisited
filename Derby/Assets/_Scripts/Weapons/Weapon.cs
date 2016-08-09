using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour {
	//just a test to see what it would feel like
	public Transform m_ProjectileSpawnPoint;
	public GameObject m_Projectile;
	public ParticleEmitter m_MuzzelFlash;
	public Light m_MuzzelLight;
	public Transform m_Stand;
	public GameObject m_HitParticlePrefab;
	public AudioClip m_FireSound;
	
	private GameObject m_ProjectileCreated;
	private GameObject m_HitParticleCreated;
	
	private float m_FireRate = 0.08f;
	private float m_LastFired;
    private InputController m_InputController;

    void Start() 
    {
        m_InputController = transform.root.GetComponent<InputController>();
    }
	void FixedUpdate() 
	{
		RaycastHit mouseHit = new RaycastHit();
		
		if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out mouseHit, Mathf.Infinity))
		{
			m_Stand.LookAt(mouseHit.point);
			
		}

        if (Input.GetMouseButton(0))
		{
			if(Time.time - m_LastFired > m_FireRate)
			{
				//m_ProjectileCreated = Instantiate(m_Projectile, m_ProjectileSpawnPoint.position, m_ProjectileSpawnPoint.rotation) as GameObject;
				//m_ProjectileCreated.AddComponent<Rigidbody>().AddForce (m_ProjectileSpawnPoint.TransformPoint(Vector3.forward * 100000));
				StartCoroutine(WaitForDistance(mouseHit.point));
				m_MuzzelFlash.Emit();
				m_MuzzelLight.enabled = true;
				GetComponent<AudioSource>().PlayOneShot(m_FireSound);
				m_LastFired = Time.time;
			}
		}
		else
		{
			m_MuzzelLight.enabled = false;
		}
		
    }
	
	IEnumerator WaitForDistance(Vector3 mousePoint)
	{
		float distance = Vector3.Distance(transform.position, mousePoint);
		yield return new WaitForSeconds(distance/500);
		
		if (m_HitParticlePrefab != null)
		{
			m_HitParticleCreated = Instantiate(m_HitParticlePrefab, mousePoint, Quaternion.identity) as GameObject;
			m_HitParticleCreated.GetComponent<ExplosionControl>().m_DistanceFromOrigin = distance;
		}
	}
}
