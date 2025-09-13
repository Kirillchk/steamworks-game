using UnityEngine;

public class CollisionNoise : MonoBehaviour
{
	AudioSource source;
	System.Random r = new(0);
	void Start()
	{
		source = GetComponent<AudioSource>();
	}
	void OnCollisionEnter(Collision collision)
    {
		Debug.Log($"collision {collision.relativeVelocity.sqrMagnitude}");
		source.pitch = r.Next(-3, 3);
		if (!source.isPlaying)
			source.Play();
    }
}