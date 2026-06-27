using TMPro;
using UnityEngine;

public class TutorialConditionView : MonoBehaviour
{
    [SerializeField] private TMP_Text leftPlayerTxt;
    [SerializeField] private TMP_Text rightPlayerTxt;
    [SerializeField] private TMP_Text leftConditionTxt;
    [SerializeField] private TMP_Text rightConditionTxt;

    public void SetTexts(MiniGameTutorialContent content)
    {
        if (content.LeftPlayerText != null)
            leftPlayerTxt.text = content.LeftPlayerText;

        if (content.RightPlayerText != null)
            rightPlayerTxt.text = content.RightPlayerText;

        leftConditionTxt.text = content.LeftConditionText;
        rightConditionTxt.text = content.RightConditionText;
    }
}
