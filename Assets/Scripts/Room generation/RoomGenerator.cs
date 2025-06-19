using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
public class RoomGenerator : MonoBehaviour
{
	int ind = 0;
	public float speed = 5;
	private System.Random rng = new (7);
	[SerializeField] List<GameObject> Rooms = new();
	GameObject getRandomRoom() => Rooms[rng.Next(Rooms.Count)];
	public static List<GameObject> Doors = new();
	GameObject getRandomDoor()
	{
		Doors.RemoveAll(door => door == null);
		return Doors[rng.Next(Doors.Count)];
	}
	List<BoxCollider> RoomColliders = new();
	bool checkColisions(BoxCollider comp)
	{
		Physics.SyncTransforms();
		foreach (var col in RoomColliders)
			if (comp.bounds.Intersects(col.bounds))
			{
				// Debug.LogWarning($"IND:{ind} {col.bounds} intersects {comp.bounds}");
				DrawBoxCollider(col, Color.green, speed);
				DrawBoxCollider(comp, Color.red, speed);
				return true;
			}
		return false;
	}
	
	async void Start()
	{
		RoomColliders.Add(
			Instantiate(Rooms[0], new Vector3(), new Quaternion()).GetComponent<BoxCollider>()
		);
		await Task.Delay(10);
		while (ind < 101)
		{
            
			// await Task.Delay((int)(speed * 500));	
			// Debug.Log("PRE INIT");
			bool res;

			// Selects random doors and inits new room
			GameObject firstdoorObject = getRandomDoor();
			Vector3 firstDoorPosition = firstdoorObject.transform.position;

			GameObject newRoom = Instantiate(getRandomRoom(), firstDoorPosition, new Quaternion());
			GameObject secondDoorObject = newRoom.GetComponent<RoomBehaviour>().GetRadomDoor(rng);

			Debug.DrawLine(firstDoorPosition, firstDoorPosition + Vector3.up, Color.red, speed);
			Debug.DrawLine(secondDoorObject.transform.position, secondDoorObject.transform.position + Vector3.up, Color.blue, speed);
            
			// await Task.Delay((int)(speed * 500));
			// Debug.Log("INIT");

			//smashes doors together
			newRoom.transform.position -= secondDoorObject.transform.position - firstDoorPosition;

			// await Task.Delay((int)(speed * 500));
			// Debug.Log("SMASH");

			// Applies calculated rotation angle to align selected doors

			Vector3 vec1 = SnapToCardinal(firstDoorPosition - firstdoorObject.transform.parent.position);
			Vector3 vec2 = SnapToCardinal(secondDoorObject.transform.localPosition)*-1;
			Vector3 orient = Vector3.up;

			Debug.DrawLine(firstDoorPosition, firstDoorPosition + vec1 + Vector3.up, Color.red, speed);
			Debug.DrawLine(firstDoorPosition, firstDoorPosition + vec2 + Vector3.up*2, Color.blue, speed);
			Debug.DrawLine(firstDoorPosition, firstDoorPosition + orient + Vector3.up*3, Color.yellow, speed);

			float angle = Vector3.SignedAngle(vec1, vec2, orient*-1);
			newRoom.transform.RotateAround(firstDoorPosition, orient, angle);

			//await Task.Delay((int)(speed * 500));
			//Debug.Log("ROTUNDA");

			var roomCollider = newRoom.GetComponent<BoxCollider>();
			bool intersects = checkColisions(roomCollider);
			if (intersects)
			{
				firstdoorObject.GetComponent<RoomDoor>().Close();
				Destroy(newRoom);
			}
			else
			{
				firstdoorObject.GetComponent<RoomDoor>().Open();
				RoomColliders.Add(roomCollider);
			}
			Destroy(secondDoorObject);
			res = !intersects;
		
			Debug.Log($"IND:{ind} sucsess: {res}");
			if (res)
				ind++;

			await Task.Delay((int)(speed * 10));
		}

		foreach (GameObject door in Doors) 
			door.GetComponent<RoomDoor>().Close();
	}
	Vector3 SnapToCardinal(Vector3 input)
	{
		Vector3 rounded = new(
			Mathf.Round(input.x),
			Mathf.Round(input.y),
			Mathf.Round(input.z)
		);

		// Find the axis with the largest absolute value
		float max = Mathf.Max(
			Mathf.Abs(rounded.x),
			Mathf.Abs(rounded.y),
			Mathf.Abs(rounded.z)
		);

		// Zero out non-dominant axes
		if (Mathf.Abs(rounded.x) != max) rounded.x = 0;
		if (Mathf.Abs(rounded.y) != max) rounded.y = 0;
		if (Mathf.Abs(rounded.z) != max) rounded.z = 0;

		return rounded.normalized;
	}
	public static void DrawBoxCollider(BoxCollider boxCollider, Color color, float duration = 0.1f)
    {
        if (boxCollider == null) return;

        // Get the transform of the box collider
        Transform transform = boxCollider.transform;
        
        // Get collider center and size
        Vector3 center = boxCollider.center;
        Vector3 size = boxCollider.size;
        
        // Calculate half extents
        Vector3 halfExtents = size * 0.5f;
        
        // Calculate local positions of the box corners
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
        
        // Convert local positions to world positions
        for (int i = 0; i < corners.Length; i++)
        {
            corners[i] = transform.TransformPoint(corners[i]);
        }
        
        // Draw the bottom square
        Debug.DrawLine(corners[0], corners[1], color, duration);
        Debug.DrawLine(corners[1], corners[2], color, duration);
        Debug.DrawLine(corners[2], corners[3], color, duration);
        Debug.DrawLine(corners[3], corners[0], color, duration);
        
        // Draw the top square
        Debug.DrawLine(corners[4], corners[5], color, duration);
        Debug.DrawLine(corners[5], corners[6], color, duration);
        Debug.DrawLine(corners[6], corners[7], color, duration);
        Debug.DrawLine(corners[7], corners[4], color, duration);
        
        // Draw the vertical edges
        Debug.DrawLine(corners[0], corners[4], color, duration);
        Debug.DrawLine(corners[1], corners[5], color, duration);
        Debug.DrawLine(corners[2], corners[6], color, duration);
        Debug.DrawLine(corners[3], corners[7], color, duration);
    }
}
