using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class ScoreMenu : MonoBehaviour
{
    string[] scoreTitle = { "Race", "Place score", "Wreck", "Drifting", "Bonus" };
    int[] scoreValue;

    [SerializeField]
    private Transform bonusList;
    [SerializeField]
    GameObject scorePrefab;
    ScoreManager scoreManager;
    [SerializeField]
    Text totalScoreValue;
    [SerializeField]
    private GameObject scorePanel;
    [SerializeField]
    private Sprite[] places;
    [SerializeField]
    private Image place;
    private int step = 0;

    private float stepTime = 1f;
    private float delay = 2f;
    private float currentDelay;
    private float nullDelay = 0.5f;
    private bool nullStep = false;

    private List<ScoreSlot> slots = new List<ScoreSlot>();

    private bool start = false;
    private float currentScore = 0;
    private float currentTotalScore = 0;
    private float scoreStep;

    void FixedUpdate()
    {
        if (start)
        {
            if (step < scoreValue.Length)
            {
                if (!slots[step].gameObject.activeSelf)
                {
                    slots[step].gameObject.SetActive(true);
                    StartCoroutine(ScoreDelay());
                }

                if ((currentScore < scoreValue[step] && nullStep == false) || (currentScore < nullDelay && nullStep == true))
                {
                    currentScore += scoreStep;
                    if (nullStep == false)
                    {
                        currentTotalScore += scoreStep;
                        slots[step].SetScore(scoreTitle[step], Mathf.FloorToInt(currentScore));
                        totalScoreValue.text = Mathf.FloorToInt(currentTotalScore).ToString();
                    }
                }
                else
                {
                    slots[step].SetScore(scoreTitle[step], scoreValue[step]);
                    step++;
                    currentScore = 0;

                    if (step < scoreValue.Length)
                    {
                        SolvScoreStep(step);
                    }
                }
            }
            else
            {
                if (currentDelay < delay)
                {
                    currentDelay += Time.fixedDeltaTime;
                }
                else
                {
                    scorePanel.SetActive(false);
                    SetFinishValues();
                    Reset();
                }
            }
        }
    }

    private void Reset()
    {
        start = false;
        step = 0;
        currentScore = 0;
        currentTotalScore = 0;
        currentDelay = 0;
    }

    private void SetFinishValues()
    {
        totalScoreValue.text = Mathf.FloorToInt(scoreManager.Score).ToString();
    }

    IEnumerator ScoreDelay()
    {
        bonusList.GetComponent<VerticalLayoutGroup>().spacing = 1;
        yield return new WaitForSeconds(0.01f);
        bonusList.GetComponent<VerticalLayoutGroup>().spacing = 0;
    }

    public void SetPlace(int index)
    {
        if (index < places.Length)
        {
            place.overrideSprite = places[index];
        }
        else
        {
            place.gameObject.SetActive(false);
        }
    }

    public void Init()
    {
        if (scoreManager == null)
            scoreManager = FindObjectOfType<ScoreManager>();
        ClearList();
    }

    private void GetScore()
    {
        scoreValue = new int[scoreTitle.Length];
        totalScoreValue.text = scoreManager.Score.ToString();
        scoreValue[0] = scoreManager.RaceScore;
        scoreValue[1] = scoreManager.WinnerScore;
        scoreValue[2] = scoreManager.WreckScore;
        scoreValue[3] = scoreManager.DriftScore;
        scoreValue[4] = scoreManager.BonusScore;
    }

    private void CreateListScore()
    {
        //CreateScore(scoreTitle[0], scoreValue[0]);
        for (int i = 0; i < scoreTitle.Length; i++)
        {
            CreateScore(scoreTitle[i], scoreValue[i]);
        }

        for (int i = 1; i < scoreTitle.Length; i++)
        {
            slots[i].gameObject.SetActive(false);
        }
    }



    public void OnScorePanelClick()
    {
        if (start == true)
        {
            Reset();

            for (int i = 0; i < scoreTitle.Length; i++)
            {
                slots[i].SetScore(scoreTitle[i], scoreValue[i]);
                slots[i].gameObject.SetActive(true);
            }

            SetFinishValues();
        }
        else
        {
            scorePanel.SetActive(false);
        }
    }

    private void CreateScore(string bonusName, int scoreValue)
    {
        ScoreSlot slot = Instantiate(scorePrefab, bonusList).GetComponent<ScoreSlot>();

        slots.Add(slot);
        slot.SetScore(bonusName, scoreValue);
        //slot.gameObject.SetActive(false);
    }

    private void ClearList()
    {
        Transform[] list = bonusList.GetComponentsInChildren<Transform>();

        if (list != null)
        {
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i] != bonusList.transform)
                    Destroy(list[i].gameObject);
            }
        }
        slots.Clear();
    }

    public void OnExitClick()
    {
        MatchMenu.OnExitClick();
    }

    public void OnShareClick()
    {

    }

    private void SolvScoreStep(int index)
    {
        if (scoreValue[index] > 0)
        {
            nullStep = false;
            scoreStep = (float)scoreValue[index] / (stepTime / Time.fixedDeltaTime);
        }
        else
        {
            nullStep = true;
            scoreStep = Time.fixedDeltaTime;
        }
    }

    public void Launch()
    {
        Init();
        GetScore();
        CreateListScore();
        Reset();
        slots[0].gameObject.SetActive(true);
        SetPlace(scoreManager.Place - 1);
        SolvScoreStep(0);
        start = true;
        gameObject.SetActive(true);
        scorePanel.SetActive(true);
    }
}

