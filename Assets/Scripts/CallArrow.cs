using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CallArrow : MonoBehaviour
{
    [SerializeField] List<Transform> playerTransforms;
    [SerializeField] float timeOnScreen = 1.5f;
    [SerializeField] GameObject arrowPrefab;

    GameObject lastArrow = null;

    public static CallArrow instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError("duplicate CallArrow, destroying");
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    public void DrawArrow(int callerId, int victimId, bool successful)
    {
        if (lastArrow != null) DeleteArrow();

        Vector3 start = playerTransforms[callerId].position;
        Vector3 end = playerTransforms[victimId].position;
        Vector3 dir = end - start;

        lastArrow = Instantiate(arrowPrefab, start, Quaternion.identity);
        lastArrow.transform.position = start + dir / 2f;
        lastArrow.transform.rotation = Quaternion.LookRotation(dir);
        lastArrow.transform.localScale = new Vector3(0.1f, 0.1f, dir.magnitude);

        var renderer = lastArrow.GetComponent<Renderer>();
        if (renderer != null)
            renderer.material.color = successful ? Color.green : Color.red;

        StartCoroutine(DeleteArrowAfterTime());
    }

    IEnumerator DeleteArrowAfterTime()
    {
        yield return new WaitForSeconds(timeOnScreen);
        if (lastArrow != null)
        {
            DeleteArrow();
        }
    }
    
    public void DeleteArrow()
    {
        Destroy(lastArrow);
        lastArrow = null;
    }
}
