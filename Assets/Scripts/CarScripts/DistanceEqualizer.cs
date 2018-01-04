using UnityEngine;
[RequireComponent(typeof(AudioLowPassFilter))]
[RequireComponent(typeof(AudioSource))]
public class DistanceEqualizer : Photon.MonoBehaviour
{
    [SerializeField]
    private float StartCutoffFrom = 10f;
    [Range(0.01f, 1f)]
    [SerializeField]
    private float DistanceCutoff = 0.3f;

    float distance;
    float distcut;

    AudioListener AudioListener;
    AudioSource AudioSource;
    AudioLowPassFilter audioFilter;

    // Use this for initialization
    void Start()
    {
        AudioListener = FindObjectOfType<AudioListener>();
        AudioSource = gameObject.GetComponent<AudioSource>();
        gameObject.GetComponent<AudioLowPassFilter>().enabled = true;
        audioFilter = gameObject.GetComponent<AudioLowPassFilter>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (AudioSource.isPlaying)
        {
            distance = Vector3.Distance(transform.position, AudioListener.transform.position);
            //Lowpass filter cutoff frequency          
            distcut = 22000 * Mathf.Pow(0.6f, (distance - StartCutoffFrom) * Mathf.Pow(DistanceCutoff * 10, 3) / 1000);
            audioFilter.cutoffFrequency = distcut;
        }
    }
}
