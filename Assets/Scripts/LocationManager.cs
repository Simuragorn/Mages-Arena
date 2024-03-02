using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VectorForestScenery;

public class LocationManager : MonoBehaviour
{
    [SerializeField] private List<SceneryItem> sceneryItems;
    [SerializeField] private float minDelay = 30f;
    [SerializeField] private float maxDelay = 80f;

    private float delayTimeLeft = 0;

    void LateUpdate()
    {
        delayTimeLeft -= Time.deltaTime;
        if (delayTimeLeft <= 0)
        {
            delayTimeLeft = Random.Range(minDelay, maxDelay);
            RunAnimations();
        }
    }

    private void RunAnimations()
    {
        int animationId = Random.Range(0, 2);
        sceneryItems.ForEach(i =>
        {
            if (animationId == 0)
            {
                i.WindLeft();
            }
            else if (animationId == 1)
            {
                i.WindRight();
            }
        });
    }
}
