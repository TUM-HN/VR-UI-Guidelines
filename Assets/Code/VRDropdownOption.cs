using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VRDropdownOption : MonoBehaviour
{
    public TextMeshProUGUI labelText;
    public GameObject templatePanel;
    
    [Header("Text Styling")]
    public Color selectedTextColor = Color.white;
    public Color defaultTextColor = Color.gray;
    
    public void SetOption(string optionText)
    {
        labelText.text = optionText;
        
        labelText.color = selectedTextColor;
        
        labelText.fontStyle = FontStyles.Normal;
        
        templatePanel.SetActive(false);
    }
}
