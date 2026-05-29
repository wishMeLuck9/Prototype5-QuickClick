using UnityEngine;

public class DestroyAfterDelay : MonoBehaviour
{
    [SerializeField] private float delay = 1.0f;

    private void Start()
    {
        Destroy(gameObject, delay);
    }
}
