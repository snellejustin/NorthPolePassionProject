using UnityEngine;
using UnityEditor;

public class FrostSetupTool : EditorWindow
{
    [MenuItem("Tools/Setup Frost Quad")]
    public static void SetupFrostQuad()
    {
        // 1. Find Main Camera
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogError("Setup Frost: Could not find Main Camera tagged 'MainCamera'!");
            return;
        }

        // 2. Create Quad
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "FrostVisor";
        
        // 3. Parent and Transform
        quad.transform.SetParent(mainCam.transform, false);
        quad.transform.localPosition = new Vector3(0f, 0f, 0.35f); // 35cm in front
        quad.transform.localRotation = Quaternion.identity;
        quad.transform.localScale = new Vector3(0.8f, 0.5f, 1f); // Adjust scale

        // 4. Cleanup Components
        Collider col = quad.GetComponent<Collider>();
        if (col != null) DestroyImmediate(col);

        // 5. Assign Material
        string matPath = "Assets/Mat_FullScreenFrost.mat"; 
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        
        if (mat != null)
        {
            Renderer rend = quad.GetComponent<Renderer>();
            if (rend != null) rend.sharedMaterial = mat;
            Debug.Log("Setup Frost: Assigned 'Mat_FullScreenFrost' to Quad.");
        }
        else
        {
            Debug.LogWarning($"Setup Frost: Could not find material at '{matPath}'. Please assign it manually.");
        }

        // 6. Select it for the user
        Selection.activeGameObject = quad;
        Debug.Log("Setup Frost: 'FrostVisor' created successfully attached to Main Camera!");
    }
}
