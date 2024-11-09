using UnityEngine;
using UnityEngine.UI;

public class SliderListener : MonoBehaviour
{
    public FurniturePlacementManager placementManager; // Reference to your main script
    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();

        // Make sure the placementManager is assigned
        if (placementManager == null)
        {
            Debug.LogError("Placement Manager not assigned to SliderListener.");
        }
    }

    private void OnEnable()
    {
        slider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    private void OnDisable()
    {
        slider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }

    private void OnSliderValueChanged(float value)
    {
        // Call the scaling function in FurniturePlacementManager
        Debug.Log("new scale value from slider: "+ value);
        if (placementManager != null)
        {
            placementManager.ScaleSelectedObject(value);
        }
    }
}
