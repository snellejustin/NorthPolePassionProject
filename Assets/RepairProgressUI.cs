using UnityEngine;
using UnityEngine.UI;

public class RepairProgressUI : MonoBehaviour
{
    public Image progressImage;
    public GameObject canvasObject;

    private float currentProgress = 0f;

    void Start()
    {
        // Ensure it starts hidden or empty
        if (progressImage) progressImage.fillAmount = 0f;
        Hide();
    }

    public void SetProgress(float progress)
    {
        currentProgress = Mathf.Clamp01(progress);
        if (progressImage)
        {
            progressImage.fillAmount = currentProgress;

            // Change color: Red -> Yellow -> Green
            if (currentProgress < 0.5f)
            {
                // Red to Yellow
                progressImage.color = Color.Lerp(Color.red, Color.yellow, currentProgress * 2f);
            }
            else
            {
                // Yellow to Green
                progressImage.color = Color.Lerp(Color.yellow, Color.green, (currentProgress - 0.5f) * 2f);
            }
        }

        // Show if we have some progress, hide if 0 (optional, or keep visible)
        if (progress > 0f && progress < 1f)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    public void Show()
    {
        if (canvasObject) canvasObject.SetActive(true);
    }

    public void Hide()
    {
        if (canvasObject) canvasObject.SetActive(false);
    }
}
