using UnityEngine;
using System.Collections;

public class ExplosionControl : MonoBehaviour {

	public ParticleEmitter[] m_ParticleEmitters;
	public Light m_ExplosionLight;
	public AudioClip m_ExplosionSound;
	public float m_DistanceFromOrigin;
	
	private float m_MinLightRange = 0;
	private float m_MaxLightRange = 50;
	private float m_CurrentRange = 0;
	
	private float m_SoundMetersPerSeconds = 343;
	
	void Start()
	{
		StartCoroutine(DoLightShow());
	}
	
	IEnumerator DoLightShow()
	{
		Vector3 explosionPos = transform.position;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, 10);
		
        foreach (Collider hit in colliders)
		{
            if (hit.GetComponent<Rigidbody>())
			{
                hit.GetComponent<Rigidbody>().AddExplosionForce(100, explosionPos, 10, 3.0F);
			}
        }
		
		StartCoroutine(DoSoundAtDistance());
		
		float incrementer = m_MinLightRange;
		while(m_ExplosionLight.range < m_MaxLightRange)
		{
			m_ExplosionLight.range = Mathf.Lerp(m_ExplosionLight.range, m_MaxLightRange, incrementer);
			incrementer += 0.01f;
			yield return new WaitForSeconds(0.01f);
		}
		incrementer = m_MaxLightRange;
		while(m_ExplosionLight.range > m_MinLightRange)
		{
			m_ExplosionLight.range = Mathf.Lerp(m_ExplosionLight.range, m_MinLightRange, incrementer);
			incrementer -= 0.0001f;
			yield return new WaitForSeconds(0.01f);
		}
		yield return new WaitForSeconds(5f);
		Destroy(gameObject);
		
	}

	IEnumerator DoSoundAtDistance()
	{
		//Debug.Log (m_DistanceFromOrigin);
		yield return new WaitForSeconds(m_DistanceFromOrigin/m_SoundMetersPerSeconds);
		GetComponent<AudioSource>().PlayOneShot(m_ExplosionSound);
	}
}
