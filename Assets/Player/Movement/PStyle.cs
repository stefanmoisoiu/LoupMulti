using System;
using TMPro;
using UnityEngine;

public class PStyle : PNetworkBehaviour
{
    private ushort _currentStylePoints;
    public NoLookStyle[] noLookStyles;
    public RotationStyle[] rotationStyles;
    
    private const string NoLookTextTag = "NoLookStyleText";
    private const string RotationTextTag = "RotationStyleText";
    private TMP_Text _noLookText, _rotationText;

    private float _currentRotation;
    private float _currentNoLookTime;
    
    private NoLookStyle _currentNoLookStyle;
    private RotationStyle _currentRotationStyle;

    [SerializeField] private AnimationClip changedStyleAnimation;
    [SerializeField] private AnimationClip stoppedStyleAnimation;

    private Animator _noLookAnimator;
    private Animator _rotationAnimator;

    [SerializeField] private PGrounded grounded;
    [SerializeField] private PCamera cam;
    
    public bool NoLook { get; private set; }

    protected override void StartAnyOwner()
    {
        InputManager.instance.OnAction1 += StartNoLook;
        InputManager.instance.OnStopAction1 += StopNoLook;

        GameObject noLook = GameObject.FindGameObjectWithTag(NoLookTextTag);
        _noLookText = noLook.transform.GetChild(0).GetComponent<TMP_Text>();
        _noLookAnimator = noLook.GetComponent<Animator>();
        
        GameObject rotation = GameObject.FindGameObjectWithTag(RotationTextTag);
        _rotationText = rotation.transform.GetChild(0).GetComponent<TMP_Text>();
        _rotationAnimator = rotation.GetComponent<Animator>();
    }

    protected override void DisableAnyOwner()
    {
        InputManager.instance.OnAction1 -= StartNoLook;
        InputManager.instance.OnStopAction1 -= StopNoLook;
    }

    private void StartNoLook()
    {
        NoLook = true;
    }
    private void StopNoLook()
    {
        NoLook = false;
    }

    protected override void UpdateAnyOwner()
    {
        if (NoLook)
        {
            _currentNoLookTime += Time.deltaTime;

            NoLookStyle noLookStyle = null;
            
            for (int i = 0; i < noLookStyles.Length; i++)
            {
                bool isLast = i == noLookStyles.Length - 1;
                bool myStyleReached = noLookStyles[i].StyleReached(_currentNoLookTime);
                bool nextStyleReached = !isLast && noLookStyles[i + 1].StyleReached(_currentNoLookTime);
                if (myStyleReached && !nextStyleReached)
                {
                    noLookStyle = noLookStyles[i];
                    break;
                }
            }
            
            if (noLookStyle != null)
            {
                if (_currentNoLookStyle != noLookStyle)
                {;
                    _noLookAnimator.Play(changedStyleAnimation.name);
                    noLookStyle.SetTextToStyle(_noLookText);
                }
                
            }
            else if (_currentNoLookStyle != null)
            {
                _noLookAnimator.Play(stoppedStyleAnimation.name);
            }
            _currentNoLookStyle = noLookStyle;
        }
        else
        {
            _currentNoLookTime = 0;
            
            if (_currentNoLookStyle != null)
            {
                _noLookAnimator.Play(stoppedStyleAnimation.name);
            }
            
            _currentNoLookStyle = null;
        }
        
        
        
        if (!grounded.FullyGrounded())
        {
            _currentRotation += cam.LookDelta.x;

            RotationStyle rotationStyle = null;
            
            for (int i = 0; i < rotationStyles.Length; i++)
            {
                bool isLast = i == rotationStyles.Length - 1;
                float absCurrentRotation = Mathf.Abs(_currentRotation);
                bool myStyleReached = rotationStyles[i].StyleReached(absCurrentRotation);
                bool nextStyleReached = !isLast && rotationStyles[i + 1].StyleReached(absCurrentRotation);
                if (myStyleReached && !nextStyleReached)
                {
                    rotationStyle = rotationStyles[i];
                    break;
                }
            }
            
            if (rotationStyle != null)
            {
                if (_currentRotationStyle != rotationStyle)
                {
                    _rotationAnimator.Play(changedStyleAnimation.name);
                    rotationStyle.SetTextToStyle(_rotationText);
                }
                
            }
            else if (_currentRotationStyle != null)
            {
                _rotationAnimator.Play(stoppedStyleAnimation.name);
            }
            _currentRotationStyle = rotationStyle;
        }
        else
        {
            _currentRotation = 0;
            
            if (_currentRotationStyle != null)
            {
                _rotationAnimator.Play(stoppedStyleAnimation.name);
            }
            
            _currentRotationStyle = null;
        }
    }

    [Serializable]
    public class NoLookStyle : Style
    {
        public float time;
        public bool StyleReached(float currentTime) => currentTime >= time;
    }
    [Serializable]
    public class RotationStyle : Style
    {
        public int rotation;
        public bool StyleReached(float currentRotation) => currentRotation >= rotation;
    }

    public abstract class Style
    {
        public string name;
        public Color color = Color.white;
        public int fontSize = 32;

        public void SetTextToStyle(TMP_Text text)
        {
            text.color = color;
            text.fontSize = fontSize;
            text.text = name;
        }

        public Style()
        {
            name = "Default";
            color = Color.white;
            fontSize = 32;
        }
    }
}