using UnityEngine;

public class Shield : MonoBehaviour
{
    private Transform target;
    private bool isLaunched = false;
    public void Launch(Transform spawn)
    {
        target = spawn;
        isLaunched = true;
    }

    private void Update()
    {
        if (!isLaunched)
        {
            return;
        }
        transform.position = target.position;
        transform.rotation = target.rotation;
    }
}
