using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    [Header("Canvases")]
    public Canvas originalCanvas;
    public Canvas badUICanvas;

    [Header("Tutorial Effects")]
    public ArrowFade arrowFade;
    public FadeImage distanceFade;

    public FadeImage centeredImage;
    public FadeImage floatingImage;
    public FadeImage roundedImage;
    public FadeImage colorsImage;
    public FadeImage spacingImage;
    public FadeImage backgroundImage;
    public FadeImage arialImage;
    public FadeImage interactiveImage;
    public FadeImage feedbackImage;
    public FadeImage keyboardImage;
    public FadeImage instructionsImage;
    public FadeImage textboxImage;
    public FadeImage helpImage;
    public FadeImage tutorialEndImage;

    [Header("Menu Buttons")]
    public Button TutBackButton;
    public Button TutHelpButton;
    public Button tutSaveButton;
    public Toggle tutToggle;

    [Header("Tutorial Buttons")]
    public Button tutorialBeginButton;
    public Button tutorialExitButton;
    public Button tutorialResetButton;

    [Header("Input Field for Tutorial")]
    public InputField tutorialInputField;

    [Header("Panels")]
    public GameObject initialButtons;
    public GameObject goodUIPanel;
    public GameObject badUIPanel;
    public GameObject backButtonGood;
    public GameObject backButtonBad;

    [Header("Tutorial Panels")]
    public Canvas tutorialCanvas;
    public GameObject tutorialInfoPanel;
    public GameObject tutorialMainPanel;

    [Header("Good UI Fields")]
    public InputField nameField;
    public Toggle sampleToggle;

    [Header("Bad UI Fields")]
    public InputField nameFieldBad;
    public Toggle sampleToggleBad;

    [Header("Good UI Label Buttons")]
    public Button goodUIButton1;
    public Button goodUIButton2;

    [Header("Bad UI Label Buttons")]
    public Button badUIButton1;
    public Button badUIButton2;

    [Header("Help Panel")]
    public GameObject helpPanel;

    [Header("Save Confirmation")]
    public GameObject saveConfirmationPanel;

    [Header("Passthrough")]
    public GameObject passthroughLayer;

    [Header("Immersive Background")]
    public GameObject immersiveBackground;

    [Header("Logo Images")]
    public GameObject goodUILogo;
    public GameObject badUILogo;

    private string originalGoodButton1Label;
    private string originalGoodButton2Label;
    private string originalBadButton1Label;
    private string originalBadButton2Label;

    private FormData savedGoodUIData = null;
    private FormData savedBadUIData = null;
    private Coroutine savePanelCoroutine;

    private bool pendingGoodUIPanel = false;
    private Coroutine hoverPulseCoroutine;
    private bool isHoverPulsing = false;
    
    private Coroutine tutorialSequenceCoroutine;
    private bool isTutorialRunning = false;

    private void Start()
    {
        StoreOriginalButtonLabels();

        sampleToggle.onValueChanged.AddListener(delegate { TogglePassthroughAndLogo(sampleToggle.isOn, isGoodUI: true); });
        sampleToggleBad.onValueChanged.AddListener(delegate { TogglePassthroughAndLogo(sampleToggleBad.isOn, isGoodUI: false); });

        TogglePassthroughAndLogo(false, isGoodUI: true);
        TogglePassthroughAndLogo(false, isGoodUI: false);

        if (tutorialCanvas != null) tutorialCanvas.gameObject.SetActive(false);
        if (tutorialInfoPanel != null) tutorialInfoPanel.SetActive(false);
        if (tutorialMainPanel != null) tutorialMainPanel.SetActive(false);

        SetupTutorialButtons();
    }

    private void StoreOriginalButtonLabels()
    {
        if (goodUIButton1 != null)
        {
            Text button1Text = goodUIButton1.GetComponentInChildren<Text>();
            TextMeshProUGUI button1TMP = goodUIButton1.GetComponentInChildren<TextMeshProUGUI>();
            originalGoodButton1Label = button1Text != null ? button1Text.text : 
                                     (button1TMP != null ? button1TMP.text : "");
        }

        if (goodUIButton2 != null)
        {
            Text button2Text = goodUIButton2.GetComponentInChildren<Text>();
            TextMeshProUGUI button2TMP = goodUIButton2.GetComponentInChildren<TextMeshProUGUI>();
            originalGoodButton2Label = button2Text != null ? button2Text.text : 
                                     (button2TMP != null ? button2TMP.text : "");
        }

        if (badUIButton1 != null)
        {
            Text button1Text = badUIButton1.GetComponentInChildren<Text>();
            TextMeshProUGUI button1TMP = badUIButton1.GetComponentInChildren<TextMeshProUGUI>();
            originalBadButton1Label = button1Text != null ? button1Text.text : 
                                    (button1TMP != null ? button1TMP.text : "");
        }

        if (badUIButton2 != null)
        {
            Text button2Text = badUIButton2.GetComponentInChildren<Text>();
            TextMeshProUGUI button2TMP = badUIButton2.GetComponentInChildren<TextMeshProUGUI>();
            originalBadButton2Label = button2Text != null ? button2Text.text : 
                                    (button2TMP != null ? button2TMP.text : "");
        }
    }

    private void SetupTutorialButtons()
    {
        if (tutorialBeginButton != null)
            tutorialBeginButton.onClick.AddListener(BeginTutorial);
        
        if (tutorialExitButton != null)
            tutorialExitButton.onClick.AddListener(ExitTutorial);
        
        if (tutorialResetButton != null)
            tutorialResetButton.onClick.AddListener(ResetTutorial);
    }

    public void ShowGoodUI()
    {
        initialButtons.SetActive(false);
        pendingGoodUIPanel = true;

        if (helpPanel != null)
        {
            helpPanel.SetActive(true);
            helpPanel.transform.SetAsLastSibling();
        }
    }

    public void ShowBadUI()
    {
        originalCanvas.gameObject.SetActive(false);
        badUICanvas.gameObject.SetActive(true);
        badUIPanel.SetActive(true);
        backButtonBad.SetActive(true);

        if (savedBadUIData != null)
        {
            LoadFormData(savedBadUIData, isBad: true);
        }
        else
        {
            ClearForm(isBad: true);
        }

        if (helpPanel != null) helpPanel.SetActive(false);
    }

    public void ReturnFromGoodUI()
    {
        goodUIPanel.SetActive(false);
        backButtonGood.SetActive(false);
        initialButtons.SetActive(true);
        ClearForm(isBad: false);
        TogglePassthroughAndLogo(false, isGoodUI: true);

        if (savedGoodUIData == null)
        {
            ResetButtonLabelsToOriginal(isGoodUI: true);
        }

        if (helpPanel != null) helpPanel.SetActive(false);
        if (saveConfirmationPanel != null) saveConfirmationPanel.SetActive(false);
    }

    public void ReturnFromBadUI()
    {
        badUIPanel.SetActive(false);
        backButtonBad.SetActive(false);
        badUICanvas.gameObject.SetActive(false);
        originalCanvas.gameObject.SetActive(true);
        initialButtons.SetActive(true);
        ClearForm(isBad: true);
        TogglePassthroughAndLogo(false, isGoodUI: false);

        if (savedBadUIData == null)
        {
            ResetButtonLabelsToOriginal(isGoodUI: false);
        }

        if (helpPanel != null) helpPanel.SetActive(false);
        if (saveConfirmationPanel != null) saveConfirmationPanel.SetActive(false);
    }

  private void ResetButtonLabelsToOriginal(bool isGoodUI)
{
    if (isGoodUI)
    {
        SetButtonLabel(goodUIButton1, originalGoodButton1Label, isOriginal: true);
        SetButtonLabel(goodUIButton2, originalGoodButton2Label, isOriginal: true);
    }
    else
    {
        SetButtonLabel(badUIButton1, originalBadButton1Label, isOriginal: true);
        SetButtonLabel(badUIButton2, originalBadButton2Label, isOriginal: true);
    }
}


    private void SetButtonLabel(Button button, string label, bool isOriginal = false)
{
    if (button == null) return;

    Text buttonText = button.GetComponentInChildren<Text>();
    TextMeshProUGUI buttonTMP = button.GetComponentInChildren<TextMeshProUGUI>();

    Color grayColor = new Color32(145, 145, 145, 255); 

    if (buttonText != null)
    {
        buttonText.text = label;
        if (isOriginal)
        {
            buttonText.fontStyle = FontStyle.BoldAndItalic;
            buttonText.color = grayColor;
        }
    }
    else if (buttonTMP != null)
    {
        buttonTMP.text = label;
        if (isOriginal)
        {
            buttonTMP.fontStyle = FontStyles.Bold | FontStyles.Italic;
            buttonTMP.color = grayColor;
        }
    }
}


    private string GetButtonLabel(Button button)
    {
        if (button == null) return "";

        Text buttonText = button.GetComponentInChildren<Text>();
        TextMeshProUGUI buttonTMP = button.GetComponentInChildren<TextMeshProUGUI>();

        return buttonText != null ? buttonText.text : 
               (buttonTMP != null ? buttonTMP.text : "");
    }

    public void SaveCurrentGoodUI()
    {
        savedGoodUIData = new FormData
        {
            name = nameField.text,
            toggle = sampleToggle.isOn,
            button1Label = GetButtonLabel(goodUIButton1),
            button2Label = GetButtonLabel(goodUIButton2)
        };

        ShowSaveConfirmation();
    }

    public void SaveCurrentBadUI()
    {
        savedBadUIData = new FormData
        {
            name = nameFieldBad.text,
            toggle = sampleToggleBad.isOn,
            button1Label = GetButtonLabel(badUIButton1),
            button2Label = GetButtonLabel(badUIButton2)
        };

        ShowSaveConfirmation();
    }

    private void LoadFormData(FormData data, bool isBad)
    {
        if (!isBad)
        {
            nameField.text = data.name;
            sampleToggle.isOn = data.toggle;
            TogglePassthroughAndLogo(data.toggle, isGoodUI: true);

            if (!string.IsNullOrEmpty(data.button1Label))
                SetButtonLabel(goodUIButton1, data.button1Label);
            if (!string.IsNullOrEmpty(data.button2Label))
                SetButtonLabel(goodUIButton2, data.button2Label);
        }
        else
        {
            nameFieldBad.text = data.name;
            sampleToggleBad.isOn = data.toggle;
            TogglePassthroughAndLogo(data.toggle, isGoodUI: false);

            if (!string.IsNullOrEmpty(data.button1Label))
                SetButtonLabel(badUIButton1, data.button1Label);
            if (!string.IsNullOrEmpty(data.button2Label))
                SetButtonLabel(badUIButton2, data.button2Label);
        }
    }

    private void ClearForm(bool isBad)
    {
        if (!isBad)
        {
            nameField.text = "";
            sampleToggle.isOn = false;

            if (savedGoodUIData == null)
            {
                SetButtonLabel(goodUIButton1, originalGoodButton1Label);
                SetButtonLabel(goodUIButton2, originalGoodButton2Label);
            }
        }
        else
        {
            nameFieldBad.text = "";
            sampleToggleBad.isOn = false;

            if (savedBadUIData == null)
            {
                SetButtonLabel(badUIButton1, originalBadButton1Label);
                SetButtonLabel(badUIButton2, originalBadButton2Label);
            }
        }
    }

    private void TogglePassthroughAndLogo(bool immersiveModeEnabled, bool isGoodUI)
    {
        if (passthroughLayer != null)
            passthroughLayer.SetActive(!immersiveModeEnabled);

        if (immersiveBackground != null)
            immersiveBackground.SetActive(immersiveModeEnabled);

        if (isGoodUI)
        {
            if (goodUILogo != null)
                goodUILogo.SetActive(immersiveModeEnabled);
        }
        else
        {
            if (badUILogo != null)
                badUILogo.SetActive(immersiveModeEnabled);
        }
    }

    private void TogglePassthrough(bool immersiveModeEnabled)
    {
        if (passthroughLayer != null)
            passthroughLayer.SetActive(!immersiveModeEnabled);

        if (immersiveBackground != null)
            immersiveBackground.SetActive(immersiveModeEnabled);
    }

    public void ShowHelpPanel()
    {
        if (helpPanel != null)
        {
            helpPanel.SetActive(true);
            helpPanel.transform.SetAsLastSibling();
        }
    }

    public void CloseHelpPanel()
    {
        if (helpPanel != null)
        {
            helpPanel.SetActive(false);
        }

        if (pendingGoodUIPanel)
        {
            pendingGoodUIPanel = false;

            goodUIPanel.SetActive(true);
            backButtonGood.SetActive(true);

            if (savedGoodUIData != null)
                LoadFormData(savedGoodUIData, isBad: false);
            else
                ClearForm(isBad: false);
        }
    }

    private void ShowSaveConfirmation()
    {
        if (saveConfirmationPanel != null)
        {
            saveConfirmationPanel.SetActive(true);

            if (savePanelCoroutine != null)
                StopCoroutine(savePanelCoroutine);

            savePanelCoroutine = StartCoroutine(HideSaveConfirmationAfterDelay(3f));
        }
    }

    private IEnumerator HideSaveConfirmationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        saveConfirmationPanel.SetActive(false);
        savePanelCoroutine = null;
    }
    
    private void SetButtonsHoverState(bool isHovered)
    {
        var pointerData = new PointerEventData(EventSystem.current);
        var buttons = new[] { TutBackButton, TutHelpButton, tutSaveButton };
        foreach (var button in buttons)
        {
            if (isHovered)
                ExecuteEvents.Execute(button.gameObject, pointerData, ExecuteEvents.pointerEnterHandler);
            else
                ExecuteEvents.Execute(button.gameObject, pointerData, ExecuteEvents.pointerExitHandler);
        }

        var togglePointer = new PointerEventData(EventSystem.current);
        if (isHovered)
            ExecuteEvents.Execute(tutToggle.gameObject, togglePointer, ExecuteEvents.pointerEnterHandler);
        else
            ExecuteEvents.Execute(tutToggle.gameObject, togglePointer, ExecuteEvents.pointerExitHandler);
    }

    private IEnumerator PulseHoverState()
    {
        isHoverPulsing = true;
        while (isHoverPulsing)
        {
            SetButtonsHoverState(true);
            yield return new WaitForSeconds(0.5f);
            SetButtonsHoverState(false);
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void ShowTutorial()
    {
        originalCanvas.gameObject.SetActive(false);
        tutorialCanvas.gameObject.SetActive(true);

        if (tutorialInfoPanel != null) tutorialInfoPanel.SetActive(true);
        if (tutorialMainPanel != null) tutorialMainPanel.SetActive(false);
    }

    public void BeginTutorial()
    {
        Debug.Log("Begin Tutorial button pressed");
        
        if (tutorialSequenceCoroutine != null)
        {
            StopCoroutine(tutorialSequenceCoroutine);
            tutorialSequenceCoroutine = null;
        }
        
        if (tutorialInfoPanel != null) tutorialInfoPanel.SetActive(false);
        if (tutorialMainPanel != null) tutorialMainPanel.SetActive(true);
        
        isTutorialRunning = true;
        tutorialSequenceCoroutine = StartCoroutine(PlayTutorialSequence());
    }

    public void ExitTutorial()
    {
        Debug.Log("Exit Tutorial button pressed");
        
        StopTutorialSequence();
        
        if (tutorialCanvas != null) tutorialCanvas.gameObject.SetActive(false);
        if (originalCanvas != null) originalCanvas.gameObject.SetActive(true);
        if (initialButtons != null) initialButtons.SetActive(true);
        
        if (tutorialInfoPanel != null) tutorialInfoPanel.SetActive(false);
        if (tutorialMainPanel != null) tutorialMainPanel.SetActive(false);
    }

    public void ResetTutorial()
    {
        Debug.Log("Reset Tutorial button pressed");
        
        if (tutorialResetButton != null)
        {
            tutorialResetButton.OnDeselect(null);
            EventSystem.current.SetSelectedGameObject(null);
        }
        
        if (tutorialSequenceCoroutine != null)
        {
            StopCoroutine(tutorialSequenceCoroutine);
            tutorialSequenceCoroutine = null;
        }
        
        isHoverPulsing = false;
        if (hoverPulseCoroutine != null)
        {
            StopCoroutine(hoverPulseCoroutine);
            hoverPulseCoroutine = null;
        }
        
        SetButtonsHoverState(false);
        SetHelpButtonHoverState(false);
        
        if (tutorialInputField != null)
        {
            tutorialInputField.DeactivateInputField();
        }
        
        StopAllFadeCoroutines();
        ResetAllFadeImages();
        
        StartCoroutine(RestartTutorialAfterFrame());
    }

    private void StopAllFadeCoroutines()
    {
        var fadeImages = new FadeImage[] 
        { 
            distanceFade, centeredImage, floatingImage, roundedImage, 
            colorsImage, spacingImage, backgroundImage, arialImage, 
            interactiveImage, feedbackImage, keyboardImage, 
            instructionsImage, textboxImage, helpImage, tutorialEndImage 
        };
        
        foreach (var fadeImage in fadeImages)
        {
            if (fadeImage != null)
            {
                fadeImage.StopFade();
            }
        }
        
        if (arrowFade != null)
        {
            arrowFade.StopFade();
        }
    }

    private IEnumerator RestartTutorialAfterFrame()
    {
        yield return null;
        
        isTutorialRunning = true;
        tutorialSequenceCoroutine = StartCoroutine(PlayTutorialSequence());
    }

    private void StartTutorialSequence()
    {
        isTutorialRunning = true;
        tutorialSequenceCoroutine = StartCoroutine(PlayTutorialSequence());
    }

    private void StopTutorialSequence()
    {
        if (tutorialSequenceCoroutine != null)
        {
            StopCoroutine(tutorialSequenceCoroutine);
            tutorialSequenceCoroutine = null;
        }
        
        isTutorialRunning = false;
        
        isHoverPulsing = false;
        if (hoverPulseCoroutine != null)
        {
            StopCoroutine(hoverPulseCoroutine);
            hoverPulseCoroutine = null;
        }
        
        SetButtonsHoverState(false);
        SetHelpButtonHoverState(false);
        
        if (tutorialInputField != null)
        {
            tutorialInputField.DeactivateInputField();
        }
        
        ResetAllFadeImages();
    }

    private void ResetAllFadeImages()
    {
        var fadeImages = new FadeImage[] 
        { 
            distanceFade, centeredImage, floatingImage, roundedImage, 
            colorsImage, spacingImage, backgroundImage, arialImage, 
            interactiveImage, feedbackImage, keyboardImage, 
            instructionsImage, textboxImage, helpImage, tutorialEndImage 
        };
        
        foreach (var fadeImage in fadeImages)
        {
            if (fadeImage != null)
            {
                fadeImage.gameObject.SetActive(true);
                CanvasGroup canvasGroup = fadeImage.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 0f;
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;
                }
            }
        }
        
        if (arrowFade != null)
        {
            arrowFade.gameObject.SetActive(true);
            Image arrowImage = arrowFade.GetComponent<Image>();
            if (arrowImage != null)
            {
                arrowImage.fillAmount = 0f;
            }
        }
    }

    private IEnumerator RunMultiple(params IEnumerator[] coroutines)
    {
        Coroutine[] handles = new Coroutine[coroutines.Length];
        for (int i = 0; i < coroutines.Length; i++)
        {
            handles[i] = StartCoroutine(coroutines[i]);
        }
        foreach (Coroutine handle in handles)
        {
            yield return handle;
        }
    }

    private IEnumerator PlayTutorialSequence()
    {
        Debug.Log("Starting tutorial sequence");
        yield return new WaitForSeconds(1f);

        if (!isTutorialRunning) yield break;

        if (arrowFade != null)
        {
            Debug.Log("Starting arrow fade");
            StartCoroutine(arrowFade.PlayFade(duration: 1f, holdTime: 3f));
        }

        yield return new WaitForSeconds(1f);
        if (!isTutorialRunning) yield break;

        if (distanceFade != null)
        {
            Debug.Log("Starting distance fade");
            yield return StartCoroutine(distanceFade.PlayFade(duration: 1f, holdTime: 2f));
        }

        yield return new WaitForSeconds(1f);
        if (!isTutorialRunning) yield break;

        if (centeredImage != null)
        {
            Debug.Log("Starting centered image fade");
            yield return StartCoroutine(centeredImage.PlayFade(duration: 1f, holdTime: 3f));
        }

        yield return new WaitForSeconds(1f);
        if (!isTutorialRunning) yield break;

        if (floatingImage != null)
        {
            Debug.Log("Starting floating image fade");
            yield return StartCoroutine(floatingImage.PlayFade(duration: 1f, holdTime: 3f));
        }

        yield return new WaitForSeconds(1f);
        if (!isTutorialRunning) yield break;

        if (colorsImage != null)
        {
            Debug.Log("Starting colors image fade");
            yield return StartCoroutine(colorsImage.PlayFade(duration: 1f, holdTime: 3f));
        }

        yield return new WaitForSeconds(1f);
        if (!isTutorialRunning) yield break;

        if (roundedImage != null)
        {
            Debug.Log("Starting rounded image fade");
            yield return StartCoroutine(roundedImage.PlayFade(duration: 1f, holdTime: 3f));
        }

        yield return new WaitForSeconds(1f);
        if (!isTutorialRunning) yield break;

        if (spacingImage != null)
        {
            Debug.Log("Starting spacing image fade");
            yield return StartCoroutine(spacingImage.PlayFade(duration: 1f, holdTime: 3f));
        }

        yield return new WaitForSeconds(1f);
        if (!isTutorialRunning) yield break;

        if (backgroundImage != null)
        {
            Debug.Log("Starting background image fade");
            yield return StartCoroutine(backgroundImage.PlayFade(duration: 1f, holdTime: 3f));
        }

        yield return new WaitForSeconds(1f);
        if (!isTutorialRunning) yield break;

        if (arialImage != null)
        {
            Debug.Log("Starting arial image fade");
            yield return StartCoroutine(arialImage.PlayFade(duration: 1f, holdTime: 3f));
        }

        yield return new WaitForSeconds(1f);
        if (!isTutorialRunning) yield break;

        if (interactiveImage != null)
        {
            Debug.Log("Starting interactive image fade");
            yield return StartCoroutine(interactiveImage.PlayFade(duration: 1f, holdTime: 3f));
        }

        yield return new WaitForSeconds(1f);
        if (!isTutorialRunning) yield break;

        if (feedbackImage != null)
        {
            Debug.Log("Starting feedback image fade with hover pulse");
            hoverPulseCoroutine = StartCoroutine(PulseHoverState());
            yield return StartCoroutine(feedbackImage.PlayFade(duration: 1f, holdTime: 3f));
            isHoverPulsing = false;

            if (hoverPulseCoroutine != null)
                StopCoroutine(hoverPulseCoroutine);

            SetButtonsHoverState(false);
            yield return new WaitForSeconds(1f);
        }

        if (!isTutorialRunning) yield break;

        if (tutorialInputField != null)
        {
            Debug.Log("Starting keyboard input demonstration");
            tutorialInputField.Select();
            tutorialInputField.ActivateInputField();

            if (keyboardImage != null)
                yield return StartCoroutine(keyboardImage.PlayFade(duration: 1f, holdTime: 3f));

            tutorialInputField.DeactivateInputField();
        }

        yield return new WaitForSeconds(1f);
        if (!isTutorialRunning) yield break;

        Debug.Log("Starting final help section with multiple elements");
        SetHelpButtonHoverState(true);

        IEnumerator fadeInstructions = instructionsImage != null ? instructionsImage.PlayFade(duration: 1f, holdTime: 7f) : null;
        IEnumerator fadeTextbox = textboxImage != null ? textboxImage.PlayFade(duration: 1f, holdTime: 7f) : null;
        IEnumerator fadeHelpImage = helpImage != null ? helpImage.PlayFade(duration: 1f, holdTime: 7f) : null;

        yield return RunMultiple(fadeInstructions, fadeTextbox, fadeHelpImage);

        if (!isTutorialRunning) yield break;

        SetHelpButtonHoverState(false);
        
        yield return new WaitForSeconds(1f);
        if (!isTutorialRunning) yield break;
        
        if (tutorialEndImage != null)
        {
            Debug.Log("Starting tutorial end image fade");
            yield return StartCoroutine(tutorialEndImage.PlayFade(duration: 1f, holdTime: 3f));
        }
        
        Debug.Log("Tutorial sequence completed");
        
        isTutorialRunning = false;
        tutorialSequenceCoroutine = null;
    }

    private void SetHelpButtonHoverState(bool isHovered)
    {
        if (TutHelpButton == null) return;
        
        var pointerData = new PointerEventData(EventSystem.current);

        if (isHovered)
            ExecuteEvents.Execute(TutHelpButton.gameObject, pointerData, ExecuteEvents.pointerEnterHandler);
        else
            ExecuteEvents.Execute(TutHelpButton.gameObject, pointerData, ExecuteEvents.pointerExitHandler);
    }
}

