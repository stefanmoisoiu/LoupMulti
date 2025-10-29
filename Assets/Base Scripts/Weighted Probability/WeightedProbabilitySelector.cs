namespace Base_Scripts
{
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine; // Nécessaire pour MinAttribute et Random.Range
using Random = UnityEngine.Random;

/// <summary>
/// Classe sérializable pour choisir un résultat aléatoire basé sur des poids définis.
/// Utilisable comme champ [SerializeField] dans d'autres classes/MonoBehaviours.
/// </summary>
/// <typeparam name="T">Le type de résultat à choisir (doit être sérializable par Unity si T est complexe).</typeparam>
[Serializable]
public class WeightedProbabilitySelector<T>
{
    [Serializable]
    public struct WeightedOutcome
    {
        public T Outcome;
        [Min(0f)]
        public float Weight;

        public bool IsLocked;
    }

    [Tooltip("Liste des résultats possibles avec leur poids associé.")]
    [SerializeField] // Permet de le voir et le modifier dans l'inspecteur
    private List<WeightedOutcome> outcomes = new List<WeightedOutcome>();

    // Pas de cache _totalWeight ici car OnValidate n'est pas appelé
    // Le calcul sera fait à chaque appel, ce qui est généralement négligeable

    /// <summary>
    /// Choisit un résultat aléatoire basé sur les poids définis.
    /// </summary>
    /// <returns>Le résultat choisi, ou default(T) si aucun résultat valide n'est disponible.</returns>
    public T GetRandomOutcome(List<WeightedOutcome> filteredOutcomes = null)
    {
        List<WeightedOutcome> weightedOutcomes = filteredOutcomes ?? outcomes;
        if (weightedOutcomes == null || weightedOutcomes.Count == 0)
        {
            Debug.LogWarning("WeightedProbabilitySelector: La liste 'outcomes' est vide.");
            return default;
        }

        // Calcule le poids total à chaque fois (simple et robuste pour une classe non-MonoBehaviour)
        float totalWeight = weightedOutcomes.Where(o => o.Weight > 0).Sum(o => o.Weight);

        if (totalWeight <= 0f)
        {
            Debug.LogWarning("WeightedProbabilitySelector: Aucun 'outcome' n'a de poids positif.");
            return weightedOutcomes.FirstOrDefault().Outcome; // Retourne le premier, ou default(T)
        }

        float randomValue = Random.Range(0f, totalWeight);
        float currentWeightSum = 0f;

        foreach (var outcome in weightedOutcomes.Where(o => o.Weight > 0))
        {
            currentWeightSum += outcome.Weight;
            if (randomValue <= currentWeightSum)
            {
                return outcome.Outcome;
            }
        }

        // Fallback (sécurité)
        return weightedOutcomes.Last(o => o.Weight > 0).Outcome;
    }

    // --- Optionnel : Helper pour récupérer tous les outcomes ---
    public List<WeightedOutcome> GetAllOutcomes()
    {
        return outcomes;
    }
}
}