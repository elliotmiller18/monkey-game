using UnityEngine;

public class CallAudio : MonoBehaviour
{
    [SerializeField] AudioClip wrong;
    [SerializeField] AudioClip right;

    public static CallAudio instance;

    public void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError("Duplicate CallAudio, destroying");
            Destroy(gameObject);
            return;
        }
        instance = this;
    }
    
    public void PlayCallClip(bool successful)
    {
        AudioSource.PlayClipAtPoint(successful ? right : wrong, Camera.main.transform.position);
    }
}
