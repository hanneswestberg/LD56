using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private bool debugMode;
    public bool DebugMode => debugMode;

    [SerializeField] private GameObject _startGamePanel;
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private GameObject _bloodPanel;
    [SerializeField] private GameObject _suckPanel;
    [SerializeField] private GameObject _catchAllTouchPanel;
    [SerializeField] private TextMeshProUGUI _finalScoreText;

    private Tick tick;
    private FootSpawner footSpawner;
    private GrassSpawner grassSpawner;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        tick = Tick.Instance;
        grassSpawner = GetComponent<GrassSpawner>();
        footSpawner = GetComponent<FootSpawner>();
        grassSpawner.SpawnGrass();
        Input.multiTouchEnabled = false;

        if (debugMode)
        {
            StartGame();
            return;
        }

        _startGamePanel.SetActive(true);
        _gameOverPanel.SetActive(false);
        _bloodPanel.SetActive(false);
        _suckPanel.SetActive(false);
        _catchAllTouchPanel.SetActive(false);
    }

    public void StartGame()
    {
        tick.StartGame();
        footSpawner.StartSpawning();
        tick.Spawn();

        _startGamePanel.SetActive(false);
        _bloodPanel.SetActive(true);
        _suckPanel.SetActive(true);
        _catchAllTouchPanel.SetActive(true);
    }

    public void LoseGame()
    {
        _catchAllTouchPanel.SetActive(false);
        _gameOverPanel.SetActive(true);
        _bloodPanel.SetActive(false);
        _suckPanel.SetActive(false);
        _finalScoreText.text = $"Final score: {tick.Score:F0} \n\n You managed to evolve {tick.timesEvolved - 1} time{(tick.timesEvolved != 1 ? "s" : "")}. \n You mothered {tick.BabiesSpawned} tick{(tick.BabiesSpawned != 1 ? "s" : "")}\n\n{GetFinalScoreText(tick.Score)}";
    }

    private string GetFinalScoreText(float score)
    {
        // Sends random funny message based on score
        if (score < 100)
            return "The tick lived a short life, and died a painful death. This is a sad day for tick-kind.";
        if(score < 300)
            return "The tick lived a pretty short life, and died a tragic death. The tick will be mourned.";
        if (score < 600)
            return "The tick lived a decent life, but sadly it was cut short. The tick will be missed.";
        if (score < 1000)
            return "The tick lived a long life, and died a peaceful death. The tick will be remembered.";
        if (score < 2000)
            return "The tick lived an amazing life, and died a glorious death. The tick will be celebrated.";

        return "The tick lived a legendary life, and died a heroic death. The tick will be immortalized.";
    }


    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
