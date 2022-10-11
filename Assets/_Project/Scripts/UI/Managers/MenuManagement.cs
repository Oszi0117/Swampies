using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MenuManagement : MonoBehaviour
{
    [Header("STARTING CROSSFADE")]
    [SerializeField] private Image _startingCrossfadeImg;
    [SerializeField] private Color32 _startingCrossfadeColor;
    [Space(10   )]
    [Header("MAIN MENU")]
    [SerializeField] private GameObject _mainMenuPanel;
    [SerializeField] private Button _enableSettingsButton;
    [SerializeField] private Button _disableSettingsButton;
    [SerializeField] private GameObject _settingsPanel;
    [SerializeField] private Button _dailyQuestsButton;
    [SerializeField] private Sprite[] _dailyQuestsBarsWithArrows = new Sprite[2];
    [SerializeField] private GameObject _dailyQuestsScrollView;
    [SerializeField] private GameObject _playButton;
    [SerializeField] private GameObject _pawn;
    [SerializeField] private Button _secretSceneButton;
    [Space(10)]
    [Header("MODE MENU")]
    [SerializeField] private GameObject _modeMenuPanel;
    [SerializeField] private GameObject _singleButton;
    [SerializeField] private GameObject _multiButton;
    [Space(10)]
    [Header("LEVELS MENU")]
    [SerializeField] private GameObject _levelsMenuPanel;
    [SerializeField] private GameObject _levelPanel;
    [SerializeField] private TMP_Text _levelNameText;
    [SerializeField] private TMP_Text _yourScoreText;
    [SerializeField] private GameObject _closeLevelPanelButton;
    [SerializeField] private Toggle _speedrunToggle;

    private ButtonsAnimations _DOTweenAnimations;
    private GameObject _currentPanel;
    private string _levelNameToLoad;

    private void Awake()
    {
        _DOTweenAnimations = GetComponent<ButtonsAnimations>();
    }

    private void OnEnable()
    {
        _currentPanel = _mainMenuPanel;
        _startingCrossfadeImg.enabled = true;
        _startingCrossfadeImg.color = _startingCrossfadeColor;
        SpriteRenderer[] pawnSprites = _pawn.GetComponentsInChildren<SpriteRenderer>();
        if (pawnSprites.Length > 0)
        {
            foreach (SpriteRenderer pawnSprite in pawnSprites)
            {
                var pawnColor = pawnSprite.color;
                pawnSprite.color = new Color(pawnColor.r, pawnColor.g, pawnColor.b, 0);
                pawnSprite.DOColor(new Color(pawnColor.r, pawnColor.g, pawnColor.b, 1f), 1.5f);
            }
        }
        _startingCrossfadeImg.DOColor(new Color32(_startingCrossfadeColor.r, _startingCrossfadeColor.g, _startingCrossfadeColor.b, 0), 1.5f).OnComplete(() =>
        {
            _startingCrossfadeImg.enabled = false;
            
        });
    }

    private void Start()
    {
        _enableSettingsButton.onClick.AddListener(() =>
        {
            if (_settingsPanel.activeInHierarchy)
            {
                ClosePanel(_settingsPanel);
            }
            else
            {
                OpenPanel(_settingsPanel);
            }
        });
        _disableSettingsButton.onClick.AddListener(() => ClosePanel(_settingsPanel));
        _playButton.GetComponent<Button>().onClick.AddListener(() => SwitchBetweenPanels(_modeMenuPanel));
        _singleButton.GetComponent<Button>().onClick.AddListener(() => SwitchBetweenPanels(_levelsMenuPanel));
        _dailyQuestsButton.onClick.AddListener(() =>
        {
            if (_dailyQuestsScrollView.activeInHierarchy)
            { 
                _dailyQuestsScrollView.SetActive(false);
                _dailyQuestsButton.GetComponent<Image>().sprite = _dailyQuestsBarsWithArrows[0];
            }
            else
            {
                _dailyQuestsScrollView.SetActive(true);
                _dailyQuestsButton.GetComponent<Image>().sprite = _dailyQuestsBarsWithArrows[1];
            }
        });
        _secretSceneButton.onClick.AddListener((() => LoadSceneWithName("SecretScene")));
        //multi button
        if (PlayerPrefs.HasKey("SpeedrunMode"))
        {
            _speedrunToggle.isOn = PlayerPrefs.GetInt("SpeedrunMode") == 1;
        }
        else
        {
            PlayerPrefs.SetInt("SpeedrunMode", 0);
            _speedrunToggle.isOn = false;
        }
        
        _speedrunToggle.onValueChanged.AddListener((isOn) => { ToggleSpeedrunMode(isOn); });
    }

    public void SwitchBetweenPanels(GameObject openPanel)
    {
        DOTween.KillAll();
        _currentPanel.SetActive(false);
        openPanel.SetActive(true);
        _currentPanel = openPanel;
        if (openPanel == _mainMenuPanel)
        {
            _pawn.SetActive(true);
        }
        else
        {
            _pawn.SetActive(false);
        }
    }

    public void LoadScene()
    {
        if (string.IsNullOrEmpty(_levelNameToLoad)) return;

        DOTween.KillAll();
        LoadingScreenCanvas.Instance?.LoadScene(_levelNameToLoad);
    }
    public void LoadSceneWithName(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;

        DOTween.KillAll();
        LoadingScreenCanvas.Instance?.LoadScene(sceneName);
    }

    public void BackToMenu(RectTransform button)
    {
        DOTween.KillAll();
        button.localScale = Vector3.one;
        _mainMenuPanel.SetActive(true);
        _pawn.SetActive(true);
        _modeMenuPanel.SetActive(false);
        _levelsMenuPanel.SetActive(false);
        _currentPanel = _mainMenuPanel;
        _levelPanel.SetActive(false);
    }

    public void OpenLevelPanel(string levelName)
    {
        _levelNameText.text = levelName;
        //TO DO: read score of current player and display here
        //_yourScoreText.text = ;
        OpenPanel(_levelPanel);
    }
    public void CloseLevelPanel()
    {
        ClosePanel(_levelPanel);
    }
    public void SetLevelToLoad(string levelName)
    {
        _levelNameToLoad = levelName;
    }
    public void ToggleSpeedrunMode(bool isOn)
    {
        if (isOn)
        {
            //enable speedrun mode
            PlayerPrefs.SetInt("SpeedrunMode", 1);

        }
        else
        {
            //disable speedrun
            PlayerPrefs.SetInt("SpeedrunMode", 0);
        }
    }
    private void OpenPanel(GameObject panel)
    {
        panel.transform.localScale = Vector2.zero;
        panel.SetActive(true);
        panel.transform.DOScale(1, 0.5f).SetEase(Ease.InOutCubic);
    }
    private void ClosePanel(GameObject panel)
    {
        panel.transform.DOScale(0, 0.5f).SetEase(Ease.InOutCubic).OnComplete(() => { panel.SetActive(false); });
    }
}