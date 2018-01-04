using UnityEngine;
using UnityEngine.UI;

public class ScoreSlot : MonoBehaviour
{

    [SerializeField]
    private Text scoreName;
    [SerializeField]
    private Text scoreValue;

    public void SetScore(string scoreName, int scoreValue)
    {
        this.scoreName.text = scoreName;
        this.scoreValue.text = scoreValue.ToString();
    }
}
