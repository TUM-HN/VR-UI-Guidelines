using UnityEngine;
using UnityEngine.UI;

public class SimpleOVRDropdownScroll : MonoBehaviour
{
    [Header("Scroll Settings")]
    [SerializeField] private float scrollSpeed = 500f;
    [SerializeField] private float deadZone = 0.1f;
    [SerializeField] private bool invertScrollDirection = false;
    [SerializeField] private float buttonHeight = 30f; 
    
    [Header("Controller Settings")]
    [SerializeField] private bool useRightController = true;
    [SerializeField] private bool useBothControllers = false;
    
    [Header("UI Focus")]
    [SerializeField] private bool onlyScrollWhenUIFocused = true;
    [SerializeField] private GameObject dropdownObject; 
    
    [Header("Haptic Feedback")]
    [SerializeField] private bool enableHapticFeedback = true;
    [SerializeField] private float hapticIntensity = 0.1f;
    [SerializeField] private float hapticDuration = 0.1f;
    
    [Header("Smooth Scrolling")]
    [SerializeField] private bool useSmoothing = true;
    [SerializeField] private float smoothingSpeed = 8f;
   
    private RectTransform templateRect;
    private RectTransform scrollableContent; 
    private Vector2 currentThumbstickInput;
    private bool isScrolling = false;
    private bool wasScrollingLastFrame = false;
    private Vector3 targetPosition;
    private int totalButtons;
    private int visibleButtons;
    private float maxScrollDistance;
    private float templateHeight;
    private Mask templateMask;
    private float currentScrollOffset = 0f; 
    
    void Awake()
    {
        templateRect = GetComponent<RectTransform>();
        scrollableContent = templateRect;
        templateMask = GetComponent<Mask>();
        
        DisableLayoutComponents();
    }
    
    void Start()
    {
        CalculateScrollBounds();
        
        targetPosition = Vector3.zero;
        currentScrollOffset = 0f;
        
        InitializeButtonPositions();
        
        Debug.Log($"SimpleOVR Scroll initialized - Buttons: {totalButtons}, Visible: {visibleButtons}, Max Scroll: {maxScrollDistance}");
    }
    
    void OnEnable()
    {
                if (templateRect != null)
        {
            StartCoroutine(ForceInitializeOnEnable());
        }
    }
    
    private System.Collections.IEnumerator ForceInitializeOnEnable()
    {
        yield return null;
        
        CalculateScrollBounds();
        InitializeButtonPositions();
        targetPosition = Vector3.zero;
        currentScrollOffset = 0f;
    }
    
    private void DisableLayoutComponents()
    {
        ContentSizeFitter csf = GetComponent<ContentSizeFitter>();
        if (csf != null)
        {
            csf.enabled = false;
        }
        
        VerticalLayoutGroup vlg = GetComponent<VerticalLayoutGroup>();
        if (vlg != null)
        {
            vlg.enabled = false;
        }
        
        HorizontalLayoutGroup hlg = GetComponent<HorizontalLayoutGroup>();
        if (hlg != null)
        {
            hlg.enabled = false;
        }
        
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            LayoutElement le = child.GetComponent<LayoutElement>();
            if (le != null)
            {
                le.enabled = false;
            }
        }
    }
    
    private void InitializeButtonPositions()
    {
        int buttonIndex = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.GetComponent<Button>() != null)
            {
                RectTransform buttonRect = child.GetComponent<RectTransform>();
                
                LayoutElement le = buttonRect.GetComponent<LayoutElement>();
                if (le != null) le.enabled = false;
                
                buttonRect.anchorMin = new Vector2(0, 1);
                buttonRect.anchorMax = new Vector2(1, 1);
                buttonRect.pivot = new Vector2(0.5f, 1);
                
                buttonRect.sizeDelta = new Vector2(0, buttonHeight);
                
                buttonRect.anchoredPosition = new Vector2(0, -buttonIndex * buttonHeight);
                
                LayoutRebuilder.ForceRebuildLayoutImmediate(buttonRect);
                
                buttonIndex++;
            }
        }
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(templateRect);
    }
    
    void Update()
    {
        if (!ShouldProcessScroll())
            return;
            
        wasScrollingLastFrame = isScrolling;
        GetThumbstickInput();
        ProcessScrolling();
        
        if (useSmoothing)
        {
            ApplySmoothScrolling();
        }
    }
    
    private void CalculateScrollBounds()
    {
        totalButtons = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).GetComponent<Button>() != null)
            {
                totalButtons++;
            }
        }
        
        templateHeight = templateRect.rect.height;
        
        visibleButtons = Mathf.FloorToInt(templateHeight / buttonHeight);
        
        if (totalButtons > visibleButtons)
        {
            maxScrollDistance = (totalButtons - visibleButtons) * buttonHeight;
        }
        else
        {
            maxScrollDistance = 0f;
        }
        
        Debug.Log($"Template Height: {templateHeight}, Button Height: {buttonHeight}, Total Buttons: {totalButtons}, Visible: {visibleButtons}");
    }
    
    private bool ShouldProcessScroll()
    {
        if (!OVRManager.isHmdPresent) return false;
        
        if (totalButtons <= visibleButtons) return false;
        
        if (onlyScrollWhenUIFocused)
        {
            if (dropdownObject != null && !dropdownObject.activeInHierarchy)
                return false;
        }
        
        return true;
    }
    
    private void GetThumbstickInput()
    {
        Vector2 thumbstickValue = Vector2.zero;
        
        if (useBothControllers)
        {
            Vector2 rightInput = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch);
            Vector2 leftInput = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.LTouch);
            thumbstickValue = (rightInput.magnitude > leftInput.magnitude) ? rightInput : leftInput;
        }
        else if (useRightController)
        {
            thumbstickValue = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch);
        }
        else
        {
            thumbstickValue = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.LTouch);
        }
        
        currentThumbstickInput = thumbstickValue;
    }
    
    private void ProcessScrolling()
    {
        if (Mathf.Abs(currentThumbstickInput.y) < deadZone)
        {
            isScrolling = false;
            return;
        }
        
        isScrolling = true;
        
        if (enableHapticFeedback && isScrolling && !wasScrollingLastFrame)
        {
            TriggerHapticFeedback();
        }
        
        float scrollInput = currentThumbstickInput.y;
        if (invertScrollDirection)
            scrollInput = -scrollInput;
        
        float scrollAmount = -scrollInput * scrollSpeed * Time.deltaTime;
        
        if (useSmoothing)
        {
            currentScrollOffset += scrollAmount;
            currentScrollOffset = Mathf.Clamp(currentScrollOffset, 0f, maxScrollDistance);
            targetPosition.y = -currentScrollOffset;
        }
        else
        {
            currentScrollOffset += scrollAmount;
            currentScrollOffset = Mathf.Clamp(currentScrollOffset, 0f, maxScrollDistance);
            ApplyDirectScroll();
        }
    }
    
    private void ApplyDirectScroll()
    {
        int buttonIndex = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.GetComponent<Button>() != null)
            {
                RectTransform buttonRect = child.GetComponent<RectTransform>();
                
                float yPos = -buttonIndex * buttonHeight + currentScrollOffset;
                buttonRect.anchoredPosition = new Vector2(0, yPos);
                
                buttonIndex++;
            }
        }
    }
    
    private void ApplySmoothScrolling()
    {
        int buttonIndex = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.GetComponent<Button>() != null)
            {
                RectTransform buttonRect = child.GetComponent<RectTransform>();
                Vector2 currentPos = buttonRect.anchoredPosition;
                
                float targetY = -buttonIndex * buttonHeight + currentScrollOffset;
                Vector2 targetPos = new Vector2(0, targetY);
                Vector2 smoothedPos = Vector2.Lerp(currentPos, targetPos, Time.deltaTime * smoothingSpeed);
                
                buttonRect.anchoredPosition = smoothedPos;
                buttonIndex++;
            }
        }
    }
    
    private void TriggerHapticFeedback()
    {
        OVRInput.Controller activeController = useRightController ? OVRInput.Controller.RTouch : OVRInput.Controller.LTouch;
        
        if (useBothControllers)
        {
            OVRInput.SetControllerVibration(hapticIntensity, hapticIntensity, OVRInput.Controller.LTouch);
            OVRInput.SetControllerVibration(hapticIntensity, hapticIntensity, OVRInput.Controller.RTouch);
            Invoke(nameof(StopHapticFeedback), hapticDuration);
        }
        else
        {
            OVRInput.SetControllerVibration(hapticIntensity, hapticIntensity, activeController);
            Invoke(nameof(StopHapticFeedback), hapticDuration);
        }
    }
    
    private void StopHapticFeedback()
    {
        if (useBothControllers)
        {
            OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
            OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
        }
        else
        {
            OVRInput.Controller activeController = useRightController ? OVRInput.Controller.RTouch : OVRInput.Controller.LTouch;
            OVRInput.SetControllerVibration(0, 0, activeController);
        }
    }
    
    public void SetScrollSpeed(float newSpeed) => scrollSpeed = newSpeed;
    public void SetButtonHeight(float height) => buttonHeight = height;
    public void SetControllerPreference(bool useRight) => useRightController = useRight;
    public void SetSmoothScrolling(bool enableSmoothing) => useSmoothing = enableSmoothing;
    public void SetDeadZone(float newDeadZone) => deadZone = Mathf.Clamp01(newDeadZone);
    public bool IsCurrentlyScrolling() => isScrolling;
    public Vector2 GetCurrentThumbstickInput() => currentThumbstickInput;
    
    public void ScrollToTop()
    {
        currentScrollOffset = 0f;
        if (useSmoothing)
        {
            targetPosition.y = 0f;
        }
        else
        {
            ApplyDirectScroll();
        }
    }
    
    public void ScrollToBottom()
    {
        currentScrollOffset = maxScrollDistance;
        if (useSmoothing)
        {
            targetPosition.y = -maxScrollDistance;
        }
        else
        {
            ApplyDirectScroll();
        }
    }
    
    public void ScrollToButton(int buttonIndex)
    {
        float buttonPosition = buttonIndex * buttonHeight;
        float scrollNeeded = buttonPosition - (visibleButtons - 1) * buttonHeight;
        currentScrollOffset = Mathf.Clamp(scrollNeeded, 0f, maxScrollDistance);
        
        if (useSmoothing)
        {
            targetPosition.y = -currentScrollOffset;
        }
        else
        {
            ApplyDirectScroll();
        }
    }
    
    public void RefreshScrollBounds()
    {
        CalculateScrollBounds();
        InitializeButtonPositions();
        currentScrollOffset = 0f;
        if (useSmoothing)
            targetPosition = Vector3.zero;
    }
    
    public void OnDropdownOpened()
    {
        StartCoroutine(InitializeAfterFrame());
    }
    
    private System.Collections.IEnumerator InitializeAfterFrame()
    {
        yield return null;
        InitializeButtonPositions();
        targetPosition = Vector3.zero;
        currentScrollOffset = 0f;
    }
    
    void OnDestroy()
    {
        StopHapticFeedback();
    }
    
    void OnGUI()
    {
        if (!Application.isPlaying || !Debug.isDebugBuild) return;
        
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.fontSize = 14;
        
        GUI.Label(new Rect(10, 10, 400, 25), $"Thumbstick: {currentThumbstickInput}", style);
        GUI.Label(new Rect(10, 30, 400, 25), $"Scrolling: {isScrolling}", style);
        GUI.Label(new Rect(10, 50, 400, 25), $"Scroll Offset: {currentScrollOffset:F1}", style);
        GUI.Label(new Rect(10, 70, 400, 25), $"Buttons: {totalButtons} | Visible: {visibleButtons}", style);
        GUI.Label(new Rect(10, 90, 400, 25), $"Max Scroll: {maxScrollDistance:F1}", style);
    }
}