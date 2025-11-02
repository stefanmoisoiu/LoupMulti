#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Base_Scripts;

[CustomPropertyDrawer(typeof(WeightedProbabilitySelector<>))]
public class WeightedProbabilitySelectorDrawer : PropertyDrawer
{
    private const float TotalWeight = 100f;
    private const float FloatComparisonTolerance = 0.01f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty outcomesList = property.FindPropertyRelative("outcomes");

        position.height = EditorGUIUtility.singleLineHeight;
        property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label, true);
        position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        if (property.isExpanded && outcomesList != null && outcomesList.isArray)
        {
            EditorGUI.indentLevel++;
            
            Rect listHeaderRect = position;
            listHeaderRect.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(listHeaderRect, outcomesList.FindPropertyRelative("Array.size"));
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            float currentTotalWeight = CalculateTotalWeight(outcomesList);
            float totalLockedWeight = CalculateLockedWeight(outcomesList);

            GUIStyle totalLabelStyle = new GUIStyle(EditorStyles.label);
            if (!Mathf.Approximately(currentTotalWeight, TotalWeight))
            {
                totalLabelStyle.normal.textColor = Color.yellow;
            }
            Rect totalRect = position;
            totalRect.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(totalRect, $"Total: {currentTotalWeight:F1}/{TotalWeight} (Locked: {totalLockedWeight:F1})", totalLabelStyle);
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            EditorGUI.BeginChangeCheck(); 

            for (int i = 0; i < outcomesList.arraySize; i++)
            {
                SerializedProperty element = outcomesList.GetArrayElementAtIndex(i);
                SerializedProperty outcomeProp = element.FindPropertyRelative("Outcome");
                SerializedProperty weightProp = element.FindPropertyRelative("Weight");
                SerializedProperty lockedProp = element.FindPropertyRelative("IsLocked");

                // --- Dessine un séparateur ---
                Rect separatorRect = position;
                separatorRect.height = 1;
                separatorRect.x += EditorGUI.IndentedRect(position).x - position.x - 10;
                separatorRect.width = position.width - (separatorRect.x - position.x);
                EditorGUI.DrawRect(separatorRect, new Color(0.1f, 0.1f, 0.1f, 1f));
                position.y += EditorGUIUtility.standardVerticalSpacing;

                // --- Ligne 1: Outcome ---
                float h_outcome = EditorGUI.GetPropertyHeight(outcomeProp);
                Rect outcomeRect = position;
                outcomeRect.height = h_outcome;
                EditorGUI.PropertyField(outcomeRect, outcomeProp);
                position.y += h_outcome + EditorGUIUtility.standardVerticalSpacing;

                // --- Ligne 2: Weight, Percentage, Locked ---
                float h_line2 = EditorGUIUtility.singleLineHeight; 
                Rect line2Rect = position;
                line2Rect.height = h_line2;
                
                // 1. Weight
                Rect weightRect = line2Rect;
                weightRect.width = EditorGUIUtility.labelWidth + 60; // Label + Champ
                EditorGUI.PropertyField(weightRect, weightProp);
                
                // --- Suivi de la position horizontale ---
                float currentX = weightRect.x + weightRect.width + 5;

                // 2. Percentage
                string percentageString = "";
                if (currentTotalWeight > FloatComparisonTolerance && weightProp.floatValue > 0f)
                {
                    percentageString = $" ({(weightProp.floatValue / currentTotalWeight * 100f):F1}%)";
                }
                
                GUIContent percentageContent = new GUIContent(percentageString);
                float percentageWidth = EditorStyles.label.CalcSize(percentageContent).x + 20;

                Rect percentageRect = line2Rect;
                percentageRect.x = currentX;
                percentageRect.width = percentageWidth;
                EditorGUI.LabelField(percentageRect, percentageContent);
                currentX += percentageWidth + 5; // Avance X

                // 3. "Locked" Label
                GUIContent lockedLabelContent = new GUIContent("Locked");
                float lockedLabelWidth = EditorStyles.label.CalcSize(lockedLabelContent).x + 20;

                Rect lockedLabelRect = line2Rect;
                lockedLabelRect.x = currentX;
                lockedLabelRect.width = lockedLabelWidth;
                EditorGUI.LabelField(lockedLabelRect, lockedLabelContent);
                currentX += lockedLabelWidth + 3; // Avance X (espace serré)

                // 4. Checkbox (le toggle seul)
                Rect lockedFieldRect = line2Rect;
                lockedFieldRect.x = currentX;
                lockedFieldRect.width = 20; // Largeur juste pour la case
                
                EditorGUI.PropertyField(lockedFieldRect, lockedProp, GUIContent.none);

                position.y += h_line2 + (EditorGUIUtility.standardVerticalSpacing * 2);
            }

            if (EditorGUI.EndChangeCheck()) 
            {
                NormalizeWeights(outcomesList);
                EditorUtility.SetDirty(property.serializedObject.targetObject);
            }
            
            EditorGUI.indentLevel--;
        } 

        EditorGUI.EndProperty();
    }
    
    // --- GetPropertyHeight (Corrigé) ---
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
                
                for (int i = 0; i < outcomesList.arraySize; i++)
                {
                    SerializedProperty element = outcomesList.GetArrayElementAtIndex(i);
                    SerializedProperty outcomeProp = element.FindPropertyRelative("Outcome");
                    
                    // Calcule la hauteur pour nos 2 lignes personnalisées
                    float h_outcome = EditorGUI.GetPropertyHeight(outcomeProp);
                    float h_line2 = EditorGUIUtility.singleLineHeight; // Weight + Locked
                    
                    // Ajoute la hauteur pour les champs + séparateur + padding
                    float elementHeight = h_outcome + h_line2 + (EditorGUIUtility.standardVerticalSpacing * 4); 
                    
                    totalHeight += elementHeight;
                }
            }
        }
        return totalHeight;
    }

    // --- (Les fonctions 'CalculateTotalWeight', 'CalculateLockedWeight', 'NormalizeWeights' restent INCHANGÉES) ---
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
        
        if (totalLockedWeight > TotalWeight)
        {
            Debug.LogWarning("Locked weights exceed total weight. Normalization might behave unexpectedly. Please unlock some weights.");
            totalLockedWeight = TotalWeight; 
        }

        float availableWeightForUnlocked = TotalWeight - totalLockedWeight;
        if (availableWeightForUnlocked < 0) availableWeightForUnlocked = 0; 

        float currentUnlockedSum = 0f;
        List<int> unlockedIndices = new ();
        for (int i = 0; i < list.arraySize; i++)
        {
            SerializedProperty element = list.GetArrayElementAtIndex(i);
            if (!element.FindPropertyRelative("IsLocked").boolValue)
            {
                currentUnlockedSum += element.FindPropertyRelative("Weight").floatValue;
                unlockedIndices.Add(i);
            }
        }
        
        if (unlockedIndices.Count > 0 && !Mathf.Approximately(currentUnlockedSum, availableWeightForUnlocked))
        {
            float factor = 1.0f;
            if (currentUnlockedSum > FloatComparisonTolerance)
            {
                factor = availableWeightForUnlocked / currentUnlockedSum;
            }
            else
            {
                factor = 0; 
                float equalShare = availableWeightForUnlocked / unlockedIndices.Count;
                 foreach (int index in unlockedIndices)
                 {
                     list.GetArrayElementAtIndex(index).FindPropertyRelative("Weight").floatValue = equalShare;
                 }
                 list.serializedObject.ApplyModifiedProperties();
                 return;
            }
            
            foreach (int index in unlockedIndices)
            {
                SerializedProperty weightProp = list.GetArrayElementAtIndex(index).FindPropertyRelative("Weight");
                weightProp.floatValue = Mathf.Max(0f, weightProp.floatValue * factor);
            }
            
            float finalUnlockedSum = 0f;
            foreach (int index in unlockedIndices) { finalUnlockedSum += list.GetArrayElementAtIndex(index).FindPropertyRelative("Weight").floatValue;}

            float difference = availableWeightForUnlocked - finalUnlockedSum;
            
            if (unlockedIndices.Count > 0 && !Mathf.Approximately(difference, 0f))
            {
                 SerializedProperty firstUnlockedWeight = list.GetArrayElementAtIndex(unlockedIndices[0]).FindPropertyRelative("Weight");
                 firstUnlockedWeight.floatValue = Mathf.Max(0f, firstUnlockedWeight.floatValue + difference);
            }
        }
        list.serializedObject.ApplyModifiedProperties();
    }
}
#endif