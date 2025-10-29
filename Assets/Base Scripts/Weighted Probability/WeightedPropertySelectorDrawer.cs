using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using Base_Scripts; // Namespace of your WeightedProbabilitySelector

[CustomPropertyDrawer(typeof(WeightedProbabilitySelector<>))]
public class WeightedProbabilitySelectorDrawer : PropertyDrawer
{
    private const float TOTAL_WEIGHT = 100f;
    private const float FLOAT_COMPARISON_TOLERANCE = 0.01f; // For Mathf.Approximately

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty outcomesList = property.FindPropertyRelative("outcomes");

        // --- Draw Foldout ---
        position.height = EditorGUIUtility.singleLineHeight;
        property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label, true);
        position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        if (property.isExpanded && outcomesList != null && outcomesList.isArray)
        {
            EditorGUI.indentLevel++;

            // --- Draw List Size ---
            Rect listHeaderRect = position;
            listHeaderRect.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(listHeaderRect, outcomesList.FindPropertyRelative("Array.size"));
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            // --- Calculate Weights ---
            float currentTotalWeight = CalculateTotalWeight(outcomesList);
            float totalLockedWeight = CalculateLockedWeight(outcomesList);
            float totalUnlockedWeight = currentTotalWeight - totalLockedWeight;

            // --- Draw Total Label ---
            GUIStyle totalLabelStyle = new GUIStyle(EditorStyles.label);
            if (!Mathf.Approximately(currentTotalWeight, TOTAL_WEIGHT))
            {
                totalLabelStyle.normal.textColor = Color.yellow;
            }
            Rect totalRect = position;
            totalRect.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(totalRect, $"Total: {currentTotalWeight:F1}/{TOTAL_WEIGHT} (Locked: {totalLockedWeight:F1})", totalLabelStyle);
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;


            // --- Draw Elements ---
            if (outcomesList.isExpanded) // Check if list itself is expanded
            {
                EditorGUI.BeginChangeCheck(); // Watch for changes within the loop

                for (int i = 0; i < outcomesList.arraySize; i++)
                {
                    SerializedProperty element = outcomesList.GetArrayElementAtIndex(i);
                    SerializedProperty weightProp = element.FindPropertyRelative("Weight");
                    SerializedProperty lockedProp = element.FindPropertyRelative("IsLocked");

                    float elementHeight = EditorGUI.GetPropertyHeight(element, true);
                    Rect elementRect = position;
                    elementRect.height = elementHeight;

                    // --- Draw Element Fields ---
                    // Draw the default element fields (Outcome, Weight, IsLocked)
                    EditorGUI.PropertyField(elementRect, element, new GUIContent($"Element {i}"), true);

                    // --- Draw Percentage Label (Positioned better) ---
                    Rect percentageRect = elementRect;
                    // Try to align it after the Weight field
                    percentageRect.x += EditorGUIUtility.labelWidth + 155; // Adjust based on inspector width/look
                    percentageRect.width = 55;
                    percentageRect.height = EditorGUIUtility.singleLineHeight; // Only height of the label itself

                    string percentageString = "";
                    if (currentTotalWeight > FLOAT_COMPARISON_TOLERANCE && weightProp.floatValue > 0f)
                    {
                        percentageString = $" ({(weightProp.floatValue / currentTotalWeight * 100f):F1}%)";
                    }
                    EditorGUI.LabelField(percentageRect, percentageString);


                    position.y += elementHeight + EditorGUIUtility.standardVerticalSpacing;
                }

                if (EditorGUI.EndChangeCheck()) // If ANY value inside the list changed
                {
                    NormalizeWeights(outcomesList);
                    EditorUtility.SetDirty(property.serializedObject.targetObject);
                    // GUI.changed = true; // Let Unity handle repaint
                }
            } // end if list expanded
            EditorGUI.indentLevel--;
        } // end if property expanded

        EditorGUI.EndProperty();
    }

    // --- Helper Functions ---

    private float CalculateTotalWeight(SerializedProperty list)
    {
        float total = 0f;
        for (int i = 0; i < list.arraySize; i++)
        {
            total += list.GetArrayElementAtIndex(i).FindPropertyRelative("Weight").floatValue;
        }
        return total;
    }

    private float CalculateLockedWeight(SerializedProperty list)
    {
        float total = 0f;
        for (int i = 0; i < list.arraySize; i++)
        {
            SerializedProperty element = list.GetArrayElementAtIndex(i);
            if (element.FindPropertyRelative("IsLocked").boolValue)
            {
                total += element.FindPropertyRelative("Weight").floatValue;
            }
        }
        return total;
    }

    private void NormalizeWeights(SerializedProperty list)
    {
        float totalLockedWeight = CalculateLockedWeight(list);

        // Ensure locked weights don't exceed the total
        if (totalLockedWeight > TOTAL_WEIGHT)
        {
            Debug.LogWarning("Locked weights exceed total weight. Normalization might behave unexpectedly. Please unlock some weights.");
            // Optionally, we could force-unlock items or clamp locked values, but just warning is safer.
            totalLockedWeight = TOTAL_WEIGHT; // Cap for calculation below
        }

        float availableWeightForUnlocked = TOTAL_WEIGHT - totalLockedWeight;
        if (availableWeightForUnlocked < 0) availableWeightForUnlocked = 0; // Should not happen with the check above, but safety

        // Calculate the current sum of weights ONLY for unlocked items
        float currentUnlockedSum = 0f;
        List<int> unlockedIndices = new List<int>();
        for (int i = 0; i < list.arraySize; i++)
        {
            SerializedProperty element = list.GetArrayElementAtIndex(i);
            if (!element.FindPropertyRelative("IsLocked").boolValue)
            {
                currentUnlockedSum += element.FindPropertyRelative("Weight").floatValue;
                unlockedIndices.Add(i);
            }
        }

        // If there are unlocked items and their sum needs adjustment
        if (unlockedIndices.Count > 0 && !Mathf.Approximately(currentUnlockedSum, availableWeightForUnlocked))
        {
            float factor = 1.0f;
            // Avoid division by zero if all unlocked weights were zero initially
            if (currentUnlockedSum > FLOAT_COMPARISON_TOLERANCE)
            {
                factor = availableWeightForUnlocked / currentUnlockedSum;
            }
            else
            {
                // If current sum is zero, distribute available weight equally
                factor = 0; // Prevent multiplication below
                float equalShare = availableWeightForUnlocked / unlockedIndices.Count;
                 foreach (int index in unlockedIndices)
                 {
                     list.GetArrayElementAtIndex(index).FindPropertyRelative("Weight").floatValue = equalShare;
                 }
                 // Apply changes and exit normalization early
                 list.serializedObject.ApplyModifiedProperties();
                 return;
            }


            // Apply the factor to all unlocked items
            foreach (int index in unlockedIndices)
            {
                SerializedProperty weightProp = list.GetArrayElementAtIndex(index).FindPropertyRelative("Weight");
                // Multiply, ensuring result is not negative due to float inaccuracies
                weightProp.floatValue = Mathf.Max(0f, weightProp.floatValue * factor);
            }

            // --- Final Check & Adjustment (for floating point drift) ---
            // Recalculate the sum AFTER scaling
            float finalUnlockedSum = 0f;
            foreach (int index in unlockedIndices) { finalUnlockedSum += list.GetArrayElementAtIndex(index).FindPropertyRelative("Weight").floatValue;}

            float difference = availableWeightForUnlocked - finalUnlockedSum;

            // If there's a small difference due to floating point math, add it to the first unlocked item
            if (unlockedIndices.Count > 0 && !Mathf.Approximately(difference, 0f))
            {
                 SerializedProperty firstUnlockedWeight = list.GetArrayElementAtIndex(unlockedIndices[0]).FindPropertyRelative("Weight");
                 firstUnlockedWeight.floatValue = Mathf.Max(0f, firstUnlockedWeight.floatValue + difference);
            }

        }
        // Apply changes made to the properties
        list.serializedObject.ApplyModifiedProperties();
    }


    // --- GetPropertyHeight (Adjust slightly for Total label) ---
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float totalHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // Foldout

        if (property.isExpanded)
        {
            SerializedProperty outcomesList = property.FindPropertyRelative("outcomes");
            if (outcomesList != null && outcomesList.isArray)
            {
                totalHeight += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // Size Liste
                totalHeight += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // Label Total/Locked

                if (outcomesList.isExpanded)
                {
                    for (int i = 0; i < outcomesList.arraySize; i++)
                    {
                        totalHeight += EditorGUI.GetPropertyHeight(outcomesList.GetArrayElementAtIndex(i), true) + EditorGUIUtility.standardVerticalSpacing;
                    }
                }
            }
        }
        return totalHeight;
    }
}