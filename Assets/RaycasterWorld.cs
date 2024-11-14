using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RaycasterWorld : GraphicRaycaster
{
    public float hitDistance = -1;

    // This modification operates under the assumption that this script is only being attached to a World Space canvas - it might break otherwise.
    public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
    {
        //Set middle screen pos or you can set variable on start and use it
        eventData.position = new(Screen.width / 2, Screen.height / 2);
        bool fail = false;

        if (hitDistance > 0 && eventCamera != null)
        {
            fail = true;
            int displayIndex = eventCamera.targetDisplay;

            var canvasGraphics = GraphicRegistry.GetRaycastableGraphicsForCanvas(canvas);
            if (canvasGraphics == null || canvasGraphics.Count == 0)
                return;


            var eventPosition = RelativeMouseAtScaled(eventData.position);
            if (eventPosition != Vector3.zero)
            {
                // We support multiple display and display identification based on event position.

                int eventDisplayIndex = (int)eventPosition.z;

                // Discard events that are not part of this display so the user does not interact with multiple displays at once.
                if (eventDisplayIndex != displayIndex)
                    return;
            }
            else
            {
                // The multiple display system is not supported on all platforms, when it is not supported the returned position
                // will be all zeros so when the returned index is 0 we will default to the event data to be safe.
                eventPosition = eventData.position;

#if UNITY_EDITOR
                if (Display.activeEditorGameViewTarget != displayIndex)
                    return;
                eventPosition.z = Display.activeEditorGameViewTarget;
#endif

                // We dont really know in which display the event occured. We will process the event assuming it occured in our display.
            }

            var ray = eventCamera.ScreenPointToRay(eventPosition);

            m_RaycastResults.Clear();

            Raycast(canvas, eventCamera, eventPosition, canvasGraphics, m_RaycastResults);

            int totalCount = m_RaycastResults.Count;
            for (var index = 0; index < totalCount; index++)
            {
                var go = m_RaycastResults[index].gameObject;
                bool appendGraphic = true;

                if (ignoreReversedGraphics)
                {
                    // If we have a camera compare the direction against the cameras forward.
                    var cameraForward = eventCamera.transform.rotation * Vector3.forward * eventCamera.nearClipPlane;
                    appendGraphic = Vector3.Dot(go.transform.position - eventCamera.transform.position - cameraForward, go.transform.forward) >= 0;
                }

                if (appendGraphic)
                {
                    float distance = 0;
                    Transform trans = go.transform;
                    Vector3 transForward = trans.forward;

                    // http://geomalgorithms.com/a06-_intersect-2.html
                    distance = (Vector3.Dot(transForward, trans.position - ray.origin) / Vector3.Dot(transForward, ray.direction));

                    // Check to see if the go is behind the camera.
                    if (distance < 0)
                        continue;

                    if (distance >= hitDistance)
                        continue;

                    fail = false;
                    break;
                }
            }
        }
        if (!fail)
        {
            base.Raycast(eventData, resultAppendList);
        }
    }

    [NonSerialized] private List<Graphic> m_RaycastResults = new List<Graphic>();

    [NonSerialized] static readonly List<Graphic> s2_SortedGraphics = new List<Graphic>();

    private static void Raycast(Canvas canvas, Camera eventCamera, Vector2 pointerPosition, IList<Graphic> foundGraphics, List<Graphic> results)
    {
        // Necessary for the event system
        int totalCount = foundGraphics.Count;
        for (int i = 0; i < totalCount; ++i)
        {
            Graphic graphic = foundGraphics[i];

            // -1 means it hasn't been processed by the canvas, which means it isn't actually drawn
            if (!graphic.raycastTarget || graphic.canvasRenderer.cull || graphic.depth == -1)
                continue;

            if (!RectTransformUtility.RectangleContainsScreenPoint(graphic.rectTransform, pointerPosition, eventCamera, graphic.raycastPadding))
                continue;

            if (eventCamera != null && eventCamera.WorldToScreenPoint(graphic.rectTransform.position).z > eventCamera.farClipPlane)
                continue;

            if (graphic.Raycast(pointerPosition, eventCamera))
            {
                s2_SortedGraphics.Add(graphic);
            }
        }

        s2_SortedGraphics.Sort((g1, g2) => g2.depth.CompareTo(g1.depth));
        totalCount = s2_SortedGraphics.Count;
        for (int i = 0; i < totalCount; ++i)
            results.Add(s2_SortedGraphics[i]);

        s2_SortedGraphics.Clear();
    }


    /// <summary>
    /// A version of Display.RelativeMouseAt that scales the position when the main display has a different rendering resolution to the system resolution.
    /// By default, the mouse position is relative to the main render area, we need to adjust this so it is relative to the system resolution
    /// in order to correctly determine the position on other displays.
    /// </summary>
    /// <returns></returns>
    public static Vector3 RelativeMouseAtScaled(Vector2 position)
    {
#if !UNITY_EDITOR && !UNITY_WSA
            // If the main display is now the same resolution as the system then we need to scale the mouse position. (case 1141732)
            if (Display.main.renderingWidth != Display.main.systemWidth || Display.main.renderingHeight != Display.main.systemHeight)
            {
                // The system will add padding when in full-screen and using a non-native aspect ratio. (case UUM-7893)
                // For example Rendering 1920x1080 with a systeem resolution of 3440x1440 would create black bars on each side that are 330 pixels wide.
                // we need to account for this or it will offset our coordinates when we are not on the main display.
                var systemAspectRatio = Display.main.systemWidth / (float)Display.main.systemHeight;

                var sizePlusPadding = new Vector2(Display.main.renderingWidth, Display.main.renderingHeight);
                var padding = Vector2.zero;
                if (Screen.fullScreen)
                {
                    var aspectRatio = Screen.width / (float)Screen.height;
                    if (Display.main.systemHeight * aspectRatio < Display.main.systemWidth)
                    {
                        // Horizontal padding
                        sizePlusPadding.x = Display.main.renderingHeight * systemAspectRatio;
                        padding.x = (sizePlusPadding.x - Display.main.renderingWidth) * 0.5f;
                    }
                    else
                    {
                        // Vertical padding
                        sizePlusPadding.y = Display.main.renderingWidth / systemAspectRatio;
                        padding.y = (sizePlusPadding.y - Display.main.renderingHeight) * 0.5f;
                    }
                }

                var sizePlusPositivePadding = sizePlusPadding - padding;

                // If we are not inside of the main display then we must adjust the mouse position so it is scaled by
                // the main display and adjusted for any padding that may have been added due to different aspect ratios.
                if (position.y < -padding.y || position.y > sizePlusPositivePadding.y ||
                     position.x < -padding.x || position.x > sizePlusPositivePadding.x)
                {
                    var adjustedPosition = position;

                    if (!Screen.fullScreen)
                    {
                        // When in windowed mode, the window will be centered with the 0,0 coordinate at the top left, we need to adjust so it is relative to the screen instead.
                        adjustedPosition.x -= (Display.main.renderingWidth - Display.main.systemWidth) * 0.5f;
                        adjustedPosition.y -= (Display.main.renderingHeight - Display.main.systemHeight) * 0.5f;
                    }
                    else
                    {
                        // Scale the mouse position to account for the black bars when in a non-native aspect ratio.
                        adjustedPosition += padding;
                        adjustedPosition.x *= Display.main.systemWidth / sizePlusPadding.x;
                        adjustedPosition.y *= Display.main.systemHeight / sizePlusPadding.y;
                    }

                    var relativePos = Display.RelativeMouseAt(adjustedPosition);

                    // If we are not on the main display then return the adjusted position.
                    if (relativePos.z != 0)
                        return relativePos;
                }

                // We are using the main display.
                return new Vector3(position.x, position.y, 0);
            }
#endif
        return Display.RelativeMouseAt(position);
    }

    private Canvas m2_Canvas;

    private Canvas canvas
    {
        get
        {
            if (m2_Canvas != null)
                return m2_Canvas;

            m2_Canvas = GetComponent<Canvas>();
            return m2_Canvas;
        }
    }

}