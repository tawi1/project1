using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;


public class DistanceManager : Photon.PunBehaviour
{
    private int[] currentList;
    private Vector3[] line;
    private int lapDistance = 100000;
    private int nodeDistance = 100;

    private Dictionary<int, float> cars = new Dictionary<int, float>();
    private List<int> detectCars;
    [SerializeField]
    private Text currentLap;
    [SerializeField]
    private Text maxLap;
    [SerializeField]
    private Text currentPos;
    [SerializeField]
    private Text currentTime;
    private bool timerEnabled = false;
    private float time = 0;
    private CarLaps[] carList;
    private int currentSeconds = 0;
    int maxLaps = 3;
    private int id = -1;
    private float lockTime = 1;
    private bool locked = false;

    private void Start()
    {
        if (!FindObjectOfType<MatchController>().LobbyGame)
        {
            if (PhotonNetwork.room != null)
            {
                if (PhotonNetwork.room.CustomProperties.ContainsKey("maxLaps"))
                {
                    maxLaps = (int)PhotonNetwork.room.CustomProperties["maxLaps"];
                    maxLap.text = maxLaps.ToString();
                }
                else
                    maxLap.text = "3";
            }
            else
            {
                maxLap.text = "3";
            }

            GameObject t = GameObject.Find("Line3");
            if (t != null)
            {
                BoxCollider2D[] lineCol = t.GetComponentsInChildren<BoxCollider2D>();
                line = new Vector3[lineCol.Length];
                for (int i = 0; i < lineCol.Length; i++)
                {
                    line[i] = lineCol[i].gameObject.transform.position;
                }
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void InitCars()
    {
        carList = GameObject.FindObjectsOfType<CarLaps>();
    }

    private void AddCar(int idCar, float currentIndex)
    {
        cars.Add(idCar, currentIndex);
    }

    public void RemoveCar(int idPlayer)
    {
        if (currentList != null)
        {
            for (int i = 0; i < currentList.Length; i++)
            {
                if (PhotonView.Find(currentList[i]) == null)
                {
                    cars.Remove(i);
                    break;
                }
            }
            InitCars();
        }
    }

    private void ChangeDistance(int idCar, float distance)
    {
        if (distance < maxLaps * lapDistance)
            cars[idCar] = distance;
    }

    public void SetIndex(int idCar, int currentIndex)
    {
        int laps = 0;

        float distance = 0;
        bool changed = false;

        if (carList != null)
            for (int i = 0; i < carList.Length; i++)
            {
                CarLaps c = carList[i];
                if (c != null)
                    if (c.ViewID == idCar)
                    {
                        laps = c.Laps;
                        if (laps != maxLaps)
                        {
                            changed = true;
                            if (c.GetCarCheckpoint().GetCP(0))
                                distance = laps * lapDistance + currentIndex * nodeDistance;
                            else
                                distance = laps * lapDistance;
                        }
                        else
                        {
                            changed = false;
                        }
                        break;
                    }
            }

        if (changed)
        {
            if (cars.ContainsKey(idCar))
            {
                cars[idCar] = distance;
            }
            else
            {
                AddCar(idCar, distance);
            }

            if (locked == false)
            {
                locked = true;
                StartCoroutine(LockTimer());
                Solv();
            }
        }
    }

    public void SetWinner(int idCar, int place)
    {
        cars[idCar] = (maxLaps + 1) * lapDistance - place * 1000;
        Solv();
    }

    private void Solv()
    {
        if (cars.Keys.Count > 0)
        {
            var carScoreTemp = cars.OrderByDescending(key => key.Value);

            var namesTemp = carScoreTemp.Select(key => key.Key).ToArray();

            if (currentList != null)
            {
                if (currentList.SequenceEqual(namesTemp) == false)
                {
                    currentList = namesTemp;
                    UpdateScore();
                }
            }
            else
            {
                currentList = new int[namesTemp.Length];
                currentList = namesTemp;
                UpdateScore();
            }
        }
    }

    public void UpdateScore()
    {
        int[] ids = currentList;

        int j = 0;
        for (int i = 0; i < ids.Length; i++)
        {
            j++;
            if (ids[i] == id)
            {
                SetCurrentPos(j);
                break;
            }
        }
    }

    private void SetCurrentPos(int index)
    {
        if (index == 1)
        {
            currentPos.text = "1st";
        }
        else if (index == 2)
        {
            currentPos.text = "2nd";
        }
        else if (index == 3)
        {
            currentPos.text = "3rd";
        }
        else if (index == 4)
        {
            currentPos.text = "4th";
        }
    }

    public void UpdateLap(int lap)
    {
        currentLap.text = lap.ToString();
    }

    private IEnumerator LockTimer()
    {
        yield return new WaitForSeconds(lockTime);
        locked = false;
        yield return null;
    }

    public void StartTimer()
    {
        time = 0;
        currentSeconds = 0;
        timerEnabled = true;
        StartCoroutine(Timer());
    }

    public void StopTimer(int endTime)
    {
        timerEnabled = false;
        StopCoroutine(Timer());
        time = endTime;

        if (currentTime != null)
            if (endTime != 0)
                currentTime.text = TimeLib.GetTime(endTime).ToString();
            else
                currentTime.text = TimeLib.GetSeconds(0).ToString();
    }

    private IEnumerator Timer()
    {
        while (timerEnabled == true)
        {
            time += 1000 * Time.deltaTime;
            int seconds = TimeLib.GetTotalSeconds(time);
            if (seconds > currentSeconds)
            {
                currentSeconds = seconds;
                currentTime.text = TimeLib.GetSeconds((int)time).ToString();
            }
            yield return null;
        }
    }

    public void Reset()
    {
        if (currentLap != null)
            currentLap.text = "0";
    }

    public int ID
    {
        set
        {
            id = value;
        }
    }

    public bool Locked
    {
        get
        {
            return locked;
        }
    }
}
