using UnityEngine;
using UnityEngine.UI;

public class GameLoadingSpinner : MonoBehaviour
{
    [SerializeField]
    private Image spinnerImage = null;
    [SerializeField]
    private float spinnerSpeed = -400.0F;

    private void Update()
    {
        if (spinnerImage != null && spinnerImage.rectTransform != null)
        {
            spinnerImage.rectTransform.Rotate(0.0F, 0.0F, spinnerSpeed * Time.unscaledDeltaTime);
        }
    }
}
