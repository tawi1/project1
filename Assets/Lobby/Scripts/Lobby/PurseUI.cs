using UnityEngine;
using UnityEngine.UI;

public class PurseUI : MonoBehaviour
{
    [SerializeField]
    private Text creditValue;
    [SerializeField]
    private Text topText;
    [SerializeField]
    private RectMover rectMover;
    private Vector2 startPos;
    private Vector2 targetPos;

    private void Start()
    {
        startPos = GetComponent<RectTransform>().localPosition;
        targetPos = new Vector2(startPos.x, startPos.y + GetComponent<RectTransform>().rect.height * 1.5f);
        GetComponent<RectTransform>().localPosition = targetPos;
    }

    public void SetValue(int value)
    {
        creditValue.text = value.ToString();
    }

    public void SetTopPanelName(string value)
    {
        topText.text = value;
    }

    public void SetStart()
    {
        GetComponent<RectTransform>().localPosition = startPos;
    }

    public void Move(int index)
    {
        if (index == 0)
        {
            rectMover.SetTargets(GetComponent<RectTransform>(), null, startPos, Vector2.zero);
        }
        else
        {
            rectMover.SetTargets(GetComponent<RectTransform>(), null, targetPos, Vector2.zero);
        }
    }
}

