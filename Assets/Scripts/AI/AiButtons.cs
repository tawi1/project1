using UnityEngine;

public class AiButtons : MonoBehaviour
{
    private bool UpClicked = false;
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

    bool respawn = false;

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

    public void ClickOil(bool state)
    {
        if (state)
            OilClicked = true;
        else
            OilClicked = false;
    }

    public void ClickRocket(bool state)
    {
        if (state)
            RocketClicked = true;
        else
            RocketClicked = false;
    }

    public void ClickNitro(bool state)
    {
        if (state)
            NitroClicked = true;
        else
            NitroClicked = false;
    }

    public void ClickBomb(bool state)
    {
        if (state)
            BombClicked = true;
        else
            BombClicked = false;
    }

    public void ClickFlame(bool state)
    {
        if (state)
            FlameClicked = true;
        else
            FlameClicked = false;
    }

    public void ClickLaser(bool state)
    {
        if (state)
            LaserClicked = true;
        else
            LaserClicked = false;
    }

    public void ClickIce(bool state)
    {
        if (state)
            IceClicked = true;
        else
            IceClicked = false;
    }

    public void AIClickRight(bool state)
    {
        if (state)
            RightClicked = true;
        else
            RightClicked = false;
    }

    public void AIClickLeft(bool state)
    {
        if (state)
            LeftClicked = true;
        else
            LeftClicked = false;
    }

    public void AIClickUp(bool state)
    {
        if (state)
            UpClicked = true;
        else
            UpClicked = false;
    }

    public void AIClickDown(bool state)
    {
        if (state)
            DownClicked = true;
        else
            DownClicked = false;
    }

    public int Horizontal()
    {
        if (RightClicked)
            return 1;
        else
            if (LeftClicked)
            return -1;
        else
            return 0;
    }

    public int Vertical()
    {
        if (UpClicked)
            return 1;
        else
            if (DownClicked)
            return -1;
        else
            return 0;
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
    }
}
