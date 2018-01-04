using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Buttons : MonoBehaviour
{
    private bool UpClicked = true;
    private bool DownClicked = false;
    private bool RightClicked = false;
    private bool LeftClicked = false;
    private bool NitroClicked = false;
    private bool OilClicked = false;
    private bool RocketClicked = false;
    private bool BombClicked = false;
    private bool FlameClicked = false;
    private bool LaserClicked = false;
    private bool IceClicked = false;

    [SerializeField]
    private Button Nitro;
    [SerializeField]
    private Button Oil;
    [SerializeField]
    private Button Bomb;
    [SerializeField]
    private Button Rocket;
    [SerializeField]
    private Button Flame;
    [SerializeField]
    private Button Laser;
    [SerializeField]
    private Button Ice;
    [SerializeField]
    private Text tapButton;
    private bool respawn = false;
    private float tapTime = 3f;

    void Start()
    {
        Nitro.gameObject.SetActive(false);
        Rocket.gameObject.SetActive(false);
        Oil.gameObject.SetActive(false);
        Bomb.gameObject.SetActive(false);
        Flame.gameObject.SetActive(false);
        Laser.gameObject.SetActive(false);
        Ice.gameObject.SetActive(false);
    }

    public void NitroOnClick()
    {
        NitroClicked = true;
    }

    public void OilOnClick()
    {
        OilClicked = true;
    }

    public void RocketOnClick()
    {
        RocketClicked = true;
    }

    public void BombOnClick()
    {
        BombClicked = true;
    }

    public void FlameOnClick()
    {
        FlameClicked = true;
    }

    public void LaserOnClick()
    {
        LaserClicked = true;
    }

    public void IceOnClick()
    {
        IceClicked = true;
    }

    public void DisableRocket()
    {
        RocketClicked = false;
        Rocket.gameObject.SetActive(false);
    }

    public void DisableNitro()
    {
        NitroClicked = false;
        Nitro.gameObject.SetActive(false);
    }

    public void DisableOil()
    {
        OilClicked = false;
        Oil.gameObject.SetActive(false);
    }

    public void DisableBomb()
    {
        BombClicked = false;
        Bomb.gameObject.SetActive(false);
    }

    public void DisableFlame()
    {
        FlameClicked = false;
        Flame.gameObject.SetActive(false);
    }

    public void DisableLaser()
    {
        LaserClicked = false;
        Laser.gameObject.SetActive(false);
    }

    public void DisableIce()
    {
        IceClicked = false;
        Ice.gameObject.SetActive(false);
    }

    public void EnableNitro()
    {
        Nitro.gameObject.SetActive(true);
    }

    public void EnableRocket()
    {
        Rocket.gameObject.SetActive(true);
    }

    public void EnableOil()
    {
        Oil.gameObject.SetActive(true);
    }

    public void EnableBomb()
    {
        Bomb.gameObject.SetActive(true);
    }

    public void EnableFlame()
    {
        Flame.gameObject.SetActive(true);
    }

    public void EnableLaser()
    {
        Laser.gameObject.SetActive(true);
    }

    public void EnableIce()
    {
        Ice.gameObject.SetActive(true);
    }

    public bool GetNitro()
    {
        return NitroClicked;
    }

    public bool GetRocket()
    {
        return RocketClicked;
    }

    public bool GetOil()
    {
        return OilClicked;
    }

    public bool GetBomb()
    {
        return BombClicked;
    }

    public bool GetFlame()
    {
        return FlameClicked;
    }

    public bool GetLaser()
    {
        return LaserClicked;
    }

    public bool GetIce()
    {
        return IceClicked;
    }

    public void UpOnClickDown()
    {
        UpClicked = true;
    }

    public void UpOnClickUp()
    {
        UpClicked = false;
    }

    public void DownOnClickDown()
    {
        DownClicked = true;
    }

    public void DownOnClickUp()
    {
        DownClicked = false;
    }

    public void RightOnClickDown()
    {
        RightClicked = true;
    }

    public void RightOnClickUp()
    {
        RightClicked = false;
    }

    public void LeftOnClickDown()
    {
        LeftClicked = true;
    }

    public void LeftOnClickUp()
    {
        LeftClicked = false;
    }

    public int Horizontal()
    {

        if (Input.GetAxis("Horizontal") != 0)
        {
            if (Input.GetAxis("Horizontal") > 0)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }
        else
 if (RightClicked && !LeftClicked)
            return 1;
        else
     if (LeftClicked && !RightClicked)
            return -1;
        else
            return 0;
    }

    public int Vertical()
    {
        /*  if (Input.GetAxis("Vertical") != 0)
          {
              if (Input.GetAxis("Vertical") > 0)
              {
                  return 1;
              }
              else
              {
                  return -1;
              }
          }
          else
              if (UpClicked)
              return 1;
          else
                  if (DownClicked)
              return -1;
          else
              return 0;*/
        if (RightClicked == true && LeftClicked == true)
            return -1;
        else
            return 1;
    }

    public bool RespawnClicked()
    {
        if (respawn)
        {
            respawn = false;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void OnRespawnClick()
    {
        respawn = true;
        DisableTapPanel();
    }

    public void EnableTapPanel()
    {
        tapButton.text = "TAP TO RESPAWN";
        tapButton.raycastTarget = true;
        StopAllCoroutines();
        StartCoroutine(TapTimer());
    }

    public void DisableTapPanel()
    {
        StopAllCoroutines();
        tapButton.text = "";
        tapButton.raycastTarget = false;
    }

    IEnumerator TapTimer()
    {
        yield return new WaitForSeconds(tapTime);
        DisableTapPanel();
        yield return null;
    }

    public void Reset()
    {
        DisableNitro();
        DisableRocket();
        DisableOil();
        DisableBomb();
        DisableFlame();
        DisableLaser();
        DisableIce();
        //UpClicked = false;
        DownClicked = false;
        RightClicked = false;
        LeftClicked = false;
    }
}
