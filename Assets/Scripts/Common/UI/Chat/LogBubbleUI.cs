using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LogBubbleUI : MonoBehaviour
{
    [SerializeField] private Image profileImage;
    [SerializeField] private TMP_Text messageText;

    public void SetMessage(string message)
    {
        messageText.text = message;
    }

    public void SetProfile(Sprite sprite)
    {
        profileImage.sprite = sprite;
    }
}
