using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PStaminaUI : PNetworkBehaviour
{
    [SerializeField] private PStamina stamina;
    [SerializeField] private GameObject sliderPrefab;
    private List<Slider> _staminaSliders = new();
    private List<Image> _staminaFillImages = new();

    [SerializeField] private Color fillingColor,graceColor,fullColor;
    [SerializeField] private PCanvas canvas;
    
    
    
    
    private Transform _staminaBarParent;
    private const string StaminaSliderTag = "StaminaBar";

    protected override void StartAnyOwner()
    {
        stamina.UpdatedStaminaParts += UpdateStaminaParts;
        _staminaBarParent = GameObject.FindGameObjectWithTag(StaminaSliderTag).transform;
        UpdateStaminaParts(stamina.StaminaPartCount);
    }

    protected override void DisableAnyOwner()
    {
        stamina.UpdatedStaminaParts -= UpdateStaminaParts;
        UpdateStaminaParts(0);
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
                if(_staminaSliders[i] != null) Destroy(_staminaSliders[i].gameObject);
            }
            _staminaSliders.RemoveRange(parts, _staminaSliders.Count - parts);
            _staminaFillImages.RemoveRange(parts, _staminaFillImages.Count - parts);
        }
        else if (parts > _staminaSliders.Count)
        {
            for (int i = _staminaSliders.Count; i < parts; i++)
            {
                GameObject slider = Instantiate(sliderPrefab, _staminaBarParent);
                _staminaSliders.Add(slider.GetComponent<Slider>());
                _staminaFillImages.Add(slider.transform.GetChild(1).GetChild(0).GetComponent<Image>());
            }
        }
    }
}
