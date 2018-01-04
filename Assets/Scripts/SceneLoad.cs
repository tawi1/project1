using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoad : MonoBehaviour
{
    IEnumerator Start()
    {
        var ao = SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
        yield return ao;
    }
}