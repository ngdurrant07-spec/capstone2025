using UnityEngine;

public class BreakableWall : MonoBehaviour
{
    public void Break()
    {
        Destroy(gameObject);
    }
}
