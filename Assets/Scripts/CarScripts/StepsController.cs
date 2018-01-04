using UnityEngine;
using System.Collections;

public class StepsController : Photon.PunBehaviour
{
    GameObject line;
    GameObject line2;
    bool createLine = false;
    CarScript carScript;
    CarSkin carSkin;
    CarSound carSound;
    Vector3 oldB = Vector2.zero;
    Vector3 oldB2 = Vector2.zero;
    [SerializeField]
    private GameObject smoke;
    [SerializeField]
    private GameObject smoke2;
    private Transform stepsTransform;
    private Steps stepsCleaner;
    [SerializeField]
    private Transform wheelB;
    [SerializeField]
    private Transform wheelB2;
    [SerializeField]
    private GameObject linePrefab;
    private bool steps = false;
    private float smokeAngle = 40;
    private LineRenderer lineRender;
    private LineRenderer lineRender2;
    private ParticleSystem particle;
    private ParticleSystem particle2;
    private Transform smokeTransform;
    private Transform smokeTransform2;
    private bool smokeEnabled = false;
    private bool timerEnabled = false;
    private float currentTime = 0f;
    private ScoreManager scoreManager;

    void Start()
    {
        carScript = GetComponent<CarScript>();
        carSkin = GetComponent<CarSkin>();
        carSound = GetComponent<CarSound>();
        stepsTransform = FindObjectOfType<Steps>().gameObject.transform;
        stepsCleaner = FindObjectOfType<Steps>();
        scoreManager = FindObjectOfType<ScoreManager>();
        particle = smoke.GetComponent<ParticleSystem>();
        particle2 = smoke2.GetComponent<ParticleSystem>();
        smokeTransform = smoke.transform;
        smokeTransform2 = smoke2.transform;
    }

    private void DrawSteps(bool draw)
    {
        if (draw)
        {
            carSound.EnableSkidSound();
            if (oldB != Vector3.zero)
            {
                if (createLine && !carScript.Burnout)
                {
                    line = Instantiate(linePrefab);
                    line.transform.SetParent(stepsTransform, false);

                    line2 = Instantiate(linePrefab);
                    line2.transform.SetParent(stepsTransform, false);

                    lineRender = line.GetComponent<LineRenderer>();
                    lineRender2 = line2.GetComponent<LineRenderer>();
                    lineRender.sortingOrder = 2;
                    lineRender2.sortingOrder = 2;
                    stepsCleaner.IncSteps();
                }

                createLine = false;

                if (line != null && !carScript.Burnout)
                {
                    bool lineDrawed = false;
                    float length = (wheelB.position - oldB).magnitude;
                    if (length > 0.05f)
                    {
                        if (lineRender.positionCount == 0)
                        {
                            StartLine(lineRender, oldB, wheelB.position);
                            StartLine(lineRender2, oldB2, wheelB2.position);
                        }
                        else
                        {
                            DrawLine(lineRender, wheelB.position);
                            DrawLine(lineRender2, wheelB2.position);
                        }
                        lineDrawed = true;
                    }

                    if (lineDrawed)
                    {
                        EnableSmoke();
                    }

                    oldB = wheelB.position;
                    oldB2 = wheelB2.position;
                }
                else
                {
                    if (carScript.Burnout)
                    {
                        EnableSmoke();
                    }
                    else
                    {
                        DisableSmoke();
                    }

                    createLine = true;
                    oldB = wheelB.position;
                    oldB2 = wheelB2.position;
                }
            }
            else
            {
                oldB = wheelB.position;
                oldB2 = wheelB2.position;
            }
        }
        else
        {
            carSound.DisableSkidSound();
            createLine = true;
            oldB = wheelB.position;
            oldB2 = wheelB2.position;
            DisableSmoke();
        }
    }

    private void StartLine(LineRenderer lineRender, Vector2 startPos, Vector2 endPos)
    {
        lineRender.positionCount = 2;
        lineRender.SetPosition(0, startPos);
        lineRender.SetPosition(1, endPos);
    }

    private void DrawLine(LineRenderer lineRender, Vector2 pos)
    {
        lineRender.positionCount += 1;
        lineRender.SetPosition(lineRender.positionCount - 1, pos);
    }

    private void EnableSmoke()
    {
        Vector3 dir = wheelB.position - oldB;

        float length = (wheelB.position - oldB).magnitude;
        if (length > 0.05f)
        {
            float direction = Quaternion.FromToRotation(Vector2.right, wheelB.position - oldB).eulerAngles.z;
            float direction2 = Quaternion.FromToRotation(Vector2.right, wheelB2.position - oldB2).eulerAngles.z;
            smokeTransform.eulerAngles = new Vector3(direction, -90, 90);
            smokeTransform2.eulerAngles = new Vector3(direction2, -90, 90);
        }
        else
        {
            if (carScript.Input > 0)
            {
                smokeTransform.localEulerAngles = new Vector3(0, -90, 90);
                smokeTransform2.localEulerAngles = new Vector3(0, -90, 90);
            }
            else if (carScript.Input < 0)
            {
                smokeTransform.localEulerAngles = new Vector3(180, -90, 90);
                smokeTransform2.localEulerAngles = new Vector3(180, -90, 90);
            }
        }

        float angle = Vector2.Angle(carScript.ForwardVelocity().normalized, dir);

        if (angle > 90)
            angle = angle - 90;

        if (angle > smokeAngle || carScript.Burnout)
        {
            if (smokeEnabled == false)
            {
                smokeEnabled = true;
                particle.Play();
                particle2.Play();

                if (angle > smokeAngle && photonView.isMine && !carScript.AI)
                {
                    if (timerEnabled == false)
                    {
                        timerEnabled = true;
                        StartCoroutine(AddTimer());
                    }
                }
            }
        }
        else
        {
            DisableSmoke();
        }
    }

    IEnumerator AddTimer()
    {
        while (timerEnabled == true)
        {
            currentTime += Time.deltaTime;
            yield return null;
        }
        scoreManager.AddDriftScore(currentTime);
        currentTime = 0;
    }

    public void DisableSmoke()
    {
        if (smokeEnabled == true)
        {
            smokeEnabled = false;
            if (photonView.isMine && !carScript.AI)
            {
                timerEnabled = false;
            }
            particle.Stop();
            particle2.Stop();
        }
    }

    private void Skid()
    {
        if (photonView.isMine)
        {
            int forth = carScript.Forth;
            float forwardVelocity = carScript.ForwardVel;
            float rightVelocity = carScript.RightVel;

            if (((((rightVelocity > 12f) && ((forwardVelocity > 5f)))
                || ((rightVelocity > 6f) && (forwardVelocity > 5f) && (forwardVelocity < carScript.MaxSpeed / 2)) && forth == 1) ||
                carScript.Burnout) && (!carSkin.CarHided()))
            {
                steps = true;
                DrawSteps(true);
            }
            else
            {
                steps = false;
                DrawSteps(false);
            }
        }
        else
        {
            DrawSteps(steps);
        }
    }

    // Update is called once per frame
    void Update()
    {
        Skid();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(steps);
        }
        else
        {
            steps = (bool)stream.ReceiveNext();
        }
    }
}
