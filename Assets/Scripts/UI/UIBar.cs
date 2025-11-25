using UnityEngine;
using UnityEngine.UI;

public class UIValueBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;

    // Current and max values
    public void UpdateBar(float current, float max)
    {
        float fill = Mathf.Clamp01(current / max);
        Debug.Log("Fill: " + fill);
        fillImage.fillAmount = fill;

    }
}
