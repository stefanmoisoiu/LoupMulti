using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PStaminaBar : MonoBehaviour
{
    [SerializeField] private Slider staminaBar;
    void Update()
    {
        staminaBar.value = PStamina.OwnerStamina / PStamina.MaxStamina;
    }
}
