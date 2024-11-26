using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class PStyle : PNetworkBehaviour
{
    private ushort _currentStylePoints;
    public NoLookStyle[] noLookStyles;
    public RotationStyle[] rotationStyles;

    private float _currentRotation;
    private float _currentNoLookTime;
    
    private int _currentNoLookStyleIndex = -1;
    private int _currentRotationStyleIndex = -1;
    
    [SerializeField] private PGrounded grounded;
    [SerializeField] private PCamera cam;
    [SerializeField] private PMovement movement;
    [SerializeField] private PTextScreenPopup textScreenPopup;

    [SerializeField] private float popupRotationRange = 30;
    [SerializeField] private Bounds popupPositionBounds;
    
    

    private PMovement.MoveSpeedModifiers _noLookSpeedModifier = new(1,1);
    private PMovement.MoveSpeedModifiers _rotationSpeedModifier = new(1,1);
    
    private Coroutine _noLookCoroutine;
    private Coroutine _rotationCoroutine;

    private CanvasGroup noLookPanel;
    [SerializeField] private float noLookPanelLerpDuration = 0.5f;
    [SerializeField] private AnimationCurve startNoLookPanelCurve;
    [SerializeField] private AnimationCurve stopNoLookPanelCurve;
    private Coroutine _noLookPanelCoroutine;
    private const string NoLookPanelTag = "NoLookPanel";
    
    
    public bool NoLook { get; private set; }

    protected override void StartAnyOwner()
    {
        InputManager.instance.OnAction1 += StartNoLook;
        InputManager.instance.OnStopAction1 += StopNoLook;
        
        movement.AddMoveSpeedModifier(_noLookSpeedModifier);
        movement.AddMoveSpeedModifier(_rotationSpeedModifier);
        
        noLookPanel = GameObject.FindGameObjectWithTag(NoLookPanelTag).GetComponent<CanvasGroup>();
    }

    protected override void DisableAnyOwner()
    {
        InputManager.instance.OnAction1 -= StartNoLook;
        InputManager.instance.OnStopAction1 -= StopNoLook;
    }

    private void StartNoLook()
    {
        NoLook = true;
        
        if (_noLookPanelCoroutine != null) StopCoroutine(_noLookPanelCoroutine);
        _noLookPanelCoroutine = StartCoroutine(LerpNoLookPanel(true));
    }
    private void StopNoLook()
    {
        NoLook = false;
        
        if (_noLookPanelCoroutine != null) StopCoroutine(_noLookPanelCoroutine);
        _noLookPanelCoroutine = StartCoroutine(LerpNoLookPanel(false));
    }

    private IEnumerator LerpNoLookPanel(bool start)
    {
        float t = 0;
        float startAlpha = noLookPanel.alpha;
        float endAlpha = start ? 1 : 0;
        
        while (t < noLookPanelLerpDuration)
        {
            t += Time.deltaTime;
            float adv = t / noLookPanelLerpDuration;
            noLookPanel.alpha = Mathf.Lerp(startAlpha, endAlpha, start ? startNoLookPanelCurve.Evaluate(adv) : stopNoLookPanelCurve.Evaluate(adv));
            yield return null;
        }
        
        noLookPanel.alpha = endAlpha;
    }
    protected override void UpdateAnyOwner()
    {
        UpdateNoLook();
        UpdateRotation();
    }
    private IEnumerator BoostSpeed(PMovement.MoveSpeedModifiers speedModifier, Style style)
    {
        speedModifier.maxSpeedFactor = style.maxSpeedFactor;
        speedModifier.accelerationFactor = style.accelerationFactor;

        float t = 0;
        
        while (t < style.boostDuration)
        {
            t += Time.deltaTime;
            float adv = Mathf.Pow(t / style.boostDuration, 2);
            
            speedModifier.maxSpeedFactor = Mathf.Lerp(style.maxSpeedFactor, 1, adv);
            speedModifier.accelerationFactor = Mathf.Lerp(style.accelerationFactor, 1, adv);
            
            yield return null;
        }
        
        speedModifier.maxSpeedFactor = 1;
        speedModifier.accelerationFactor = 1;
    }
    
    private void UpdateNoLook()
    {
        if (NoLook)
        {
            _currentNoLookTime += Time.deltaTime;

            int noLookStyleIndex = -1;

            for (int i = 0; i < noLookStyles.Length; i++)
            {
                bool isLast = i == noLookStyles.Length - 1;

                bool myStyleReached = noLookStyles[i].StyleReached(_currentNoLookTime);
                bool nextStyleReached = !isLast && noLookStyles[i + 1].StyleReached(_currentNoLookTime);
                if (myStyleReached && !nextStyleReached)
                {
                    noLookStyleIndex = i;
                    break;
                }
            }
            
            if (noLookStyleIndex != -1 && _currentNoLookStyleIndex != noLookStyleIndex)
            {
                textScreenPopup.CreatePopup(noLookStyles[noLookStyleIndex].popupData,Vector2.zero);
            }
            
            _currentNoLookStyleIndex = noLookStyleIndex;
        }
        else
        {
            _currentNoLookTime = 0;
            
            if (_currentNoLookStyleIndex != -1 && grounded.FullyGrounded())
            {
                if (_noLookCoroutine != null) StopCoroutine(_noLookCoroutine);
                _noLookCoroutine = StartCoroutine(BoostSpeed(_noLookSpeedModifier, noLookStyles[_currentNoLookStyleIndex]));
            }
            
            _currentNoLookStyleIndex = -1;
        }
    }
    private void UpdateRotation()
    {
        if (!grounded.FullyGrounded())
        {
            _currentRotation += cam.LookDelta.x;

            int rotationStyleIndex = -1;
            
            for (int i = 0; i < rotationStyles.Length; i++)
            {
                bool isLast = i == rotationStyles.Length - 1;
                
                float absCurrentRotation = Mathf.Abs(_currentRotation);
                bool myStyleReached = rotationStyles[i].StyleReached(absCurrentRotation);
                bool nextStyleReached = !isLast && rotationStyles[i + 1].StyleReached(absCurrentRotation);
                if (myStyleReached && !nextStyleReached)
                {
                    rotationStyleIndex = i;
                    break;
                }
            }

            if (rotationStyleIndex != -1 && _currentRotationStyleIndex != rotationStyleIndex)
            {
                float rotation = UnityEngine.Random.Range(-popupRotationRange, popupRotationRange);
                Vector3 position = new Vector3(UnityEngine.Random.Range(popupPositionBounds.min.x, popupPositionBounds.max.x),
                    UnityEngine.Random.Range(popupPositionBounds.min.y, popupPositionBounds.max.y), 0);
                textScreenPopup.CreatePopup(rotationStyles[rotationStyleIndex].popupData,position,rotation);
            }
            
            _currentRotationStyleIndex = rotationStyleIndex;
        }
        else
        {
            _currentRotation = 0;
            
            if (_currentRotationStyleIndex != -1)
            {
                if (_rotationCoroutine != null) StopCoroutine(_rotationCoroutine);
                _rotationCoroutine = StartCoroutine(BoostSpeed(_rotationSpeedModifier, rotationStyles[_currentRotationStyleIndex]));
            }
            
            _currentRotationStyleIndex = -1;
        }
    }
    [Serializable]
    public class NoLookStyle : Style
    {
        public float noLookTime;
        public bool StyleReached(float currentTime) => currentTime >= noLookTime;
    }
    [Serializable]
    public class RotationStyle : Style
    {
        public int rotation;
        public bool StyleReached(float currentRotation) => currentRotation >= rotation;
    }

    public abstract class Style
    {
        public PTextScreenPopup.PopupData popupData;
        
        public float maxSpeedFactor;
        public float accelerationFactor;
        public float boostDuration;
    }
}