using UnityEngine;

public class CollisionNoise : MonoBehaviour
{
	public float cooldownDuration = 0.25f; // Time between allowed collision sounds
    float lastCollisionTime = 0f;

    void OnCollisionEnter(Collision collision)
    {
        float currentTime = Time.time;

        if (currentTime - lastCollisionTime < cooldownDuration)
        	return;
		Debug.Log($"collision {collision.relativeVelocity.sqrMagnitude}");
		lastCollisionTime = currentTime;
    }
}