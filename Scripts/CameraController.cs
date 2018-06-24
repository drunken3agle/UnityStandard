using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

/*
This script can be used for controlling a camera moving over the game world
It already has support for the interactables class available in this repository
If you do not use the interactables class the HandleClick method needs to be updated.
*/
public class CameraController : MonoBehaviour {

    public Text DebugText;

    public GameObject player;
    public GameObject playerGoal;

    public float moveSpeed = 0.013f;      // Camera movement speed multiplier
    public float minZoomDistance = 2.5f;  // Closest zoom distance
    public float maxZoomDistance = 20.0f; // Farthest zoom distance

    private readonly float minCameraAngle = 30.0f; // Angle at closest zoom
    private readonly float maxCameraAngle = 60.0f; // Angle at farthest zoom

#if (UNITY_ANDROID)
    private Touch touchStart;
#elif (UNITY_STANDALONE)
    private Vector3 leftClickStart; 
    private Vector3 leftClickLast;

    private Vector3 rightClickStart;
    private Vector3 rightClickLast;

    public float mouseZoomSpeed = 3.0f;
#endif
    private readonly float clickThreshold = 10.0f;
    private readonly float clickDistanceThresold = 5.0f;

    private Plane floorPlane = new Plane(Vector3.up, Vector3.zero);

    // Use this for initialization
    void Start() {
    }

    // Update is called once per frame
    void FixedUpdate() {
		
#if (UNITY_ANDROID)
	
        if (Input.touchCount == 1) { // Camera/Player movement
            Touch t = Input.touches[0];
            switch (t.phase) {
                case TouchPhase.Began:
                    touchStart = t;
                    break;
                case TouchPhase.Moved: // Translate camera master
                    transform.Translate(-t.deltaPosition.x * moveSpeed, 0.0f, -t.deltaPosition.y * moveSpeed);
                    break;
                case TouchPhase.Ended: // Handle clicking
                    if (Vector2.Distance(touchStart.position, t.position) < clickThreshold) {
                        HandleClick(t.position);
                        break;
                    }
                    break;
                default:
                    break;
            }
        } else if (Input.touchCount == 2) { // Camera rotation/zoom
            Touch finger1 = Input.touches[0];
            Touch finger2 = Input.touches[1];

            float turnAngle = Angle(finger1.position, finger2.position);
            float prevTurn = Angle(finger1.position - finger1.deltaPosition,
                                   finger2.position - finger2.deltaPosition);

            float turnAngleDelta = Mathf.DeltaAngle(prevTurn, turnAngle);

            if (Mathf.Abs(turnAngleDelta) < 1.0f) { // Zoom (local Z movement on camera)
                float currentDistance = Vector2.Distance(finger1.position, finger2.position);
                float prevDistance = Vector2.Distance(finger1.position - finger1.deltaPosition,
                                                      finger2.position - finger2.deltaPosition);
                float pinchDistanceDelta = (currentDistance - prevDistance) * moveSpeed;

                Zoom(pinchDistanceDelta);
            } else { // Rotation
                Vector3 rotationDeg = Vector3.zero;
                rotationDeg.y = turnAngleDelta;
                transform.Rotate(rotationDeg, Space.World);
            }
        }
				
#elif (UNITY_STANDALONE)
	
        if (Input.GetMouseButtonDown(0)) {
            leftClickStart = Input.mousePosition;
            leftClickLast = leftClickStart;
        }

        if (Input.GetMouseButton(0)) {
            Vector3 mouseMovement = leftClickLast - Input.mousePosition;
            transform.Translate(mouseMovement.x * moveSpeed, 0.0f, mouseMovement.y * moveSpeed);
            leftClickLast = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0)) {
            Vector3 mouseMovement = leftClickStart - Input.mousePosition;
            if (mouseMovement.sqrMagnitude < clickThreshold) {
                HandleClick(Input.mousePosition);
            }
        }

        if (Input.GetMouseButtonDown(1)) {
            rightClickStart = Input.mousePosition;
            rightClickLast = rightClickStart;
        }

        if (Input.GetMouseButton(1)) {
            float mouseMovementX = rightClickLast.x - Input.mousePosition.x;
            if(mouseMovementX > 0.01f || mouseMovementX < -0.01f) {
                float turnAngle = mouseMovementX * mouseMovementX * (mouseMovementX / Mathf.Abs(mouseMovementX)) * moveSpeed;

                Vector3 rotationDeg = Vector3.zero;
                rotationDeg.y = turnAngle;
                transform.Rotate(rotationDeg, Space.World);

                rightClickLast = Input.mousePosition;
            }
        }

        Zoom(Input.GetAxis("Mouse ScrollWheel") * mouseZoomSpeed);
#endif
    }

    private void HandleClick(Vector3 ScreenPosition) {
        Ray clickRay = Camera.main.ScreenPointToRay(ScreenPosition);

        RaycastHit hit; // Perform raycast to check if interactable object was clicked
        Physics.Raycast(clickRay, out hit);
        if (hit.transform.tag == "Interactable") { // && hit.distance < clickDistanceThresold) {
            hit.transform.gameObject.GetComponent<Interactable>().Interaction();
            return;
        }

        float distance;
        floorPlane.Raycast(clickRay, out distance);
        Vector3 clickPosition = clickRay.GetPoint(distance);

        NavMeshHit closestPoint;
        if (NavMesh.SamplePosition(clickPosition, out closestPoint, 5.0f, NavMesh.AllAreas)) {
            playerGoal.transform.position = closestPoint.position;
        }
    }

    private void Zoom(float zoomAmount) {
        float distanceToMaster = Vector3.Distance(transform.position, Camera.main.transform.position);

        // Set zoom limits
        if (!(distanceToMaster < minZoomDistance && zoomAmount > 0) && !(distanceToMaster > maxZoomDistance && zoomAmount < 0)) {
            Camera.main.transform.position = Vector3.MoveTowards(Camera.main.transform.position, transform.position, zoomAmount);

            Camera.main.transform.localRotation = Quaternion.RotateTowards(Camera.main.transform.localRotation,
                Quaternion.Euler(CameraAngle(distanceToMaster), 0f, 0f), 10.0f);
        }
    }

    static private float Angle(Vector2 pos1, Vector2 pos2) {
        Vector2 from = pos2 - pos1;
        Vector2 to = new Vector2(1, 0);

        float result = Vector2.Angle(from, to);
        Vector3 cross = Vector3.Cross(from, to);

        if (cross.z > 0) {
            result = 360f - result;
        }

        return result;
    }

    // Calculates camera angle for current zoom
    private float CameraAngle(float distanceToMaster) {
        float m = (maxCameraAngle - minCameraAngle) / (maxZoomDistance - minZoomDistance);
        float t = maxCameraAngle - (m * maxZoomDistance);

        return m * distanceToMaster + t;
    }
}
