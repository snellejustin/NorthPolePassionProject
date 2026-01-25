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
        currentProgress = progress;
        if (progressImage)
        {
            progressImage.fillAmount = currentProgress;
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
