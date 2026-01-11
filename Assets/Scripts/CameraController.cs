using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Positions to lerp between
    public Vector3 labyrinthPos = new Vector3(5, 12, -5);
    public Vector3 labyrinthRot = new Vector3(45, 0, 0);

    public Vector3 summonZonePos = new Vector3(5, 8, -10);
    public Vector3 summonZoneRot = new Vector3(30, 0, 0);

    public float speed = 5f;
    private bool lookingAtLabyrinth = true;

    void Update()
    {
        // Toggle view with Space bar or Mouse Wheel
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            lookingAtLabyrinth = !lookingAtLabyrinth;
        }

        Vector3 targetPos = lookingAtLabyrinth ? labyrinthPos : summonZonePos;
        Quaternion targetRot = Quaternion.Euler(lookingAtLabyrinth ? labyrinthRot : summonZoneRot);

        // Smoothly move and rotate
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * speed);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * speed);
    }
}