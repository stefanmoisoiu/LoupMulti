using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PStaminaBar : PNetworkBehaviour
{
    [SerializeField] private PStamina stamina;
    [SerializeField] private GameObject sliderPrefab;
    private List<Slider> _staminaSliders = new List<Slider>();
    private List<Image> _staminaFillImages = new List<Image>();

    [SerializeField] private Color fillingColor,graceColor,fullColor;
    
    
    
    private GameObject _staminaBarParent;
    private const string StaminaSliderTag = "StaminaBar";

    protected override void StartAnyOwner()
    {
        _staminaBarParent = GameObject.FindGameObjectWithTag(StaminaSliderTag);
        stamina.UpdatedStaminaParts += UpdateStaminaParts;
        InitializeStaminaParts();
    }

    protected override void DisableAnyOwner()
    {
        stamina.UpdatedStaminaParts -= UpdateStaminaParts;
    }

    protected override void UpdateAnyOwner()
    {
        // Filling the stamina bar
        for (int i = 0; i < _staminaSliders.Count; i++)
        {
            float upperBound = stamina.StaminaPerPart * (i + 1);
            float lowerBound = stamina.StaminaPerPart * i;
            _staminaSliders[i].value =
                Mathf.InverseLerp(lowerBound, upperBound, stamina.Stamina);
        }
        
        // Coloring the stamina bar
        for (int i = 0; i < _staminaSliders.Count; i++)
        {
            float upperBound = stamina.StaminaPerPart * (i + 1);
            float lowerBound = stamina.StaminaPerPart * i;
            if (stamina.Stamina >= upperBound)
            {
                _staminaFillImages[i].color = fullColor;
            }
            else if (stamina.InGrace(stamina.Stamina, lowerBound, upperBound))
            {
                _staminaFillImages[i].color = graceColor;
            }
            else
            {
                _staminaFillImages[i].color = fillingColor;
            }
        }
    }

    private void UpdateStaminaParts(int parts)
    {
        if (parts < _staminaSliders.Count)
        {
            for (int i = parts; i < _staminaSliders.Count; i++)
            {
                Destroy(_staminaSliders[i].gameObject);
            }
            _staminaSliders.RemoveRange(parts, _staminaSliders.Count - parts);
            _staminaFillImages.RemoveRange(parts, _staminaFillImages.Count - parts);
        }
        else if (parts > _staminaSliders.Count)
        {
            for (int i = _staminaSliders.Count; i < parts; i++)
            {
                GameObject slider = Instantiate(sliderPrefab, _staminaBarParent.transform);
                _staminaSliders.Add(slider.GetComponent<Slider>());
                _staminaFillImages.Add(slider.transform.GetChild(1).GetChild(0).GetComponent<Image>());
            }
        }
    }
    
    private void InitializeStaminaParts() => UpdateStaminaParts(stamina.StaminaPartCount);
}
