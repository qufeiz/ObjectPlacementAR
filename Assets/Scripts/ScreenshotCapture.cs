using UnityEngine;
using System.IO;

public class ScreenshotCapture : MonoBehaviour
{
    [Tooltip("Key to press to take a screenshot")]
    public KeyCode screenShotButton = KeyCode.S; // Default key is "S"

    private void Update()
    {
        if (Input.GetKeyDown(screenShotButton))
        {
            CaptureScreenshot();
        }
    }

    private void CaptureScreenshot()
    {
        // Create a Screenshots directory if it doesn't exist
        string path = Path.Combine(Application.dataPath, "Screenshots");
        Directory.CreateDirectory(path);

        // Use a timestamp to create unique filenames
        string screenshotName = $"screenshot_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png";
        string fullPath = Path.Combine(path, screenshotName);

        // Capture the screenshot
        ScreenCapture.CaptureScreenshot(fullPath);
        Debug.Log($"Screenshot saved: {fullPath}");
    }
}
