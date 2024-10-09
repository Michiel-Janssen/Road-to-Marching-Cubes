using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BrushSettingsUI : MonoBehaviour
{
    [SerializeField] private GameObject UI_Panel;
    [SerializeField] private TMP_Text brushSizeText;
    [SerializeField] private Slider brushSizeSlider;
    [SerializeField] private TMP_Text brushStrengthText;
    [SerializeField] private Slider brushStrengthSlider;
    [SerializeField] private Toggle visualToggle;
    private bool isOpen;

    private void Start()
    {
        UpdateValueTextSize(TerraformingCamera.Instance.BrushSize);
        brushSizeSlider.value = TerraformingCamera.Instance.BrushSize;
        UpdateValueTextStrength(TerraformingCamera.Instance.BrushStrength);
        brushStrengthSlider.value = TerraformingCamera.Instance.BrushStrength;

        brushSizeSlider.onValueChanged.AddListener(OnBrushSizeSliderValueChanged);
        brushStrengthSlider.onValueChanged.AddListener(OnBrushStrengthSliderValueChanged);
        visualToggle.onValueChanged.AddListener(OnVisualToggleValueChanged);
    }

    private void OnVisualToggleValueChanged(bool value)
    {
        TerraformingCamera.Instance.UseVisual = value;
    }

    private void OnBrushSizeSliderValueChanged(float value)
    {
        UpdateValueTextSize(value);
        TerraformingCamera.Instance.BrushSize = value;
    }

    private void UpdateValueTextSize(float value)
    {
        brushSizeText.text = "Value: " + value.ToString("F2");
    }

    private void OnBrushStrengthSliderValueChanged(float value)
    {
        UpdateValueTextStrength(value);
        TerraformingCamera.Instance.BrushStrength = value;
    }

    private void UpdateValueTextStrength(float value)
    {
        brushStrengthText.text = "Value: " + value.ToString("F2");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (isOpen)
            {
                FlyCamera.Instance.inUI = false;
                FlyCamera.Instance.SetFocus(true);
                UI_Panel.SetActive(false);
            }
            else
            {
                FlyCamera.Instance.inUI = true;
                FlyCamera.Instance.SetFocus(false);
                UI_Panel.SetActive(true);
            }
            isOpen = !isOpen;
        }
    }
}
