using UnityEngine;
using UnityEngine.UI;

public class RepairProgressUI : MonoBehaviour
{
    public Image progressImage;
    public GameObject canvasObject;

    private float currentProgress = 0f;

    void Start()
    {
        if (progressImage) progressImage.fillAmount = 0f;
        Hide();
    }

    public void SetProgress(float progress)
    {
        currentProgress = Mathf.Clamp01(progress);
        if (progressImage)
        {
            progressImage.fillAmount = currentProgress;

            if (currentProgress < 0.5f)
            {
                progressImage.color = Color.Lerp(Color.red, Color.yellow, currentProgress * 2f);
            }
            else
            {
                progressImage.color = Color.Lerp(Color.yellow, Color.green, (currentProgress - 0.5f) * 2f);
            }
        }

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
