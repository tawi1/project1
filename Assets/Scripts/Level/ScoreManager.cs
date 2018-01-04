using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private float driftMult = 3f;
    private int explodeMult = 40;
    private int bonusMult = 5;
    private int levelMult = 150;
    private int oilMult = 4;
    private int frozeMult = 20;

    private int score = 0;
    private int raceScore = 0;
    private int winnerScore = 0;
    private int wreckScore = 0;
    private int driftScore = 0;
    private int bonusScore = 0;

    private int maxLaps = 3;
    private int winnerBonus = 50;
    private int maxPlayers = 4;
    private int place = 4;
    private int penalty = 0;
    private float easyAi = 0.7f;
    private float normalAi = 0.4f;
    private int easyAiCount = 0;
    private int normalAiCount = 0;

    [SerializeField]
    private float levelRate = 1;
    [SerializeField]
    private float bonusRate = 1;

    public void SolvFinish()
    {
        if (maxLaps >= 2)
        {
            score += SolvRaceScore() + SolvWinnerScore() + SolvOther();
        }
        else
        {
            score += SolvRaceScore() + SolvOther();
        }

        Purse.AddCredit(score);
        SaveData.Save();
    }

    private int SolvRaceScore()
    {
        raceScore = (int)Mathf.Floor(levelMult * maxLaps * levelRate);
        return raceScore;
    }

    private int SolvWinnerScore()
    {
        winnerScore = (maxPlayers + 1 - place) * winnerBonus - SolvPenalty();
        return winnerScore;
    }

    private int SolvPenalty()
    {
        penalty = (int)Mathf.Floor(winnerBonus * (easyAiCount * easyAi + normalAi * normalAiCount));
        return penalty;
    }

    private int SolvOther()
    {
        int other = wreckScore + driftScore + bonusScore;
        return other;
    }

    public void AddExplodeScore()
    {
        wreckScore += (int)Mathf.Floor(explodeMult * bonusRate);
    }

    public void AddFrozeScore()
    {
        wreckScore += (int)Mathf.Floor(frozeMult * bonusRate);
    }

    public void AddScore()
    {
        bonusScore += (int)Mathf.Floor(bonusMult * bonusRate);
    }

    public void AddOilScore()
    {
        bonusScore += oilMult;
    }

    public void AddDriftScore(float currentTime)
    {
        driftScore += (int)Mathf.Floor(currentTime * driftMult);
    }

    public void ResetScore()
    {
        score = 0;
        raceScore = 0;
        winnerScore = 0;
        wreckScore = 0;
        driftScore = 0;
        bonusScore = 0;
    }

    public void AddAi(int level)
    {
        if (level == 1)
        {
            normalAiCount++;
        }
        else if (level == 2)
        {
            easyAiCount++;
        }
    }

    public int Score
    {
        get
        {
            return score;
        }
    }

    public int RaceScore
    {
        get
        {
            return raceScore;
        }
    }

    public int WinnerScore
    {
        get
        {
            return winnerScore;
        }
    }

    public int WreckScore
    {
        get
        {
            return wreckScore;
        }
    }

    public int DriftScore
    {
        get
        {
            return driftScore;
        }
    }

    public int BonusScore
    {
        get
        {
            return bonusScore;
        }
    }

    public int MaxLaps
    {
        set
        {
            maxLaps = value;
        }
    }

    public int MaxPlayers
    {
        set
        {
            maxPlayers = value;
        }
    }

    public int Place
    {
        get
        {
            return place;
        }
        set
        {
            place = value;
        }
    }
}
