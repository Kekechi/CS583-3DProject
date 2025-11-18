using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject roomSelectionPanel;
    public GameObject miniGamePanel;
    public GameObject pauseMenuPanel;
    public GameObject summaryPanel;

    [Header("Main Menu Elements")]
    public Button startButton;
    public Button settingsButton;
    public Button quitButton;

    [Header("Pause Menu Elements")]
    public Button resumeButton;
    public Button mainMenuButton;

    void Start()
    {
        // Setup button listeners
        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonClicked);

        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeButtonClicked);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitButtonClicked);

        HideAllPanels();
    }

    public void ShowMainMenu()
    {
        HideAllPanels();
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
    }

    public void ShowRoomSelection()
    {
        HideAllPanels();
        if (roomSelectionPanel != null)
            roomSelectionPanel.SetActive(true);
    }

    public void ShowMiniGameUI()
    {
        HideAllPanels();
        if (miniGamePanel != null)
            miniGamePanel.SetActive(true);
    }

    public void ShowPauseMenu()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);
    }

    public void ShowSummary()
    {
        HideAllPanels();
        if (summaryPanel != null)
            summaryPanel.SetActive(true);
    }

    void HideAllPanels()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (roomSelectionPanel != null) roomSelectionPanel.SetActive(false);
        if (miniGamePanel != null) miniGamePanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (summaryPanel != null) summaryPanel.SetActive(false);
    }

    // Button callbacks
    void OnStartButtonClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.StartGame();
    }

    void OnResumeButtonClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ResumeGame();
    }

    void OnMainMenuButtonClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ReturnToMainMenu();
    }

    void OnQuitButtonClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}
