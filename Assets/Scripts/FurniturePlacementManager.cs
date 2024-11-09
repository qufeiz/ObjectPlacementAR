using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.XR.CoreUtils; 
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class FurniturePlacementManager : MonoBehaviour
{
    public GameObject SpawnableFurniture;
    public XROrigin xrOrigin;
    public ARRaycastManager raycastManager;
    public ARPlaneManager planeManager;
    public GameObject PointerObj;
    public float animationDuration = 0.5f; // Duration for the scale animation

    private List<ARRaycastHit> raycastHits = new List<ARRaycastHit>();
    private GameObject selectedObject; // Currently selected object

    // Fields for object movement
    private float speedModifier = 0.0005f;
    private Vector3 translationVector;
    public float rotationSpeed = 450f; // Speed of rotation in degrees per second
    private bool isRotating = false;

    private void Start()
    {
        if (PointerObj != null)
        {
            PointerObj.SetActive(false);
        }
    }

    private void Update()
    {
        // Continuously update the position of the placement indicator
        UpdatePlacementIndicator();


        // Handle object selection and movement with touch
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began && !EventSystem.current.IsPointerOverGameObject())
            {
                HandleObjectSelection(touch.position);
            }
            else if (touch.phase == TouchPhase.Moved && selectedObject != null && !EventSystem.current.IsPointerOverGameObject())
            {
                MoveSelectedObject(touch.deltaPosition);
            }
        }
        // Handle object selection and movement with mouse (for editor testing)
        else if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            HandleObjectSelection(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0) && selectedObject != null && !EventSystem.current.IsPointerOverGameObject())
        {
            Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            MoveSelectedObject(mouseDelta * 100); // Scale up for smoother movement
        }

        // Rotate the selected object if the rotation button is held down
        //Debug.Log("Rotating? "+isRotating);
        if (isRotating && selectedObject != null)
        {
            //Debug.Log("Rotating!!!");
            selectedObject.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }
    }

    private void UpdatePlacementIndicator()
    {
        // Perform a raycast from the center of the screen
        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        bool collision = raycastManager.Raycast(screenCenter, raycastHits, TrackableType.PlaneWithinPolygon);

        if (collision)
        {
            // Position the placement indicator at the raycast hit point
            PointerObj.transform.position = raycastHits[0].pose.position;
            PointerObj.transform.rotation = raycastHits[0].pose.rotation;

            // Make the placement indicator visible if it isn't already
            if (!PointerObj.activeInHierarchy)
            {
                PointerObj.SetActive(true);
            }
        }
        else
        {
            // Hide the placement indicator if no plane is detected
            if (PointerObj.activeInHierarchy)
            {
                PointerObj.SetActive(false);
            }
        }
    }


    public void PlaceObjectAtIndicator()
    {
        // Only place the object if the placement indicator is active
        //if (PointerObj.activeInHierarchy && !isButtonPressed())
        if(PointerObj.activeInHierarchy)
        {
            // Instantiate the furniture at the indicator's position and rotation
            GameObject _object = Instantiate(SpawnableFurniture);
            _object.transform.position = PointerObj.transform.position;
            _object.transform.rotation = PointerObj.transform.rotation;

            // Start scale animation from zero to full size
            _object.transform.localScale = Vector3.zero; // Start at zero scale
            StartCoroutine(LerpObjectScale(Vector3.zero, Vector3.one, animationDuration, _object));

            // Optionally, enable all detected planes (if you want them visible)
            foreach (var plane in planeManager.trackables)
            {
                plane.gameObject.SetActive(true);
            }

            // Ensure plane manager is enabled if needed
            planeManager.enabled = true;
        }
    }


    // Coroutine to smoothly scale the object from start to end scale
    private IEnumerator LerpObjectScale(Vector3 startScale, Vector3 endScale, float time, GameObject lerpObject)
    {
        float elapsedTime = 0.0f;

        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            float lerpFactor = elapsedTime / time;

            // Smoothly transition the scale from start to end
            lerpObject.transform.localScale = Vector3.Lerp(startScale, endScale, lerpFactor);
            yield return null;
        }

        // Ensure the final scale is set in case of timing issues
        lerpObject.transform.localScale = endScale;
    }

    // This function will remove the selected object from the scene
    public void RemoveSelectedObject()
    {
        if (selectedObject != null)
        {
            Destroy(selectedObject); // Destroys the selected object
            selectedObject = null; // Clear the reference
            Debug.Log("Selected object removed.");
        }
        else
        {
            Debug.Log("No object selected to remove.");
        }
    }


    private void HandleObjectSelection(Vector2 touchPosition)
    {


        Ray ray = Camera.main.ScreenPointToRay(touchPosition);
        RaycastHit hit;

        // Check if the ray hits an object with the "Selectable" tag
        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log("Hit!");
            if (hit.collider.CompareTag("Selectable"))
            {
                Debug.Log("Hit a furniture!!!!!!!");
                // Deselect previous object if any
                if (selectedObject != null)
                {
                    DeselectObject();
                }

                // Select new object
                selectedObject = hit.collider.gameObject;
                Debug.Log("Selected Object: " + selectedObject.name); // Check what object is selected
                HighlightObject(selectedObject);
            }
            else
            {
                DeselectObject();
            }
        }
        else
        {
            DeselectObject();
        }
    }

    //private void MoveSelectedObject(Touch touch)
    //{
    //    // Use camera's forward and right directions to move the object based on touch delta
    //    translationVector = new Vector3(Camera.main.transform.forward.x, 0f, Camera.main.transform.forward.z);
    //    selectedObject.transform.Translate(translationVector * touch.deltaPosition.y * speedModifier, Space.World);

    //    translationVector = new Vector3(Camera.main.transform.right.x, 0f, Camera.main.transform.right.z);
    //    selectedObject.transform.Translate(translationVector * touch.deltaPosition.x * speedModifier, Space.World);
    //}

    private void MoveSelectedObject(Vector2 deltaPosition)
    {
        // Use camera's forward and right directions to move the object based on input delta
        translationVector = new Vector3(Camera.main.transform.forward.x, 0f, Camera.main.transform.forward.z);
        selectedObject.transform.Translate(translationVector * deltaPosition.y * speedModifier, Space.World);

        translationVector = new Vector3(Camera.main.transform.right.x, 0f, Camera.main.transform.right.z);
        selectedObject.transform.Translate(translationVector * deltaPosition.x * speedModifier, Space.World);
    }

    private void HighlightObject(GameObject obj)
    {
        Debug.Log("Listing all child components of: " + obj.name);

        foreach (Transform child in obj.GetComponentsInChildren<Transform>(true))
        {
            Debug.Log("Child Object: " + child.name);
            Component[] components = child.GetComponents<Component>();
            foreach (var component in components)
            {
                Debug.Log(" - Component: " + component.GetType().FullName);
            }
        }

        // Try finding and enabling Outline components
        cakeslice.Outline[] outlines = obj.GetComponentsInChildren<cakeslice.Outline>(true);
        Debug.Log("Number of Outline components found on " + obj.name + ": " + outlines.Length);

        if (outlines.Length > 0)
        {
            foreach (var outline in outlines)
            {
                Debug.Log("Enabling Outline on: " + outline.gameObject.name);
                outline.enabled = true;
            }
        }
        else
        {
            Debug.LogWarning("No Outline components found. Please check if the Outline script is properly assigned.");
        }
    }


    private void DeselectObject()
    {
        if (selectedObject != null)
        {
            // Loop through all Outline components in the previously selected object
            cakeslice.Outline[] outlines = selectedObject.GetComponentsInChildren<cakeslice.Outline>();
            foreach (var outline in outlines)
            {
                outline.enabled = false; // Disable outline on deselection
                Debug.Log("Disabled outline on: " + outline.gameObject.name);
            }

            selectedObject = null;
        }
    }

    public void RotateSelectedObject()
    {
        if (selectedObject != null)
        {
            selectedObject.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }
    }

    // This function is called when the rotate button is pressed
    public void OnRotateButtonDown()
    {
        Debug.Log("Rotate Button Down - isRotating: " + isRotating);
        isRotating = true;
    }

    // This function is called when the rotate button is released
    public void OnRotateButtonUp()
    {
        Debug.Log("Rotate Button Up - isRotating: " + isRotating);
        isRotating = false;
    }

    public void ScaleSelectedObject(float scaleValue)
    {
        Debug.Log("Scale is triggered");

        if (selectedObject != null)
        {
            Debug.Log("Time to scale baby!!!!");
            Debug.Log(scaleValue);
            // Set the object's local scale based on the slider value
            selectedObject.transform.localScale = Vector3.one * scaleValue;
        }
    }

    public void SwitchFurniture(GameObject furniture)
    {
        SpawnableFurniture = furniture;
    }

    public bool isButtonPressed()
    {
        // Check if a UI button is currently selected
        return EventSystem.current.currentSelectedGameObject?.GetComponent<Button>() != null;
    }
}
