using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(Bonus))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CarCheckpoint))]
public class CarScript : Photon.PunBehaviour
{
    [SerializeField]
    private ParticleSystem engine;
    [SerializeField]
    private CarCheckpoint carCheckpont;
    [SerializeField]
    private CarSkin carSkin;
    [SerializeField]
    private CarSound carSound;

    float maxSpeedForce = 15000f;//25500f;//22500f;
    float maxSpeed = 19f;
    //   float maxSpeed = 33f;
    float torqueForce = 12500f;

    float driftFactorSticky = 0.95f;
    float driftFactorSlippy = 1;
    float maxStickyVelocity = 15f;
    //float minStickyVelocity = 5f;
    float minDriftVelocity = 5f;
    float rotateAngle = 40f;
    float currentAngle = 0f;
    //float minSlippyVelocity = 1.5f;	// <--- Exercise for the viewer
    bool drift = false;
    private Rigidbody2D rb;
    private Buttons buttons;
    private AiButtons aiButtons;
    float nitroMult = 1.35f;
    private bool control = false;
    private float nitro = 1;

    [SerializeField]
    private Transform wheelB;
    [SerializeField]
    private Transform wheelB2;
    [SerializeField]
    private Transform wheelF;
    [SerializeField]
    private Transform wheelF2;

    private Wheel wheelBW;
    private Wheel wheelBW2;
    private Wheel wheelFW;
    private Wheel wheelFW2;

    [SerializeField]
    private bool ai = false;
    [SerializeField]
    private int localId = -1;

    float acTime = 3.94f;
    [SerializeField]
    float currentRpm = 0f;
    bool burnout = false;
    bool burnoutAllowed = true;
    private float gasInput;

    private int forth = 1;
    private float forwardVelocity = 0;
    private float rightVelocity = 0;
    private float velocity;
    private bool rpm = true;
    private PathFollower pathFollower;
    private int aiLevel = 0;
    [SerializeField]
    FlameController flameController;
    private bool fragile = false;
    private Bonus carBonus;
    private Transform carTransform;
    CapsuleCollider2D capsuleCollider2D;
    private bool isColliding = false;
    private ScoreManager scoreManager;
    private bool finished = false;
    private MatchController matchController;

    [SerializeField]
    private DistanceHitter distanceHitter;

    private void Awake()
    {
        //ai = true;
        rb = GetComponent<Rigidbody2D>();
        carBonus = GetComponent<Bonus>();
        distanceHitter = GetComponent<DistanceHitter>();
        carTransform = gameObject.transform;

        wheelBW = wheelB.GetComponent<Wheel>();
        wheelBW2 = wheelB2.GetComponent<Wheel>();
        wheelFW = wheelF.GetComponent<Wheel>();
        wheelFW2 = wheelF2.GetComponent<Wheel>();

        capsuleCollider2D = GetComponent<CapsuleCollider2D>();

        scoreManager = FindObjectOfType<ScoreManager>();
        matchController = FindObjectOfType<MatchController>();

        if (matchController.LobbyGame)
        {
            ai = true;
            carSound.DeleteSounds();

            GetComponent<PhotonView>().ObservedComponents.Clear();
        }
    }

    [PunRPC]
    public void SetupPlayer(string nick, int id)
    {
        if (!matchController.LobbyGame)
        {
            GetComponentInChildren<TextMesh>().text = nick;
        }
        else
        {
            Destroy(GetComponentInChildren<TextMesh>().gameObject);
        }

        if (gameObject.GetPhotonView().isMine && !ai)
        {
            FindObjectOfType<DistanceManager>().ID = id;
            carSound.DestroyAudioFilter();
        }
    }

    [PunRPC]
    public void SetLocalId(int inputId)
    {
        localId = inputId;
        flameController.LocalId = inputId;
    }

    public int LocalId
    {
        get
        {
            return localId;
        }
    }

    private void NitroSelector()
    {
        if (carBonus.NitroState == true)
        {
            EnableNitro();
        }
        else
        {
            DisableNitro();
        }
    }

    public static Buttons ButtonsInit()
    {
        Buttons d = GameObject.FindObjectOfType<Buttons>();
        if (d != null)
        {
            return d;
        }
        else
            return null;
    }

    void FixedUpdate()
    {
        NitroSelector();
        isColliding = false;

        if (photonView.isMine || (matchController.LobbyGame && matchController.GameEnabled))
            if (!buttons && matchController.LobbyGame == false)
            {
                buttons = ButtonsInit();
            }
            else
            {
                if (ai == false)
                {
                    if (buttons.RespawnClicked() && buttons.gameObject.activeSelf)
                    {
                        carCheckpont.Respawn();
                        return;
                    }
                }
                else
                {
                    if (aiButtons == null)
                    {
                        AddAiModule();
                        aiButtons = gameObject.GetComponent<AiButtons>();
                    }

                    if (aiButtons)
                        if (aiButtons.RespawnClicked())
                        {
                            carCheckpont.Respawn();
                            return;
                        }
                }

                if (rb.velocity.magnitude > 7f)
                    if (ai == false)
                        buttons.DisableTapPanel();

                SetDrift();
                GasCar();
                RotateCar();
            }
    }

    private void SetDrift()
    {
        float driftFactor = driftFactorSticky;
        if (drift == false)
        {
            if ((ForwardVelocity().magnitude > maxStickyVelocity) && (RightVelocity().magnitude > minDriftVelocity))
            {
                driftFactor = driftFactorSlippy;
                drift = true;
            }
        }
        else
        {
            if (RightVelocity().magnitude < minDriftVelocity)
            {
                drift = false;
            }
            else
            {
                driftFactor = driftFactorSlippy;
            }
        }

        Vector2 forw = ForwardVelocity();
        forwardVelocity = forw.magnitude;

        Vector2 right = RightVelocity();
        rightVelocity = right.magnitude;

        rb.velocity = forw + right * driftFactor;
        velocity = rb.velocity.magnitude;
    }

    private void IncRPM(float input)
    {
        if (rpm)
        {
            if ((input != 0))
            {
                float maxRPM = 0;
                if (input > 0)
                {
                    if (rb.velocity.magnitude > maxSpeed / 2)
                        maxRPM = maxSpeed;
                    else
                        maxRPM = maxSpeed / 2;
                }
                else
                {
                    maxRPM = maxSpeed / 2;
                }

                forth = GetDirection();

                int side = -1;

                if ((input > 0 && forth > 0) || (input < 0 && forth < 0) || ((input < 0) && rb.velocity.magnitude < 1))
                {
                    side = 1;
                }

                if ((ForwardVel < 0.5f && currentRpm > 3f) || delayBurnout)
                {
                    if (burnoutAllowed)
                        burnout = true;
                }
                else
                {
                    burnout = false;
                }

                CheckBurnoutDrift();

                ChangeRpm(side, maxRPM);
            }
            else
            {
                if (currentRpm > 0)
                {
                    if (currentRpm < maxSpeed / 3.2f)
                    {
                        burnout = false;
                    }
                    currentRpm -= maxSpeed * 2 / (acTime * (1 / Time.fixedDeltaTime));
                }
                else
                {
                    currentRpm = 0;
                    burnout = false;
                }
            }
        }
    }

    private void ChangeRpm(int side, float maxRPM)
    {
        if (Mathf.Abs(currentRpm) < maxRPM)
        {
            if (currentRpm >= 0)
            {
                if (side > 0)
                {
                    currentRpm += maxSpeed / (acTime * (1 / Time.fixedDeltaTime));
                }
                else
                {
                    currentRpm -= 3 * maxSpeed / (acTime * (1 / Time.fixedDeltaTime));
                }
            }
            else
                currentRpm = 0;
        }
        else
        {
            if (maxRPM - currentRpm < 1.5f)
            {
                currentRpm -= 1.5f;
            }
            else
                currentRpm -= maxSpeed * 3 / (acTime * (1 / Time.fixedDeltaTime));
        }
    }

    private void CheckBurnoutDrift()
    {
        if (currentRpm > 5f && !control)
        {
            if (burnoutAllowed)
                BurnoutDrift();
        }
        else
        {
            if (currentRpm > 15f)
                if (startBurnAngle != 0)
                {
                    StartCoroutine(DelayBurnout());
                }

            startBurnAngle = 0;
        }
    }

    float startBurnAngle = 0;
    bool burnRotate = false;

    private void BurnoutDrift()
    {
        int burnoutLimit = 4;
        if (startBurnAngle == 0)
        {
            startBurnAngle = carTransform.rotation.eulerAngles.z + Random.Range(-5, 6);
            burnRotate = false;
        }
        else
        {
            float burnf = 2500f;

            if (!burnRotate)
            {
                if (carTransform.rotation.eulerAngles.z < (startBurnAngle - burnoutLimit))
                    burnRotate = true;
            }
            else
            {
                if (carTransform.rotation.eulerAngles.z > (startBurnAngle + burnoutLimit))
                    burnRotate = false;
            }

            int side = 1;
            if (burnRotate)
                side = -1;

            Vector2 force = carTransform.up * side * burnf;

            rb.AddForceAtPosition(force, wheelB.position);
            rb.AddForceAtPosition(force, wheelB2.position);

            Vector2 force2 = carTransform.right * burnf / 25;
            rb.AddForceAtPosition(force2, wheelB.position);
            rb.AddForceAtPosition(force2, wheelB2.position);
        }
    }

    bool delayBurnout = false;
    private IEnumerator DelayBurnout()
    {
        delayBurnout = true;
        yield return StartCoroutine(BurnoutTimer(0.7f));
        delayBurnout = false;
    }

    IEnumerator BurnoutTimer(float time)
    {
        float remainingTime = time;

        while (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;
            if (currentRpm < 15)
                break;
            yield return null;
        }
    }

    private void GasCar()
    {
        float input = 0;

        if (ai == false)
            input = buttons.Vertical();
        else if (aiButtons)
            input = aiButtons.Vertical();

        gasInput = input;
        bool nitroState = carBonus.NitroState;

        IncRPM(input);

        if (((input != 0) || (nitroState)) && (control))
        {
            float currentForce = 0;

            if (input > 0 || nitroState)
            {
                if (!nitroState)
                    currentForce = maxSpeedForce;
                else
                    currentForce = maxSpeedForce * nitro;
            }
            else
            {
                currentForce = -maxSpeedForce / 2;
            }

            float oil = wheelBW.GetOil();
            Vector2 force = carTransform.right * currentForce * nitro * oil;

            rb.AddForceAtPosition(force, wheelB.position);

            oil = wheelBW2.GetOil();
            force = carTransform.right * currentForce * nitro * oil;

            rb.AddForceAtPosition(force, wheelB2.position);
        }
    }

    private void RotateCar()
    {
        float input = 0;

        if (ai == false)
            input = buttons.Horizontal();
        else if (aiButtons)
            input = aiButtons.Horizontal();

        if (input != 0 && control)
        {
            RotateWheel(input);
            wheelF.localEulerAngles = new Vector3(0, 0, -currentAngle);
            wheelF2.localEulerAngles = new Vector3(0, 0, -currentAngle);

            float tf = Mathf.Lerp(0, torqueForce, ForwardVelocity().magnitude / 10);
            float oil = wheelFW.GetOil();

            Vector2 force = carTransform.up * -input * forth * oil * tf;

            rb.AddForceAtPosition(force, wheelF.position);

            oil = wheelFW2.GetOil();

            force = carTransform.up * -input * forth * oil * tf;

            rb.AddForceAtPosition(force, wheelF2.position);
        }
        else
        {
            RotateWheel(0);
            wheelF.localEulerAngles = new Vector3(0, 0, -currentAngle);
            wheelF2.localEulerAngles = new Vector3(0, 0, -currentAngle);
        }
    }

    private void RotateWheel(float right)
    {
        float rotateTime = 150f;

        float speed = rotateAngle / (1 / Time.fixedDeltaTime) / (rotateTime / 1000);

        if (right != 0f)
        {
            if ((currentAngle < 0 && right == 1) || (currentAngle > 0 && right == -1) || (Mathf.Abs(currentAngle) < rotateAngle))
            {
                currentAngle += speed * right;
            }
        }
        else
        {
            if (Mathf.Abs(currentAngle) > 0.5f)
            {
                if (currentAngle > 0)
                {
                    currentAngle -= speed;
                }
                else
                {
                    currentAngle += speed;
                }
            }
            else
            {
                currentAngle = 0;
            }
        }
    }

    #region ShortProcs
    public int GetDirection()
    {
        int forth = -1;
        if (rb.velocity.magnitude > 1)
        {
            float dot = Vector2.Dot(GetCentralAxis(), rb.velocity);

            if (dot >= 0)
                forth = 1;
        }
        else
        {
            forth = 1;
        }

        return forth;
    }

    public Vector2 ForwardVelocity()
    {
        return carTransform.right * Vector2.Dot(rb.velocity, carTransform.right);
    }

    public Vector2 RightVelocity()
    {
        return carTransform.up * Vector2.Dot(rb.velocity, carTransform.up);
    }

    public void EnableNitro()
    {
        nitro = nitroMult;

        if (engine != null)
        {
            if (engine.isPlaying == false)
                engine.Play();

            carSound.EnableNitro();
        }
    }

    public void DisableNitro()
    {
        nitro = 1;
        if (engine != null)
            if (engine.isPlaying)
            {
                engine.Stop();
                engine.Clear();

                carSound.DisableNitro();
            }
    }

    void OnTriggerEnter2D(Collider2D c)
    {
        if (isColliding) return;
        isColliding = true;

        if (photonView.isMine)
        {
            if (c.tag == "Area")
            {
                string objName = c.name;
                if (objName == "Fire")
                {
                    if (carSkin.GetModelRender().sortingOrder > 0)
                    {
                        int id = 0;

                        ExplosionController exp = c.gameObject.GetComponentInParent<ExplosionController>();
                        id = exp.OwnerId;

                        gameObject.GetComponent<Health>().Destroyed(id);
                    }
                }
                else
                if (objName.Contains("IceArea"))
                {
                    int id = c.GetComponent<IceAreaController>().OwnerId;
                    gameObject.GetComponent<Frozen>().Freeze(id);
                }
                else
                if (objName.Contains("ActivatedBomb"))
                {
                    if (!carSkin.CarHided())
                    {
                        c.gameObject.GetComponent<BombController>().DestroyBomb();
                    }
                }
            }
        }
    }

    private void OnTriggerStay2D(Collider2D c)
    {
        if (c.tag == "checkpoint")
        {
            carCheckpont.OnCheckpointEnter(c.gameObject.GetComponent<CheckPoint>().Index);
        }

        if (c.tag == "Finish")
        {
            carCheckpont.OnFinishEnter();
        }
    }

    public void ResetCar()
    {
        wheelBW.DisableOil();
        wheelBW2.DisableOil();
        wheelFW.DisableOil();
        wheelFW2.DisableOil();
        if (buttons != null)
        {
            buttons.DisableTapPanel();
            if (!buttons.gameObject.activeSelf)
                buttons.gameObject.SetActive(true);
        }
    }

    public void DisableButtons()
    {
        if (buttons != null && ai == false)
            buttons.DisableTapPanel();
    }

    private bool lockStuck = false;
    private int breakPower = 10000;

    void OnCollisionEnter2D(Collision2D c)
    {
        string tag = c.gameObject.tag;
        if (tag == "Barrier")
        {
            Vector3 pos = carTransform.position;

            Vector3 leftSide = pos + carTransform.TransformDirection(new Vector2(0, -capsuleCollider2D.size.y / 2));

            float totalResult = 0;

            totalResult = Vector2.Angle(leftSide - pos, rb.velocity.normalized);

            if (totalResult > 90)
                totalResult = 180 - totalResult;

            if (totalResult < 84f)
            {
                carSound.InitHitSound(c.relativeVelocity.magnitude);
            }
        }
        else
        if (tag == "Player")
        {
            if (photonView.isMine)
            {
                carSound.InitHitSound(c.relativeVelocity.magnitude / 2);
                if (fragile)
                {
                    if (Vector3.Dot(c.contacts[0].normal, c.relativeVelocity) * rb.mass > breakPower)
                    {
                        gameObject.GetComponent<Health>().Destroyed(c.gameObject.GetPhotonView().ownerId);
                        carSound.CarBreak();
                    }
                }
            }
            else
            {
                if (fragile)
                {
                    if (c.gameObject.GetPhotonView().isMine)
                        if (Vector3.Dot(c.contacts[0].normal, c.relativeVelocity) * rb.mass > breakPower)
                        {
                            gameObject.GetComponent<Health>().Destroyed(c.gameObject.GetPhotonView().ownerId);
                            carSound.CarBreak();
                        }
                }
            }
        }

        if (tag == "Area")
        {
            if (photonView.isMine)
            {
                gameObject.GetComponent<Health>().Destroyed(c.gameObject.GetPhotonView().ownerId);
            }
        }
    }

    void OnCollisionStay2D(Collision2D c)
    {
        if (c.gameObject.tag == "Barrier")
        {
            if (photonView.isMine)
            {
                if (control == true)
                    if (rb.velocity.magnitude < 5f)
                        if (ai == true)
                        {
                            if (pathFollower == null)
                            {
                                AddAiModule();
                            }

                            if (pathFollower.AiLevel == 0)
                            {
                                aiButtons.OnRespawnClick();
                            }
                            else
                            {
                                if (rb.velocity.magnitude < 2.5f)
                                {
                                    if (lockStuck == false)
                                    {
                                        int num = Random.Range(0, 11);
                                        int numLimit = 5;

                                        if (pathFollower.AiLevel == 2)
                                            numLimit = 3;

                                        if (num < numLimit)
                                        {
                                            aiButtons.OnRespawnClick();
                                        }
                                        else
                                        {

                                            lockStuck = true;
                                            if (gasInput > 0)
                                                pathFollower.Stuck = 1;
                                            else
                                            {
                                                pathFollower.Stuck = -1;
                                            }
                                            StartCoroutine(AddTimerStuck());
                                        }
                                    }
                                }
                            }
                        }
                        else if (buttons.gameObject.activeSelf)
                        {
                            buttons.EnableTapPanel();
                        }
            }
        }
    }

    IEnumerator AddTimerStuck()
    {
        yield return new WaitForSeconds(0.6f);
        lockStuck = false;
        yield return null;
    }

    public bool Control
    {
        get
        {
            return control;
        }
        set
        {
            control = value;
        }
    }

    [PunRPC]
    public void SetAI(int newAiLevel)
    {
        scoreManager.AddAi(newAiLevel);

        aiLevel = newAiLevel;
        AddAiModule();
        ai = true;
    }

    public void AddAiModule()
    {
        PathFollower path = GetComponent<PathFollower>();
        pathFollower = path;
        if (path == null)
        {
            gameObject.AddComponent<AiButtons>();
            gameObject.AddComponent<PathFollower>();
        }
    }

    public bool AI
    {
        get
        {
            return ai;
        }
        set
        {
            ai = value;
        }
    }

    public Vector2 GetBackAxis()
    {
        return wheelB.position - (wheelB.position + wheelB2.position) / 2;
    }

    public Vector2 GetCentralAxis()
    {
        return (wheelF.position + wheelF2.position) / 2 - (wheelB.position + wheelB2.position) / 2;
    }

    public Vector3 GetWheelB()
    {
        return wheelB.position;
    }

    public Vector3 GetWheelB2()
    {
        return wheelB2.position;
    }

    public Vector2 GetWheelBAxis()
    {
        return (wheelB.position + wheelB2.position) / 2 - wheelB.position;
    }

    public Vector2 GetWheelB2Axis()
    {
        return (wheelB.position + wheelB2.position) / 2 - wheelB2.position;
    }

    public Vector3 GetAxisPoint()
    {
        return (wheelB.position + wheelB2.position) / 2;
    }

    public Vector3 GetAxisFPoint()
    {
        return (wheelF.position + wheelF2.position) / 2;
    }

    public float MaxSpeed
    {
        get
        {
            return maxSpeed;
        }
    }

    public bool Burnout
    {
        get
        {
            return burnout;
        }
    }

    public float Input
    {
        get
        {
            return gasInput;
        }
    }

    public bool BurnoutAllowed
    {
        get
        {
            return burnoutAllowed;
        }
        set
        {
            burnoutAllowed = value;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(currentRpm);
            stream.SendNext(burnout);
        }
        else
        {
            currentRpm = (float)stream.ReceiveNext();
            burnout = (bool)stream.ReceiveNext();
        }
    }

    public int Forth
    {
        get
        {
            return forth;
        }
    }

    public float ForwardVel
    {
        get
        {
            return forwardVelocity;
        }
    }

    public float RightVel
    {
        get
        {
            return rightVelocity;
        }
    }

    public float Velocity
    {
        get
        {
            return velocity;
        }
    }

    public int AiLevel
    {
        get
        {
            return aiLevel;
        }
    }

    public void EnableRpm()
    {
        rpm = true;
    }

    public void DisableRpm()
    {
        rpm = false;
        currentRpm = 0;
    }

    public float CurrentRpm
    {
        get
        {
            return currentRpm;
        }
    }

    public bool Fragile
    {
        get
        {
            return fragile;
        }
        set
        {
            fragile = value;
        }
    }

    public string NickName
    {
        get
        {
            return GetComponentInChildren<TextMesh>().text;
        }
    }

    public bool Finished
    {
        get
        {
            return finished;
        }
        set
        {
            finished = value;
        }
    }

    public PathFollower PathFollower
    {
        get
        {
            return pathFollower;
        }
    }
    #endregion
}



