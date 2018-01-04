using System.Collections;
using UnityEngine.Advertisements;
using UnityEngine;

namespace Prototype.NetworkLobby
{
    public class AdsController : MonoBehaviour
    {
        [SerializeField]
        private GameObject noAdsButton;
        [SerializeField]
        private Message message;
        [SerializeField]
        private bool inGame = false;

        private void Start()
        {
            if (inGame)
            {
                InitAds();
            }
            else
                State(StateManager.State);
        }

        private void State(int state)
        {
            if (state >= 1)
            {
                if (Ads.noAds == true)
                {
                    DisableAdsButton();
                }
            }
            else
            {
                if (Ads.firstLauch == true)
                {
                    Ads.firstLauch = false;
                    StartCoroutine(WaitForInit());
                }
            }
        }

        private IEnumerator WaitForInit()
        {
            while (!Ads.init)
            {
                yield return null;
            }

            InitAds();

            if (Ads.noAds == false)
            {
                StartCoroutine(ShowAds());
            }
            else
            {
                DisableAdsButton();
            }
            yield return null;
        }

        private IEnumerator ShowAds()
        {
            while (!Advertisement.IsReady())
            {
                yield return null;
            }

            Advertisement.Show();

            EnableAdsButton();
        }

        public void EnableAdsButton()
        {
            if (noAdsButton != null)
                noAdsButton.SetActive(true);
        }

        public void DisableAdsButton()
        {
            noAdsButton.SetActive(false);
        }

        public void OnRewardClick()
        {
            StartCoroutine(ShowRewardAds());
        }

        private IEnumerator ShowRewardAds()
        {
            while (!Advertisement.IsReady(Ads.rewardVideoID))
            {
                yield return null;
            }

            Advertisement.Show(Ads.rewardVideoID, new ShowOptions
            {
                resultCallback = result =>
                {
                    if (result == ShowResult.Finished)
                    {
                        string textMessage = "CONGRATULATIONS! YOU EARNED 1500";
                        message.ShowMessage(textMessage, null, true, false);
                        Purse.AddCredit(1500);
                        SaveData.Save();
                    }
                    else if (result == ShowResult.Failed)
                    {
                        string textMessage = "ADS CURRENTLY UNAVAILABLE";
                        message.ShowMessage(textMessage, null, false, false);
                    }
                }
            }
            );
        }

        public void InitAds()
        {
            if (Prototype.NetworkLobby.Ads.noAds == false)
            {
#if UNITY_ANDROID || UNITY_EDITOR
                if (Advertisement.isSupported) Advertisement.Initialize(Prototype.NetworkLobby.Ads.androidAdsID, false);
#elif UNITY_IPHONE || UNITY_IPAD
                   if (Advertisement.isSupported) Advertisement.Initialize(Prototype.NetworkLobby.Ads.iosAdsID, true);
#endif
            }
        }

        public void AdsLaunch()
        {
            if (Prototype.NetworkLobby.Ads.noAds == false)
            {
                int num = Random.Range(0, 4);

                if (num == 0)
                    StartCoroutine(ShowAds());
            }
        }
    }
}
