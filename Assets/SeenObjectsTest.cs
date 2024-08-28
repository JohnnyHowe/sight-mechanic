using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SeenObjectsTest : MonoBehaviour
{
    [SerializeField] private Sight sight;
    [SerializeField] private Transform targetContainer;

    void Update()
    {
        IEnumerable<GameObject> seenObjects = sight.GetSeenObjects();
        foreach (Transform child in targetContainer)
        {
            child.GetComponent<SpriteRenderer>().color = seenObjects.Contains(child.gameObject)? Color.red: Color.green;
        }
    }
}
