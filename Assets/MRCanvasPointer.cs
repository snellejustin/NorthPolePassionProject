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

    private Transform activeHand;

    void Start()
    {
        // Default to right hand
        activeHand = rightHandAnchor;
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }
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
        if (Physics.Raycast(activeHand.position, activeHand.forward, out hit, maxDistance, uiLayerMask))
        {
            lineRenderer.SetPosition(1, hit.point);

            // Check if we hit a button
            Button btn = hit.collider.GetComponent<Button>();
            if (btn != null)
            {
                // Highlight logic could go here (e.g. btn.Select())

                // Check for click
                if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger) || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
                {
                    btn.onClick.Invoke();
                }
            }
        }
        else
        {
            lineRenderer.SetPosition(1, activeHand.position + activeHand.forward * maxDistance);
        }
    }
}
