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
        // Null checks
        if (playerTransforms == null || playerTransforms.Count == 0)
        {
            Debug.LogError("playerTransforms is not assigned or empty! Assign player transforms in the Inspector.");
            return;
        }

        if (callerId < 0 || callerId >= playerTransforms.Count)
        {
            Debug.LogError($"Invalid callerId {callerId}. Must be between 0 and {playerTransforms.Count - 1}");
            return;
        }

        if (victimId < 0 || victimId >= playerTransforms.Count)
        {
            Debug.LogError($"Invalid victimId {victimId}. Must be between 0 and {playerTransforms.Count - 1}");
            return;
        }

        if (playerTransforms[callerId] == null)
        {
            Debug.LogError($"playerTransforms[{callerId}] is null!");
            return;
        }

        if (playerTransforms[victimId] == null)
        {
            Debug.LogError($"playerTransforms[{victimId}] is null!");
            return;
        }

        if (arrowPrefab == null)
        {
            Debug.LogError("arrowPrefab is not assigned! Assign an arrow prefab in the Inspector.");
            return;
        }

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
        if (lastArrow != null)
        {
            Destroy(lastArrow);
            lastArrow = null;
        }
    }
}