using UnityEngine;
using UnityEngine.UI;

public class ClickSound : MonoBehaviour
{
    public AudioClip sound;
    private Button button { get { return GetComponent<Button>(); } }

    // Use this for initialization
    void Awake()
    {
        button.onClick.AddListener(() => PlaySound());
    }

    void PlaySound()
    {
        if (Prototype.NetworkLobby.SettingsController.GetSound())
        {
            var g = new GameObject();
            g.AddComponent<AudioSource>();
            g.name = sound.name;
            AudioSource source = g.GetComponent<AudioSource>();
            source.clip = sound;
            source.playOnAwake = false;
            source.PlayOneShot(sound);

            DestroyObject(g, source.clip.length);
        }
    }
}
