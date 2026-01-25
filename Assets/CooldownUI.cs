using UnityEngine;
using UnityEngine.UI;

public class CooldownUI : MonoBehaviour
{
    public Image fillImage;
    public Canvas canvas;

    public void SetProgress(float progress, bool isOverheated)
    {
        if (fillImage)
        {
            fillImage.fillAmount = progress;
            fillImage.color = isOverheated ? Color.red : Color.Lerp(Color.yellow, Color.red, progress);
        }
    }
}
