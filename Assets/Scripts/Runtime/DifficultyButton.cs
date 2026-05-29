using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class DifficultyButton : MonoBehaviour
{
    [SerializeField] private int difficulty = 1;

    private Button button;
    private GameManager gameManager;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        button.onClick.AddListener(SetDifficulty);
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(SetDifficulty);
        }
    }

    private void SetDifficulty()
    {
        if (gameManager != null)
        {
            gameManager.StartGame(difficulty);
        }
    }
}
