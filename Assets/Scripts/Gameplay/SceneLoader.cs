using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void eneLoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}