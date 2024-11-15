using UnityEngine;
using System.IO;

public class GameObjectScreenshot : MonoBehaviour {
    public GameObject targetObject; // The GameObject you want to capture
    public int resolutionWidth = 1024; // Width of the captured image
    public int resolutionHeight = 1024; // Height of the captured image
    public string savePath = "Screenshot.png"; // Path to save the PNG

    public void CaptureGameObject() {
        if (targetObject == null) {
            Debug.LogError("Target Object is not set.");
            return;
        }

        // Create a temporary camera
        GameObject tempCameraObject = new GameObject("TempCamera");
        Camera tempCamera = tempCameraObject.AddComponent<Camera>();

        // Set the temporary camera's position and rotation to match the target object
        tempCamera.transform.position = targetObject.transform.position + new Vector3(0, 0, -10);
        tempCamera.transform.LookAt(targetObject.transform);

        // Set the culling mask to render only the target object
        tempCamera.cullingMask = 1 << targetObject.layer;

        // Create a RenderTexture
        RenderTexture renderTexture = new RenderTexture(resolutionWidth, resolutionHeight, 24);
        tempCamera.targetTexture = renderTexture;

        // Render the target object to the RenderTexture
        tempCamera.Render();

        // Read the RenderTexture into a Texture2D
        RenderTexture.active = renderTexture;
        Texture2D texture = new Texture2D(resolutionWidth, resolutionHeight, TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(0, 0, resolutionWidth, resolutionHeight), 0, 0);
        texture.Apply();

        // Save the Texture2D as a PNG
        byte[] pngData = texture.EncodeToPNG();
        File.WriteAllBytes(Path.Combine(Application.persistentDataPath, savePath), pngData);

        Debug.Log($"Screenshot saved to: {Path.Combine(Application.persistentDataPath, savePath)}");

        // Cleanup
        RenderTexture.active = null;
        tempCamera.targetTexture = null;
        Destroy(renderTexture);
        Destroy(tempCameraObject);
        Destroy(texture);
    }
}
