// using System;
// using System.Collections;
// using Base_Scripts;
// using Game.Stats;
// using Player.Camera;
// using Player.Movement;
// using Player.Networking;
// using Player.UI;
// using UnityEngine;
// using Ability = Player.Networking.Ability;
//
// namespace Player.Abilities
// {
//     public class Style : Ability
//     {
//         public NoLookStyleProperties[] noLookStyles;
//         public RotationStyleProperties[] rotationStyles;
//
//         private float _currentRotation;
//         private float _currentNoLookTime;
//     
//         private int _currentNoLookStyleIndex = -1;
//         private int _currentRotationStyleIndex = -1;
//     
//         [SerializeField] private Grounded grounded;
//         [SerializeField] private Camera.PCamera cam;
//         [SerializeField] private Movement.Movement movement;
//         [SerializeField] private TextPopup textPopup;
//
//         [SerializeField] private float popupRotationRange = 30;
//         [SerializeField] private Bounds popupPositionBounds;
//     
//         [SerializeField] private PCanvas canvas;
//     
//     
//
//         private StatModifier<float>.ModifierComponent _noLookMaxSpeedModifier = new(1,0);
//         private StatModifier<float>.ModifierComponent _noLookAccelerationModifier = new(1,0);
//         private StatModifier<float>.ModifierComponent _rotationMaxSpeedModifier = new(1,0);
//         private StatModifier<float>.ModifierComponent _rotationAccelerationModifier = new(1,0);
//     
//         private Coroutine _noLookCoroutine;
//         private Coroutine _rotationCoroutine;
//
//         private CanvasGroup noLookPanel;
//         [SerializeField] private float noLookPanelLerpDuration = 0.5f;
//         [SerializeField] private AnimationCurve startNoLookPanelCurve;
//         [SerializeField] private AnimationCurve stopNoLookPanelCurve;
//         private Coroutine _noLookPanelCoroutine;
//         private const string NoLookPanelName = "NoLookPanel";
//     
//     
//         public bool NoLook { get; private set; }
//     
//         public override void EnableAbility()
//         {
//             base.EnableAbility();
//             //InputManager.instance.AddAbilityInputListener(AbilityInput, InputManager.ActionType.Start, StartNoLook);
//             //InputManager.instance.AddAbilityInputListener(AbilityInput, InputManager.ActionType.Stop, StopNoLook);
//         }
//
//         public override void DisableAbility()
//         {
//             base.DisableAbility();
//             //InputManager.instance.RemoveAbilityInputListener(AbilityInput, InputManager.ActionType.Start, StartNoLook);
//             //InputManager.instance.RemoveAbilityInputListener(AbilityInput, InputManager.ActionType.Stop, StopNoLook);
//         }
//
//         protected override void StartAnyOwner()
//         {
//         
//             noLookPanel = canvas.Canvas.transform.Find(NoLookPanelName).GetComponent<CanvasGroup>();
//         
//             movement.MaxSpeedModifier.AddModifier(_noLookMaxSpeedModifier);
//             movement.AccelerationModifier.AddModifier(_noLookMaxSpeedModifier);
//         
//             movement.MaxSpeedModifier.AddModifier(_rotationMaxSpeedModifier);
//             movement.AccelerationModifier.AddModifier(_rotationAccelerationModifier);
//         }
//         protected override void UpdateAnyOwner()
//         {
//             if (!AbilityEnabled) return;
//         
//             UpdateNoLook();
//             UpdateRotation();
//         }
//     
//         private void StartNoLook()
//         {
//             NoLook = true;
//         
//             if (_noLookPanelCoroutine != null) StopCoroutine(_noLookPanelCoroutine);
//             _noLookPanelCoroutine = StartCoroutine(LerpNoLookPanel(true));
//         }
//         private void StopNoLook()
//         {
//             NoLook = false;
//         
//             if (_noLookPanelCoroutine != null) StopCoroutine(_noLookPanelCoroutine);
//             _noLookPanelCoroutine = StartCoroutine(LerpNoLookPanel(false));
//         }
//
//         private IEnumerator LerpNoLookPanel(bool start)
//         {
//             float t = 0;
//             float startAlpha = noLookPanel.alpha;
//             float endAlpha = start ? 1 : 0;
//         
//             while (t < noLookPanelLerpDuration)
//             {
//                 t += Time.deltaTime;
//                 float adv = t / noLookPanelLerpDuration;
//                 noLookPanel.alpha = Mathf.Lerp(startAlpha, endAlpha, start ? startNoLookPanelCurve.Evaluate(adv) : stopNoLookPanelCurve.Evaluate(adv));
//                 yield return null;
//             }
//         
//             noLookPanel.alpha = endAlpha;
//         }
//         private IEnumerator BoostSpeed(StatModifier<float>.ModifierComponent maxSpeedModifier, StatModifier<float>.ModifierComponent accelModifier, StyleProperties styleProperties)
//         {
//             maxSpeedModifier.factor = styleProperties.maxSpeedFactor;
//             accelModifier.factor = styleProperties.accelerationFactor;
//
//             float t = 0;
//         
//             while (t < styleProperties.boostDuration)
//             {
//                 t += Time.deltaTime;
//                 float adv = Mathf.Pow(t / styleProperties.boostDuration, 2);
//             
//                 maxSpeedModifier.factor = Mathf.Lerp(styleProperties.maxSpeedFactor, 1, adv);
//                 accelModifier.factor = Mathf.Lerp(styleProperties.accelerationFactor, 1, adv);
//             
//                 yield return null;
//             }
//         
//             maxSpeedModifier.factor = 1;
//             accelModifier.factor = 1;
//         }
//     
//         private void UpdateNoLook()
//         {
//             if (NoLook)
//             {
//                 _currentNoLookTime += Time.deltaTime;
//
//                 int noLookStyleIndex = -1;
//
//                 for (int i = 0; i < noLookStyles.Length; i++)
//                 {
//                     bool isLast = i == noLookStyles.Length - 1;
//
//                     bool myStyleReached = noLookStyles[i].StyleReached(_currentNoLookTime);
//                     bool nextStyleReached = !isLast && noLookStyles[i + 1].StyleReached(_currentNoLookTime);
//                     if (myStyleReached && !nextStyleReached)
//                     {
//                         noLookStyleIndex = i;
//                         break;
//                     }
//                 }
//             
//                 if (noLookStyleIndex != -1 && _currentNoLookStyleIndex != noLookStyleIndex)
//                 {
//                     textPopup.CreatePopup(noLookStyles[noLookStyleIndex].popupData,Vector2.zero);
//                 }
//             
//                 _currentNoLookStyleIndex = noLookStyleIndex;
//             }
//             else
//             {
//                 _currentNoLookTime = 0;
//             
//                 if (_currentNoLookStyleIndex != -1 && grounded.FullyGrounded())
//                 {
//                     if (_noLookCoroutine != null) StopCoroutine(_noLookCoroutine);
//                     _noLookCoroutine = StartCoroutine(BoostSpeed(_noLookMaxSpeedModifier, _noLookAccelerationModifier, noLookStyles[_currentNoLookStyleIndex]));
//                 }
//             
//                 _currentNoLookStyleIndex = -1;
//             }
//         }
//         private void UpdateRotation()
//         {
//             if (!grounded.FullyGrounded())
//             {
//                 _currentRotation += cam.LookDelta.x;
//
//                 int rotationStyleIndex = -1;
//             
//                 for (int i = 0; i < rotationStyles.Length; i++)
//                 {
//                     bool isLast = i == rotationStyles.Length - 1;
//                 
//                     float absCurrentRotation = Mathf.Abs(_currentRotation);
//                     bool myStyleReached = rotationStyles[i].StyleReached(absCurrentRotation);
//                     bool nextStyleReached = !isLast && rotationStyles[i + 1].StyleReached(absCurrentRotation);
//                     if (myStyleReached && !nextStyleReached)
//                     {
//                         rotationStyleIndex = i;
//                         break;
//                     }
//                 }
//
//                 if (rotationStyleIndex != -1 && _currentRotationStyleIndex != rotationStyleIndex)
//                 {
//                     float rotation = UnityEngine.Random.Range(-popupRotationRange, popupRotationRange);
//                     Vector3 position = new Vector3(UnityEngine.Random.Range(popupPositionBounds.min.x, popupPositionBounds.max.x),
//                         UnityEngine.Random.Range(popupPositionBounds.min.y, popupPositionBounds.max.y), 0);
//                     textPopup.CreatePopup(rotationStyles[rotationStyleIndex].popupData,position,rotation);
//                 }
//             
//                 _currentRotationStyleIndex = rotationStyleIndex;
//             }
//             else
//             {
//                 _currentRotation = 0;
//             
//                 if (_currentRotationStyleIndex != -1)
//                 {
//                     if (_rotationCoroutine != null) StopCoroutine(_rotationCoroutine);
//                     _rotationCoroutine = StartCoroutine(BoostSpeed(_rotationMaxSpeedModifier, _rotationAccelerationModifier, rotationStyles[_currentRotationStyleIndex]));
//                 }
//             
//                 _currentRotationStyleIndex = -1;
//             }
//         }
//         [Serializable]
//         public class NoLookStyleProperties : StyleProperties
//         {
//             public float noLookTime;
//             public bool StyleReached(float currentTime) => currentTime >= noLookTime;
//         }
//         [Serializable]
//         public class RotationStyleProperties : StyleProperties
//         {
//             public int rotation;
//             public bool StyleReached(float currentRotation) => currentRotation >= rotation;
//         }
//
//         public abstract class StyleProperties
//         {
//             public TextPopup.PopupData popupData;
//         
//             public float maxSpeedFactor;
//             public float accelerationFactor;
//             public float boostDuration;
//         }
//     }
// }