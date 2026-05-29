using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> targets = new List<GameObject>();
    [SerializeField] private Text scoreText;
    [SerializeField] private Text gameOverText;
    [SerializeField] private GameObject titleScreen;
    [SerializeField] private Button restartButton;
    [SerializeField] private float baseSpawnRate = 1.0f;

    public bool IsGameActive { get; private set; }

    private int score;
    private float spawnRate;
    private Coroutine spawnRoutine;

    private void Start()
    {
        UpdateScore(0);
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }

        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(false);
        }
    }

    public void StartGame(int difficulty)
    {
        score = 0;
        spawnRate = baseSpawnRate / Mathf.Max(1, difficulty);
        IsGameActive = true;
        UpdateScore(0);

        if (titleScreen != null)
        {
            titleScreen.SetActive(false);
        }

        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }

        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(false);
        }

        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
        }

        spawnRoutine = StartCoroutine(SpawnTarget());
    }

    public void UpdateScore(int scoreToAdd)
    {
        score += scoreToAdd;
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    public void GameOver()
    {
        if (!IsGameActive)
        {
            return;
        }

        IsGameActive = false;
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
        }

        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(true);
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private IEnumerator SpawnTarget()
    {
        while (IsGameActive)
        {
            yield return new WaitForSeconds(spawnRate);

            if (!IsGameActive || targets.Count == 0)
            {
                continue;
            }

            int index = Random.Range(0, targets.Count);
            Instantiate(targets[index]);
        }
    }
}
