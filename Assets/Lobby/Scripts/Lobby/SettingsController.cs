using UnityEngine;
using UnityEngine.UI;

namespace Prototype.NetworkLobby
{
    public class SettingsController : Photon.MonoBehaviour
    {
        [SerializeField]
        private Image sound;
        [SerializeField]
        private Image music;
        [SerializeField]
        private Sprite on;
        [SerializeField]
        private Sprite off;
        [SerializeField]
        private GameObject settings;
        [SerializeField]
        private bool lobbyGame = false;

        private static bool SoundValue = true;
        private static bool MusicValue = true;

        public static void InitSound()
        {
            if (PlayerPrefs.HasKey("Sound"))
            {
                if (PlayerPrefs.GetInt("Sound") == 1)
                {
                    SoundValue = true;
                }
                else
                {
                    DisableSound();
                }

                if (PlayerPrefs.GetInt("Music") == 1)
                {
                    MusicValue = true;
                }
                else
                {
                    DisableMusic();
                }
            }
            else
            {
                SoundValue = true;
                MusicValue = true;
                PlayerPrefs.SetInt("Sound", 1);
                PlayerPrefs.SetInt("Music", 1);
            }
        }

        void OnEnable()
        {
            if (SoundValue)
                sound.overrideSprite = on;
            else
                sound.overrideSprite = off;

            if (MusicValue)
                music.overrideSprite = on;
            else
                music.overrideSprite = off;
        }

        public void OnSoundClick()
        {
            if (SoundValue)
            {
                sound.overrideSprite = off;
                DisableSound();
            }
            else
            {
                sound.overrideSprite = on;
                EnableSound(lobbyGame);
            }
        }

        public void OnMusicClick()
        {
            if (MusicValue)
            {
                music.overrideSprite = off;
                DisableMusic();
            }
            else
            {
                music.overrideSprite = on;
                EnableMusic();
            }
        }

        public void OnCloseClick()
        {
            settings.SetActive(false);
        }

        public static void EnableSound(bool lobbyGame)
        {
            SoundValue = true;
            PlayerPrefs.SetInt("Sound", 1);

            if (PhotonNetwork.offlineMode == false || lobbyGame == true)
                AudioListener.pause = false;
        }

        public static void DisableSound()
        {
            SoundValue = false;
            PlayerPrefs.SetInt("Sound", 0);
            AudioListener.pause = true;
        }

        public static void EnableMusic()
        {
            MusicValue = true;
            PlayerPrefs.SetInt("Music", 1);

            if (FlexibleMusicManager.instance != null)
            {
                if (!FlexibleMusicManager.instance.IsPlaying())
                    FlexibleMusicManager.instance.Play();
            }
        }

        public static void DisableMusic()
        {
            MusicValue = false;
            PlayerPrefs.SetInt("Music", 0);

            if (FlexibleMusicManager.instance != null)
            {
                FlexibleMusicManager.instance.Pause();
            }
        }

        public static bool GetSound()
        {
            return SoundValue;
        }

        public static bool GetMusic()
        {
            return MusicValue;
        }

        public void OnAdsClick()
        {
            IAPManager.Instance.BuyNoAds();
        }
    }
}