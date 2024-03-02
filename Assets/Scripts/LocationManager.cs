using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VectorForestScenery;

public class LocationManager : MonoBehaviour
{
    [SerializeField] private List<SceneryItem> sceneryItems;
    [SerializeField] private int animationItemsBatchSize = 5;
    [SerializeField] private float delay = 5f;

    private float delayTimeLeft = 0;

    void LateUpdate()
    {
        delayTimeLeft -= Time.deltaTime;
        if (delayTimeLeft <= 0)
        {
            delayTimeLeft = delay;
            RunAnimations();
        }
    }

    private void RunAnimations()
    {
        ShuffleSceneryItems();
        int animationId = Random.Range(0, 2);
        sceneryItems.Take(animationItemsBatchSize).ToList().ForEach(i =>
        {
            if (animationId == 0)
            {
                i.WindLeft();
            }
            else
            {
                i.WindRight();
            }
        });
    }

    private void ShuffleSceneryItems()
    {
        System.Random rng = new System.Random();
        int n = sceneryItems.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (sceneryItems[n], sceneryItems[k]) = (sceneryItems[k], sceneryItems[n]);
        }
    }
}
