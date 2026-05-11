using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public Button playOfflineButton;
    public Button multiplayerButton;
    public Button quitButton;

    private void Start()
    {
        playOfflineButton.onClick.AddListener(OnPlayOfflineClicked);
        multiplayerButton.onClick.AddListener(OnMultiplayerClicked);
        quitButton.onClick.AddListener(OnQuitClicked);

        Debug.Log("MainMenuManager started");
    }

    private void OnPlayOfflineClicked()
    {
        Debug.Log("Play Offline clicked");
        SceneManager.LoadScene("GameScene");
    }

    private void OnMultiplayerClicked()
    {
        SceneManager.LoadScene("LobbyScene");
    }

    private void OnQuitClicked()
    {
        Debug.Log("Quit clicked");
        Application.Quit();
    }
}