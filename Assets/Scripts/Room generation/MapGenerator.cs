using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
public class MapGenerator : MonoBehaviour
{
	public static WHY Finished;
	//static public float slowering = 2.5f;
	static protected System.Random rng = new(0);
	
	static protected async Task<bool> AddRoom(GameObject firstDoorObject, GameObject roomPref, string doorType = null)
	{
		//await Task.Delay((int)(slowering * 500));
		//Debug.Log("PRE INIT");

		var firstDoor = firstDoorObject.GetComponent<RoomDoor>();
		Vector3 firstDoorPosition = firstDoorObject.transform.position;

		// Inits new room and selects random door 
		GameObject newRoom = Instantiate(roomPref, firstDoorPosition, new Quaternion());

		List<GameObject> nonBanned = newRoom.GetComponent<RoomBehaviour>().roomDoors.ToList();
		firstDoor.RoomBanDict.TryGetValue(roomPref, out var Banned);
		nonBanned.RemoveAll(x => (Banned??Enumerable.Empty<GameObject>().ToList()).Contains(x));

		if (nonBanned.Count == 0) {
			Destroy(newRoom);
			return true;
		}
		GameObject secondDoorObject = nonBanned.RandomElement(rng);

		if (secondDoorObject.GetComponent<RoomDoor>().DoorType == doorType && doorType != null){
			Destroy(newRoom);
			return true;
		}

		//Debug.DrawLine(firstDoorPosition, firstDoorPosition + Vector3.up, Color.red, slowering);
		//Debug.DrawLine(secondDoorObject.transform.position, secondDoorObject.transform.position + Vector3.up, Color.blue, slowering);
		//await Task.Delay((int)(slowering * 500));
		//Debug.Log("INIT");

		// Smashes doors together
		newRoom.transform.position -= secondDoorObject.transform.position - firstDoorPosition;

		//await Task.Delay((int)(slowering * 500));
		//Debug.Log("SMASH");

		// Applies calculated rotation angle to align selected doors
		Vector3 vec1 = firstDoor.VectorA;
		Vector3 vec2 = secondDoorObject.GetComponent<RoomDoor>().VectorB;
		Vector3 orient = Vector3.up;

		//Debug.DrawLine(firstDoorPosition, firstDoorPosition + vec1 + Vector3.up, Color.red, slowering);
		//Debug.DrawLine(firstDoorPosition, firstDoorPosition + vec2 + Vector3.up * 2, Color.blue, slowering);
		//Debug.DrawLine(firstDoorPosition, firstDoorPosition + orient + Vector3.up * 3, Color.yellow, slowering);

		float angle = Vector3.SignedAngle(vec1, vec2, orient * -1);
		newRoom.transform.RotateAround(firstDoorPosition, orient, angle);

		//await Task.Delay((int)(slowering * 500));
		//Debug.Log("ROTUNDA");

		var roomCollider = newRoom.GetComponents<BoxCollider>();
		bool intersects = checkColisions(roomCollider);

		if (intersects)
		{
			if (!firstDoor.RoomBanDict.TryGetValue(roomPref, out _))
				firstDoor.RoomBanDict[roomPref] = new();
			firstDoor.RoomBanDict[roomPref].Add(secondDoorObject);
			Destroy(newRoom);
		}
		else
			firstDoor.Open();

		newRoom.GetComponent<RoomBehaviour>().EnableBack();
		Destroy(secondDoorObject);
		await Task.Yield();
		return intersects;
	}
	static bool checkColisions(BoxCollider[] comps)
	{
		foreach (var comp in comps)
			comp.enabled = false;
		foreach (var comp in comps)
		{
			// TODO: Rewrite with Physics.BoxCast() for prod
			var colliders = Physics.OverlapBox(
				comp.transform.TransformPoint(comp.center),
				comp.size * 0.5f,
				comp.transform.rotation,
				LayerMask.GetMask("LVL"),
				QueryTriggerInteraction.Collide
			);
			//foreach (var col in colliders)
			//{
			//	Debug.LogWarning($"{col.bounds} intersects {comp.bounds}");
			//	ColliderDrawer.DrawCollider(col, Color.green, slowering);
			//}WHY
			//ColliderDrawer.DrawCollider(comp, Color.red, slowering);
			if (colliders.Length > 0)
				return true;
		}
		foreach (var comp in comps)
			comp.enabled = true;

		return false;
	}
}
public delegate void WHY();
public static class RandomElements
{
	public static T RandomElement<T>(this T[] array, System.Random rng) =>
		array[rng.Next(array.Length)];
	public static T RandomElement<T>(this List<T> array, System.Random rng) =>
		array[rng.Next(array.Count)];
}
public static class ColliderDrawer
{
    public static void DrawCollider(Collider collider, Color color, float duration = 0.1f)
    {
        if (collider == null) return;

        switch (collider)
        {
            case BoxCollider boxCollider:
                DrawBoxCollider(boxCollider, color, duration);
                break;
                
            case SphereCollider sphereCollider:
                DrawSphereCollider(sphereCollider, color, duration);
                break;
                
            case CapsuleCollider capsuleCollider:
                DrawCapsuleCollider(capsuleCollider, color, duration);
                break;
                
            case MeshCollider meshCollider:
                DrawMeshCollider(meshCollider, color, duration);
                break;
                
            // Add more collider types as needed
            default:
                Debug.LogWarning($"Collider type {collider.GetType().Name} not supported for drawing");
                break;
        }
    }

    private static void DrawBoxCollider(BoxCollider boxCollider, Color color, float duration)
    {
        Transform transform = boxCollider.transform;
        Vector3 center = boxCollider.center;
        Vector3 size = boxCollider.size;
        Vector3 halfExtents = size * 0.5f;
        
        Vector3[] corners = new Vector3[8]
        {
            center + new Vector3(-halfExtents.x, -halfExtents.y, -halfExtents.z),
            center + new Vector3(halfExtents.x, -halfExtents.y, -halfExtents.z),
            center + new Vector3(halfExtents.x, -halfExtents.y, halfExtents.z),
            center + new Vector3(-halfExtents.x, -halfExtents.y, halfExtents.z),
            center + new Vector3(-halfExtents.x, halfExtents.y, -halfExtents.z),
            center + new Vector3(halfExtents.x, halfExtents.y, -halfExtents.z),
            center + new Vector3(halfExtents.x, halfExtents.y, halfExtents.z),
            center + new Vector3(-halfExtents.x, halfExtents.y, halfExtents.z)
        };
        
        for (int i = 0; i < corners.Length; i++)
        {
            corners[i] = transform.TransformPoint(corners[i]);
        }
        
        // Bottom square
        Debug.DrawLine(corners[0], corners[1], color, duration);
        Debug.DrawLine(corners[1], corners[2], color, duration);
        Debug.DrawLine(corners[2], corners[3], color, duration);
        Debug.DrawLine(corners[3], corners[0], color, duration);
        
        // Top square
        Debug.DrawLine(corners[4], corners[5], color, duration);
        Debug.DrawLine(corners[5], corners[6], color, duration);
        Debug.DrawLine(corners[6], corners[7], color, duration);
        Debug.DrawLine(corners[7], corners[4], color, duration);
        
        // Vertical edges
        Debug.DrawLine(corners[0], corners[4], color, duration);
        Debug.DrawLine(corners[1], corners[5], color, duration);
        Debug.DrawLine(corners[2], corners[6], color, duration);
        Debug.DrawLine(corners[3], corners[7], color, duration);
    }

    private static void DrawSphereCollider(SphereCollider sphereCollider, Color color, float duration)
    {
        Transform transform = sphereCollider.transform;
        Vector3 center = transform.TransformPoint(sphereCollider.center);
        float radius = sphereCollider.radius * GetMaxScale(sphereCollider.transform);
        
        DrawWireSphere(center, radius, color, duration);
    }

    private static void DrawCapsuleCollider(CapsuleCollider capsuleCollider, Color color, float duration)
    {
        Transform transform = capsuleCollider.transform;
        Vector3 center = capsuleCollider.center;
        float radius = capsuleCollider.radius;
        float height = capsuleCollider.height;
        
        Vector3 scale = transform.lossyScale;
        float scaledRadius = radius * Mathf.Max(scale.x, scale.z);
        float scaledHeight = height * scale.y;
        
        Vector3 direction = GetDirectionVector(capsuleCollider.direction);
        Vector3 topSphereCenter = center + direction * (scaledHeight * 0.5f - scaledRadius);
        Vector3 bottomSphereCenter = center - direction * (scaledHeight * 0.5f - scaledRadius);
        
        topSphereCenter = transform.TransformPoint(topSphereCenter);
        bottomSphereCenter = transform.TransformPoint(bottomSphereCenter);
        
        // Draw spheres
        DrawWireSphere(topSphereCenter, scaledRadius, color, duration);
        DrawWireSphere(bottomSphereCenter, scaledRadius, color, duration);
        
        // Draw connecting lines
        Vector3 right = transform.right * scaledRadius;
        Vector3 forward = transform.forward * scaledRadius;
        Vector3 up = transform.up * scaledRadius;
        
        Debug.DrawLine(topSphereCenter + right, bottomSphereCenter + right, color, duration);
        Debug.DrawLine(topSphereCenter - right, bottomSphereCenter - right, color, duration);
        Debug.DrawLine(topSphereCenter + forward, bottomSphereCenter + forward, color, duration);
        Debug.DrawLine(topSphereCenter - forward, bottomSphereCenter - forward, color, duration);
    }

    private static void DrawMeshCollider(MeshCollider meshCollider, Color color, float duration)
    {
        if (meshCollider.sharedMesh == null) return;
        
        Mesh mesh = meshCollider.sharedMesh;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        
        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 v1 = meshCollider.transform.TransformPoint(vertices[triangles[i]]);
            Vector3 v2 = meshCollider.transform.TransformPoint(vertices[triangles[i+1]]);
            Vector3 v3 = meshCollider.transform.TransformPoint(vertices[triangles[i+2]]);
            
            Debug.DrawLine(v1, v2, color, duration);
            Debug.DrawLine(v2, v3, color, duration);
            Debug.DrawLine(v3, v1, color, duration);
        }
    }

    private static void DrawWireSphere(Vector3 center, float radius, Color color, float duration, int segments = 16)
    {
        // Draw 3 circles to form a wire sphere
        DrawCircle(center, Vector3.up, Vector3.right, radius, color, duration, segments);
        DrawCircle(center, Vector3.right, Vector3.forward, radius, color, duration, segments);
        DrawCircle(center, Vector3.forward, Vector3.up, radius, color, duration, segments);
    }

    private static void DrawCircle(Vector3 center, Vector3 normal, Vector3 axis, float radius, Color color, float duration, int segments)
    {
        Vector3 perpendicular = Vector3.Cross(normal, axis).normalized * radius;
        Vector3 lastPoint = center + perpendicular;
        
        for (int i = 1; i <= segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2;
            Vector3 nextPoint = center + (perpendicular * Mathf.Cos(angle) + (Vector3.Cross(normal, perpendicular) * Mathf.Sin(angle)));
            Debug.DrawLine(lastPoint, nextPoint, color, duration);
            lastPoint = nextPoint;
        }
    }

    private static Vector3 GetDirectionVector(int direction)
    {
        switch (direction)
        {
            case 0: return Vector3.right;   // X-axis
            case 1: return Vector3.up;       // Y-axis
            case 2: return Vector3.forward;  // Z-axis
            default: return Vector3.up;
        }
    }

    private static float GetMaxScale(Transform t)
    {
        Vector3 scale = t.lossyScale;
        return Mathf.Max(scale.x, Mathf.Max(scale.y, scale.z));
    }
}
