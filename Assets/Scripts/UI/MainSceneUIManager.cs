using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainSceneUIManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup mainPanel;
    [SerializeField] private CanvasGroup creditsPanel;
    [SerializeField] private Button playButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button creditsBackButton;

    private void Awake()
    {
        if (playButton) playButton.onClick.AddListener(OnPlayButtonClicked);
        if (creditsButton) creditsButton.onClick.AddListener(OnCreditsButtonClicked);
        if (creditsBackButton) creditsBackButton.onClick.AddListener(OnCreditsBackButtonClicked);
    }

    private void OnPlayButtonClicked()
    {
        AudioManager.Instance.PlayOneShotSFX(AudioName.SFXButtonClick);
        SceneManager.LoadScene("GameScene");
    }

    private void OnCreditsButtonClicked()
    {
        AudioManager.Instance.PlayOneShotSFX(AudioName.SFXButtonClick);
        mainPanel.gameObject.SetActive(false);
        creditsPanel.gameObject.SetActive(true);
    }

    private void OnCreditsBackButtonClicked()
    {
        AudioManager.Instance.PlayOneShotSFX(AudioName.SFXButtonClick);
        creditsPanel.gameObject.SetActive(false);
        mainPanel.gameObject.SetActive(true);
    }
}
