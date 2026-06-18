using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Target : MonoBehaviour
{
    [SerializeField] private int pointValue = 5;
    [SerializeField] private bool badTarget;
    [SerializeField] private ParticleSystem explosionParticle;

    private readonly float minSpeed = 12f;
    private readonly float maxSpeed = 16f;
    private readonly float maxTorque = 10f;
    private readonly float xRange = 4f;
    private readonly float ySpawnPos = -4.5f;

    private Rigidbody targetRb;
    private GameManager gameManager;

    private void Start()
    {
        targetRb = GetComponent<Rigidbody>();
        gameManager = FindFirstObjectByType<GameManager>();

        transform.position = RandomSpawnPos();
        targetRb.AddForce(RandomForce(), ForceMode.Impulse);
        targetRb.AddTorque(RandomTorque(), RandomTorque(), RandomTorque(), ForceMode.Impulse);
    }

    private void OnMouseDown()
    {
        if (gameManager == null || !gameManager.IsGameActive)
        {
            return;
        }

        if (explosionParticle != null)
        {
            Instantiate(explosionParticle, transform.position, explosionParticle.transform.rotation);
        }

        if (badTarget)
        {
            gameManager.GameOver();
        }
        else
        {
            gameManager.UpdateScore(pointValue);
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (gameManager != null && gameManager.IsGameActive && !badTarget)
        {
            gameManager.GameOver();
        }

        Destroy(gameObject);
    }

    private Vector3 RandomForce()
    {
        return Vector3.up * Random.Range(minSpeed, maxSpeed);
    }

    private float RandomTorque()
    {
        return Random.Range(-maxTorque, maxTorque);
    }

    private Vector3 RandomSpawnPos()
    {
        return new Vector3(Random.Range(-xRange, xRange), ySpawnPos, 0f);
    }
}
