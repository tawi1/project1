using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;


public class ShareAndRate : MonoBehaviour
{
    string subject = "Blast Cars Racing score";
    string body = "I'm get ";
    string bodyStart = "I'm get ";
    static string androidID = "com.Spirit604.BlastCars";
    static string iphoneID = "";
    [SerializeField]
    private ScoreManager scoreManager;

    void Start()
    {
        scoreManager = FindObjectOfType<ScoreManager>();
    }

#if UNITY_IPHONE || UNITY_IPAD
	[DllImport("__Internal")]
	private static extern void sampleMethod (string iosPath, string message);
#endif

    public void OnShareClick()
    {
        body = bodyStart;
        body += scoreManager.Score.ToString() + " score";
#if UNITY_ANDROID
        shareScreenshot();

#elif UNITY_IPHONE || UNITY_IPAD
        OniOSTextSharingClick();
#endif
    }

    private bool isProcessing = false;

    public void shareScreenshot()
    {
        if (!isProcessing)
            StartCoroutine(captureScreenshot());
    }

    public IEnumerator captureScreenshot()
    {
        isProcessing = true;
        yield return new WaitForEndOfFrame();

        //byte[] dataToSave = Resources.Load<TextAsset>("everton").bytes;
        byte[] dataToSave = MakeScreen().EncodeToPNG();

        string destination = Path.Combine(Application.persistentDataPath, System.DateTime.Now.ToString("yyyy-MM-dd-HHmmss") + ".png");

        File.WriteAllBytes(destination, dataToSave);

        if (!Application.isEditor)
        {
            // block to open the file and share it ------------START
            AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
            AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");
            intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
            AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
            AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("parse", "file://" + destination);
            intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_STREAM"), uriObject);
            intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), body + "\n" +
                                                 "Download the game on play store at " + "\nhttps://play.google.com/store/apps/details?id=" + androidID);
            //intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_SUBJECT"), "Which club is this?");
            intentObject.Call<AndroidJavaObject>("setType", "image/jpeg");
            AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");

            // option one WITHOUT chooser:
            currentActivity.Call("startActivity", intentObject);

            // block to open the file and share it ------------END

        }
        isProcessing = false;
    }

    public void OniOSTextSharingClick()
    {
#if UNITY_IPHONE || UNITY_IPAD
		byte[] bytes = MakeScreen().EncodeToPNG();
string path = Application.persistentDataPath + "/MyImage.png";
File.WriteAllBytes(path, bytes);
string path_ = "MyImage.png";
string shareMessage = body + "\n" +"Download the game on apple store at " + "\nhttps://itunes.apple.com/app/=" + iphoneID;
sampleMethod (path, shareMessage);
		
#endif
    }

    private Texture2D MakeScreen()
    {
        Texture2D screenTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, true);
        // put buffer into texture
        screenTexture.ReadPixels(new Rect(0f, 0f, Screen.width, Screen.height), 0, 0);

        // apply
        screenTexture.Apply();
        return screenTexture;
    }

    public static void RateUs()
    {
#if UNITY_ANDROID
        Application.OpenURL("market://details?id=" + androidID);
#elif UNITY_IPHONE
		Application.OpenURL("itms-apps://itunes.apple.com/app/"+iphoneID);
#endif
    }
}
