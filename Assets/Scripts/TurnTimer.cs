using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TurnTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] float timerDuration = 5f;
    
    [Header("UI References")]
    [SerializeField] GameObject timerObject;
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] Image timerFillImage; 
    
    private float currentTime;
    private bool isTimerActive = false;
    
    public static TurnTimer instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    void Start()
    {
        if (timerObject != null)
            timerObject.SetActive(false);
    }

    void Update()
    {
        if (!isTimerActive) return;

        currentTime -= Time.deltaTime;

        UpdateTimerDisplay();

        if (currentTime <= 0f)
        {
            isTimerActive = false;
            if (timerObject != null)
                timerObject.SetActive(false);
            
            OnTimerExpired();
        }
    }

    public void StartTimer()
    {
        currentTime = timerDuration;
        isTimerActive = true;
        
        if (timerObject != null)
            timerObject.SetActive(true);
        
        UpdateTimerDisplay();
    }

    public void StopTimer()
    {
        isTimerActive = false;
        
        if (timerObject != null)
            timerObject.SetActive(false);
    }

    void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            timerText.text = Mathf.Ceil(currentTime).ToString("0");
        }

        if (timerFillImage != null)
        {
            timerFillImage.fillAmount = currentTime / timerDuration;
        }
    }

    void OnTimerExpired()
    {
        if (BSGameLogic.instance != null)
        {
            BSGameLogic.instance.Continue();
        }
    }

    public bool IsTimerActive()
    {
        return isTimerActive;
    }
}