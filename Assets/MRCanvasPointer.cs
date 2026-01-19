using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MRCanvasPointer : MonoBehaviour
{
    public Transform rightHandAnchor;
    public Transform leftHandAnchor;
    public LineRenderer lineRenderer;
    public float maxDistance = 3.0f;
    public LayerMask uiLayerMask;

    public Color normalColor = Color.red;
    public Color hoverColor = Color.green;

    private Transform activeHand;

    void Start()
    {
        // Default to right hand
        activeHand = rightHandAnchor;
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }
        lineRenderer.startColor = normalColor;
        lineRenderer.endColor = normalColor;
    }

    void Update()
    {
        // Simple hand switching based on movement or input could go here
        // For now, we stick to the assigned activeHand (usually Right)
        
        UpdatePointer();
    }

    void UpdatePointer()
    {
        lineRenderer.SetPosition(0, activeHand.position);
        
        RaycastHit hit;
        // Raycast against the UI layer
        if (Physics.Raycast(activeHand.position, activeHand.forward, out hit, maxDistance, uiLayerMask))
        {
            // We hit something! Stop the line at the hit point
            lineRenderer.SetPosition(1, hit.point);

            // Check if we hit a button
            Button btn = hit.collider.GetComponent<Button>();
            if (btn != null)
            {
                // Visual Feedback: Turn Green
                lineRenderer.startColor = hoverColor;
                lineRenderer.endColor = hoverColor;

                // Check for click (Trigger press OR 'A'/'X' button)
                if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger) || 
                    OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) ||
                    OVRInput.GetDown(OVRInput.Button.One) || 
                    OVRInput.GetDown(OVRInput.Button.Three))
                {
                    btn.onClick.Invoke();
                }
            }
            else
            {
                // Hit something but not a button, revert color
                lineRenderer.startColor = normalColor;
                lineRenderer.endColor = normalColor;
            }
        }
        else
        {
            // Hit nothing, extend line to max distance
            lineRenderer.SetPosition(1, activeHand.position + activeHand.forward * maxDistance);
            lineRenderer.startColor = normalColor;
            lineRenderer.endColor = normalColor;
        }
    }
}
