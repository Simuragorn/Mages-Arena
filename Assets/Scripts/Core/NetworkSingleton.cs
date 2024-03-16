using FishNet.Object;
using UnityEngine;

public class NetworkSingleton<T> : NetworkBehaviour where T : Component
{
    public static T Singleton { get; private set; }
    protected virtual void Awake()
    {
        if (Singleton != null && Singleton != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Singleton = GetComponent<T>();
        }
    }
}
