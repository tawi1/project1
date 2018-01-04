using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathFollower : Photon.PunBehaviour
{
    List<Vector3> line = new List<Vector3>();
    List<Vector3> cpsPos = new List<Vector3>();
    [SerializeField]
    List<int> cpsIndex = new List<int>();
    CarScript carScript;
    CarCheckpoint carCheckpoint;
    CarLaps carLaps;
    AiButtons buttons;

    bool rotateClicked = false;
    private bool[] bonuses;

    float distance = 10f;
    [SerializeField]
    private int currentIndex = 0;
    private bool currentNitro = false;

    private int nodeAngle = 15;
    private int shotAngle = 4;
    private int targetAngle = 30;
    private float minDist = 10f;
    private float shootDistance = 18f;
    float clickTime = 0.15f;

    private bool searching = false;
    private float oilDist = 9f;
    private float flameDist = 7f;

    private Bonus bonus;
    private float bonusDist = 18f;
    Transform _targetEnemy;
    Transform targetBonus;
    Vector3 corrective = Vector3.zero;
    private bool bonusVisible = false;
    GameObject[] players;
    ObjectManager objectManager;
    private int aiLevel = 1;
    private int stuck = 0;
    private bool resetIndex = false;
    private bool doubleResetIndex = false;
    private List<int> Points = new List<int>();
    private Transform gameobjectTransform;
    private float sizeX;
    private float doubleResetDist = 9f;
    private int checkP = 2;
    private bool cps = false;
    [SerializeField]
    private int cpTarget = -1;
    private Vector3 startPosition;
    private DistanceHitter distanceHitter;

    private Vector2 hitPos = Vector2.zero; private Vector2 startPos = Vector2.zero;

    void Start()
    {
        carScript = gameObject.GetComponent<CarScript>();
        carCheckpoint= gameObject.GetComponent<CarCheckpoint>();
        distanceHitter = GetComponent<DistanceHitter>();
        carLaps = GetComponent<CarLaps>();
        buttons = gameObject.GetComponent<AiButtons>();
        bonus = gameObject.GetComponent<Bonus>();
        gameobjectTransform = gameObject.transform;
        sizeX = gameObject.GetComponent<CapsuleCollider2D>().size.x * gameobjectTransform.localScale.x;
        players = GameObject.FindGameObjectsWithTag("Player");
        distance = 1.3f * GameObject.Find("Track").GetComponent<LineRenderer>().startWidth;
        objectManager = FindObjectOfType<ObjectManager>();
        aiLevel = carScript.AiLevel;
        InitAiLevel();
        line = PathCalculation.PathCalc(false, aiLevel);
        checkP = carCheckpoint.CheckPCount;

        AddPoints();
        if (checkP > 2)
        {
            cps = true;
            AddCps();
        }
    }

    private void AddCps()
    {
        GameObject start = GameObject.Find("Start");
        startPosition = start.transform.position;
        bool reverse = start.GetComponent<StartController>().Reverse;

        CheckPoint[] cp = carCheckpoint.GetCp();
        Vector3[] track = carCheckpoint.GetTrack();
        for (int i = 0; i < cp.Length; i++)
        {
            cpsPos.Add(cp[i].gameObject.transform.position);
        }

        if (track != null)
        {
            List<int> cpsIndexLocal = new List<int>();
            for (int j = 0; j < cpsPos.Count; j++)
            {
                int currentInd = 0;
                float minDist = 999999;
                float dist = 0;
                for (int i = 0; i < track.Length; i++)
                {
                    if (i - 1 > 0)
                    {
                        dist = Vector2.Distance(cpsPos[j], track[i - 1]) + Vector2.Distance(cpsPos[j], track[i]);
                    }
                    else
                        dist = Vector2.Distance(cpsPos[j], track[track.Length - 1]) + Vector2.Distance(cpsPos[j], track[i]);

                    if (dist < minDist)
                    {
                        minDist = dist;
                        currentInd = i;
                    }
                }
                cpsIndexLocal.Add(currentInd);
            }

            List<int> distNodes = new List<int>();
            distNodes = carCheckpoint.GetDistNodes();
            for (int j = 0; j < cpsIndexLocal.Count; j++)
            {
                int currentInd = 0;
                for (int i = 0; i < Points.Count; i++)
                {
                    int prevIndex = i - 1;

                    if (!reverse)
                    {
                        if (prevIndex < 0)
                            prevIndex = Points.Count - 1;
                    }
                    else
                    {
                        prevIndex = i + 1;
                        if (prevIndex >= Points.Count)
                            prevIndex = 0;
                    }

                    int prevIndexTrack = distNodes[Points[prevIndex]];
                    int currentIndexTrack = distNodes[Points[i]];
                    if (cpsIndexLocal[j] >= prevIndexTrack && cpsIndexLocal[j] <= currentIndexTrack)
                    {
                        if (!reverse)
                        {
                            currentInd = i;
                        }
                        else
                        {
                            currentInd = i + 1;
                            if (currentInd >= Points.Count)
                            {
                                currentInd = 0;
                            }
                        }
                        break;
                    }
                }
                cpsIndex.Add(currentInd);
            }
        }
    }

    private void AddPoints()
    {
        GameObject t = GameObject.Find("Line3");
        DistanceNode[] g = t.GetComponentsInChildren<DistanceNode>();

        for (int i = 0; i < line.Count; i++)
        {
            float minNodeDist = 99999;
            int nodeIndex = 0;
            for (int j = 0; j < g.Length; j++)
            {
                float currentDist = Vector2.Distance(line[i], g[j].gameObject.transform.position);
                if (currentDist < minNodeDist)
                {
                    minNodeDist = currentDist;
                    nodeIndex = g[j].Index;
                }
            }
            Points.Add(nodeIndex);
        }
    }

    public void InitAiLevel()
    {
        if (aiLevel == 1)
        {
            nodeAngle = 25;
            shotAngle = 15;
            minDist = 4;
            shootDistance = 14;
            clickTime = 0.3f;
        }
        else if (aiLevel == 2)
        {
            nodeAngle = 35;
            shotAngle = 25;
            minDist = 0;
            shootDistance = 12;
            clickTime = 0.45f;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(gameobjectTransform.position, distance);
        if (startPos != Vector2.zero)
            Gizmos.DrawWireSphere(startPos, distance);

        if (hitPos != Vector2.zero)
            Gizmos.DrawWireSphere(hitPos, distance);
    }

    // Find the closest visible enemy.
    private void UpdateTargetEnemy(Vector2 axis, int type)
    {
        Vector3 localCorrective = Vector2.zero;
        Transform target = null;

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] != null)
            {
                GameObject player = players[i];
                float dist = Vector2.Distance(player.transform.position, gameobjectTransform.position);

                bool condition = false;
                if (type == 0 || type == 1)
                {
                    if (player.GetPhotonView().viewID != gameObject.GetPhotonView().viewID && dist < shootDistance && dist > minDist)
                    {
                        condition = true;
                    }
                }
                else
                {
                    if (player.GetPhotonView().viewID != gameObject.GetPhotonView().viewID && dist < shootDistance && dist > 1)
                    {
                        condition = true;
                    }
                }

                if (condition)
                {
                    Vector2 dir = player.transform.position - gameobjectTransform.position;
                    float dot = Vector2.Dot(dir, axis);

                    if (dot >= 0)
                    {
                        float angle = Vector2.Angle(dir, axis);
                        if (angle < targetAngle)
                        {
                            Vector3 rayOffSet = GetForwardRaycast();
                            RaycastHit2D hitInfo = Physics2D.Raycast(rayOffSet, dir);

                            //Debug.Log(hitInfo.transform.gameObject.name);
                            if (hitInfo.transform == player.transform && !player.GetComponent<CarSkin>().CarHided())
                            {
                                float time = 0;
                                if (type == 0)
                                    time = dist / gameObject.GetComponent<Bonus>().Rocket.GetComponent<RocketLaunch>().RocketSpeed;
                                else
                                if (type == 1)
                                    time = dist / IceLaunch.iceSpeed;
                                else
                                    time = dist / LaserLaunch.laserSpeed;
                                localCorrective = player.GetComponent<Rigidbody2D>().velocity * time;
                                target = player.transform;
                                break;
                            }
                        }
                    }
                }
            }
        }

        if (target != null)
        {
            _targetEnemy = target;
            corrective = localCorrective;
        }
        else
        {
            _targetEnemy = null;
            corrective = Vector3.zero;
        }
    }

    private Vector2 GetForwardRaycast()
    {
        return gameobjectTransform.position + gameobjectTransform.TransformDirection(new Vector2(sizeX / 2 + 0.1f, 0));
    }

    private Vector2 GetBackwardRaycast()
    {
        return gameobjectTransform.position + gameobjectTransform.TransformDirection(new Vector2(-(sizeX / 2 + 0.1f), 0));
    }

    private void CheckBackward(bool flame)
    {
        for (int i = 0; i < players.Length; i++)
        {
            GameObject player = players[i];
            if (player != null)
            {
                Transform playerTransform = player.transform;
                bool condition = false;

                float dist = Vector2.Distance(playerTransform.position, gameobjectTransform.position);

                if (!flame)
                {
                    if (player.GetPhotonView().viewID != gameObject.GetPhotonView().viewID && dist <= oilDist)
                        condition = true;
                }
                else
                {
                    if (player.GetPhotonView().viewID != gameObject.GetPhotonView().viewID && dist <= flameDist)
                        condition = true;
                }

                if (condition)
                {
                    Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

                    if (rb.velocity.magnitude >= 5f)
                    {
                        Vector3 axisPoint = carScript.GetWheelB();
                        Vector3 dir1 = player.GetComponent<CarScript>().GetWheelB2() - axisPoint;
                        Vector2 axis = carScript.GetWheelBAxis();

                        float angle1 = Vector2.Angle(dir1, axis);

                        Vector3 axisPoint2 = carScript.GetWheelB2();
                        Vector2 dir2 = player.GetComponent<CarScript>().GetWheelB() - axisPoint2;
                        Vector2 axis2 = carScript.GetWheelB2Axis();

                        float angle2 = Vector2.Angle(dir2, axis2);

                        Vector2 axis3 = carScript.GetBackAxis();
                        float angle3 = Vector2.Angle(axis3, rb.velocity);

                        float dot = Vector2.Dot(axis3, rb.velocity);
                        if (angle1 <= 90 && angle2 <= 90 && ((Mathf.Abs(angle3 - 90) < 40 && dot >= 0) || dist < 3f))
                        {
                            Vector3 backRayOffSet = GetBackwardRaycast();
                            Vector3 dir = playerTransform.position - backRayOffSet;
                            RaycastHit2D hitInfo = Physics2D.Raycast(backRayOffSet, dir);
                            if (hitInfo.transform == playerTransform)
                            {

                                buttons.ClickOil(true);
                                buttons.ClickBomb(true);
                                buttons.ClickFlame(true);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    private void SearchBonus()
    {
        List<BonusNode> mapBonuses = objectManager.GetMap();
        bool found = false;
        if (mapBonuses != null)
        {
            for (int i = 0; i < mapBonuses.Count; i++)
            {
                BonusNode bonus = mapBonuses[i];

                if (bonus != null)
                {
                    if (bonuses[bonus.Index] == false)
                    {
                        Transform transformBonus = bonus.transform;
                        float dist = Vector2.Distance(transformBonus.position, gameobjectTransform.position);
                        if (dist < bonusDist && dist > 5)
                        {
                            Vector2 dir = transformBonus.position - carScript.GetAxisFPoint();
                            Vector2 axis = carScript.GetCentralAxis();
                            float angle = Vector2.Angle(dir, axis);
                            float dot = Vector2.Dot(dir, axis);

                            if ((Mathf.Abs(angle) < 40) && !bonusVisible && dot > 0)
                            {
                                Vector3 rayOffSet = GetForwardRaycast();
                                dir = transformBonus.position - rayOffSet;
                                RaycastHit2D hitInfo = Physics2D.Raycast(rayOffSet, dir);

                                if (hitInfo.transform == transformBonus)
                                {
                                    targetBonus = transformBonus;
                                    found = true;
                                    bonusVisible = true;
                                    break;
                                }
                            }
                            else if ((Mathf.Abs(angle) < 40) && bonusVisible)
                            {
                                found = true;
                            }
                        }
                    }
                }
            }
            if (!found)
            {
                targetBonus = null;
                bonusVisible = false;
            }
        }
    }

    private float GetDistance()
    {
        if (_targetEnemy != null)
            return Vector2.Distance(_targetEnemy.position, gameobjectTransform.position);
        else
            return 0;
    }

    private bool CheckObstacle(Vector3 checkPoint)
    {
        bool rotate = true;
        Vector3 rayOffSet = GetForwardRaycast();
        Vector2 dir = checkPoint - rayOffSet;
        float rayDist = Vector2.Distance(checkPoint, gameobjectTransform.position) - 1;
        RaycastHit2D hitInfo = Physics2D.Raycast(rayOffSet, dir, rayDist);
        if (hitInfo)
        {
            Transform hit = hitInfo.transform;
            if (hit.gameObject.tag == "Barrier")
            {
                /*(float obstacleDist = Vector2.Distance(hit.position, rayOffSet);

                if (obstacleDist < rayDist)
                {*/
                rotate = false;

            }
        }
        return rotate;
    }

    public int Stuck
    {
        set
        {
            stuck = value;
        }
    }

    public void DisableReset()
    {
        resetIndex = false;
    }

    void FixedUpdate()
    {
        if (!photonView.isMine || !carScript.AI)
            return;

        if (Vector2.Distance(line[currentIndex], gameobjectTransform.position) < distance && stuck == 0)
        {
            if (line[currentIndex].z == 1)
                currentNitro = true;
            else
                currentNitro = false;

            bool check = true;
            if (resetIndex)
            {
                check = CheckObstacle(line[currentIndex]);

                if (!check)
                {
                    int index = currentIndex - 1;
                    if (index < 0)
                        index = line.Count - 1;
                    float newDist = Vector2.Distance(line[index], gameobjectTransform.position);
                    if (newDist < 5)
                    {
                        check = true;
                        buttons.OnRespawnClick();
                    }
                }
            }

            if (check)
            {
                if (cps == false)
                {
                    currentIndex++;
                }
                else
                {
                    int currentCp = carCheckpoint.GetLastCP();
                    if (currentCp >= 0 || currentIndex == 0)
                    {
                        if (currentCp != -1)
                        {
                            if ((cpsIndex[currentCp] < currentIndex && cpsIndex[currentCp] != 0) || (cpsIndex[currentCp] < currentIndex && currentIndex < 5))
                            {
                                cpTarget = currentCp;
                                currentIndex = cpsIndex[currentCp];
                            }
                            else
                            {
                                cpTarget = -1;
                                currentIndex++;
                            }
                        }
                        else
                        {
                            cpTarget = -1;
                            currentIndex++;
                        }
                    }
                    else
                    {
                        cpTarget = -2;
                    }
                }

                resetIndex = false;
                doubleResetIndex = false;
            }

            if (currentNitro)
                buttons.ClickNitro(true);
            else
                buttons.ClickNitro(false);
        }
        else
        {
            if (resetIndex)
            {
                bool check = !CheckObstacle(line[currentIndex]);

                if (!check)
                {
                    int index = currentIndex - 1;
                    if (index < 0)
                        index = line.Count - 1;
                    float newDist = Vector2.Distance(line[index], gameobjectTransform.position);
                    if (newDist < 5)
                    {
                        stuck = 0;
                        resetIndex = false;
                        buttons.OnRespawnClick();
                    }
                }
            }
            else if (doubleResetIndex)
            {
                int index = currentIndex - 2;
                if (index < 0)
                    index += line.Count;
                float newDist = Vector2.Distance(line[index], gameobjectTransform.position);
                if (newDist < doubleResetDist)
                {
                    doubleResetIndex = false;
                }
            }

        }

        if (currentIndex >= line.Count)
            currentIndex = 0;

        bonuses = bonus.GetBonus();
        Vector2 centralAxis = carScript.GetCentralAxis();

        if (bonuses != null && carScript.Control)
        {
            if (bonuses[1])
            {
                searching = true;
                UpdateTargetEnemy(centralAxis, 0);
            }
            else if (bonuses[6])
            {
                searching = true;
                UpdateTargetEnemy(centralAxis, 1);
            }
            else if (bonuses[5])
            {
                searching = true;
                UpdateTargetEnemy(centralAxis, 2);
            }
            else
                searching = false;

            if (bonuses[2] || bonuses[3])
                CheckBackward(false);
            else if (bonuses[4])
                CheckBackward(true);
            else
            {
                buttons.ClickOil(false);
                buttons.ClickBomb(false);
                buttons.ClickFlame(false);
            }
        }

        Vector2 dir = Vector2.zero;
        int finishAngle = 0;


        if (!_targetEnemy || !searching)
        {
            SearchBonus();
            if (targetBonus != null)
                dir = targetBonus.position - carScript.GetAxisPoint();
            else
            {
                if (cpTarget == -1)
                {
                    int targetIndex = currentIndex;
                    if (resetIndex)
                    {
                        targetIndex--;
                    }
                    else if (doubleResetIndex)
                    {
                        targetIndex -= 2;
                    }

                    if (targetIndex < 0)
                    {
                        targetIndex += line.Count;
                    }

                    dir = line[targetIndex] - carScript.GetAxisPoint();
                }
                else
                {
                    if (cpTarget != -2)
                        dir = cpsPos[cpTarget] - carScript.GetAxisPoint();
                    else
                        dir = startPosition - carScript.GetAxisPoint();
                }
            }
            finishAngle = nodeAngle;
        }
        else
        {
            dir = (_targetEnemy.position + corrective) - carScript.GetAxisPoint();
            finishAngle = shotAngle;
        }

        Vector2 axis = carScript.GetBackAxis();

        float angle = Vector2.Angle(dir, axis);


        float currentDot = Vector2.Dot(dir, centralAxis);

        float currentAngle = Mathf.Abs(angle - 90);

        if (stuck == 0)
        {
            if (currentAngle > finishAngle || currentDot < 0)
            {
                bool rotate = true;
                if (finishAngle == nodeAngle && carScript.Control)
                {
                    //Debug.Log(currentAngle);
                    if (carScript.Velocity < 20f && (currentDot > 0))
                    {
                        int rotateAngle = 30;
                        if (currentAngle > rotateAngle)
                        {
                            rotate = CheckObstacle(line[currentIndex]);
                        }
                    }
                    else
                    {
                        int prevIndex = currentIndex - 1;
                        if (prevIndex < 0)
                            prevIndex = line.Count - 1;

                        Vector2 dir2 = line[prevIndex] - gameobjectTransform.position;
                        float dot = Vector2.Dot(dir2, centralAxis);
                        if (dot > 0)
                        {
                            bool obs = CheckObstacle(line[prevIndex]);
                            if (!obs)
                            {
                                doubleResetIndex = true;
                                prevIndex = prevIndex - 1;
                                if (prevIndex < 0)
                                    prevIndex = line.Count - 1;
                                rotate = false;
                            }
                        }
                        float prevDist = Vector2.Distance(line[prevIndex], gameobjectTransform.position);

                        if (prevDist < doubleResetDist)
                        {
                            rotate = true;
                            doubleResetIndex = false;
                        }
                        //Debug.Log("prevDist=" + prevDist + " " + prevIndex + " " + doubleResetIndex + " " + rotate);
                    }

                }

                if (rotate)
                {
                    if (angle - 90 > 0)
                    {
                        if (rotateClicked == false)
                            StartCoroutine(click(true));
                    }
                    else
                    {
                        if (rotateClicked == false)
                            StartCoroutine(click(false));
                    }
                }
                else
                {
                    DisableRotation();
                }
            }
            else
            {
                float currentDist = GetDistance();
                if (_targetEnemy && searching && currentDist > minDist)
                {
                    buttons.ClickRocket(true);
                    buttons.ClickLaser(true);
                    buttons.ClickIce(true);
                }
                else if (_targetEnemy && searching && currentDist > 2)
                {
                    buttons.ClickLaser(true);
                }
                else
                {
                    buttons.ClickRocket(false);
                    buttons.ClickLaser(false);
                    buttons.ClickIce(false);
                }

                DisableRotation();
            }
            buttons.AIClickUp(true);
        }
        else
        {
            bool backward = false;
            if (currentAngle > nodeAngle)
            {
                backward = true;
                if (!backwardClicked)
                {
                    backwardClicked = true;
                    StartCoroutine(backwardTimer());
                }
            }
            else
            {
                backward = !CheckObstacle(line[currentIndex]);
                if (backward)
                {
                    resetIndex = false;

                    int index = currentIndex - 1;
                    if (index < 0)
                    {
                        index = line.Count - 1;
                    }

                    bool oldIndex = !CheckObstacle(line[index]);

                    if (oldIndex)
                    {
                        resetIndex = false;
                        currentIndex = ResetIndex(currentIndex + 1);
                    }
                    else
                    {
                        resetIndex = true;
                    }
                }
                else
                {
                    resetIndex = false;
                }
            }

            //Debug.Log(currentAngle + " " + stuck);

            if (backward)
            {
                if (stuck == 1)
                {
                    buttons.AIClickUp(false);
                    buttons.AIClickDown(true);
                    if (angle - 90 > 0)
                    {
                        if (rotateClicked == false)
                            StartCoroutine(click(false));
                    }
                    else
                    {
                        if (rotateClicked == false)
                            StartCoroutine(click(true));
                    }
                }
                else
                {
                    if (carScript.BurnoutAllowed)
                        buttons.AIClickUp(true);
                    else
                        buttons.AIClickUp(false);

                    buttons.AIClickDown(false);
                    if (angle - 90 > 0)
                    {
                        if (rotateClicked == false)
                            StartCoroutine(click(true));
                    }
                    else
                    {
                        if (rotateClicked == false)
                            StartCoroutine(click(false));
                    }
                }
            }
            else
            {
                stuck = 0;
            }
        }
    }

    public void ResetCpTarget()
    {
        cpTarget = -1;
    }

    private void DisableRotation()
    {
        if (rotateClicked == false)
        {
            buttons.AIClickLeft(false);
            buttons.AIClickRight(false);
        }
    }

    private bool backwardClicked = false;

    private int ResetIndex(int inputIndex)
    {
        int startIndex = inputIndex;

        int currentDistIndex = distanceHitter.Index;

        for (int i = 0; i < line.Count; i++)
        {
            startIndex--;
            if (startIndex < 0)
            {
                startIndex = line.Count - 1;
            }

            int lowIndex = startIndex - 1;
            if (lowIndex < 0)
            {
                lowIndex = line.Count - 1;
            }

            if (currentDistIndex >= Points[lowIndex] && currentDistIndex <= Points[startIndex])
            {
                float currentDist = Vector2.Distance(gameobjectTransform.position, line[startIndex]);
                if (currentDist > distance)
                {
                    return startIndex;
                }
                else
                {
                    startIndex = startIndex + 1;
                    if (startIndex >= line.Count)
                    {
                        startIndex = 0;
                    }
                    return startIndex;
                }
            }
        }
        return startIndex;
    }

    IEnumerator backwardTimer()
    {
        yield return new WaitForSeconds(0.9f);
        backwardClicked = false;
        stuck = 0;
        resetIndex = false;
        yield return null;
    }

    IEnumerator click(bool side)
    {
        rotateClicked = true;
        if (side)
        {
            buttons.AIClickRight(true);
        }
        else
        {
            buttons.AIClickLeft(true);
        }

        yield return new WaitForSeconds(clickTime);

        if (side)
        {
            buttons.AIClickRight(false);
        }
        else
        {
            buttons.AIClickLeft(false);
        }

        rotateClicked = false;
        yield return null;
    }

    public int AiLevel
    {
        get
        {
            return aiLevel;
        }
        set
        {
            aiLevel = value;
        }

    }

    public void ResetIndex()
    {
        currentIndex = 0;
    }
}
