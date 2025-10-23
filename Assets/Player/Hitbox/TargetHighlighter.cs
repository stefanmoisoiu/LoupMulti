using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.Common.Hitbox;
using Player.Networking;
using UnityEngine;

namespace Player.Target
{
    public class TargetHighlighter : PNetworkBehaviour
    {
        [SerializeField] private TargetDetector targetDetector;
        [SerializeField] private bool highlightMultiple;
        [SerializeField] private bool highlightOnStart = true;
        
        [Tooltip("Fréquence de vérification des cibles (en secondes). Plus c'est haut, plus c'est performant.")]
        [SerializeField] private float checkInterval = 0.1f;

        // Cache pour le mode "Cible unique"
        private Targetable _currentSingleTarget;
        
        // Cache pour le mode "Cibles multiples" (plus robuste qu'un tableau)
        private HashSet<Targetable> _currentMultipleTargets = new HashSet<Targetable>();

        private Coroutine _highlightCoroutine;

        protected override void StartAnyOwner()
        {
            if (highlightOnStart) EnableHighlight();
        }

        public void EnableHighlight()
        {
            if (_highlightCoroutine != null) return;
            // On lance la boucle qui tourne à intervalle fixe, pas à chaque frame
            _highlightCoroutine = StartCoroutine(HighlightCheckLoop());
        }

        public void DisableHighlight()
        {
            if (_highlightCoroutine != null)
            {
                StopCoroutine(_highlightCoroutine);
                _highlightCoroutine = null;
            }
            ClearAllHighlights();
        }

        private void OnDisable()
        {
            // Sécurité pour nettoyer si l'objet est désactivé
            DisableHighlight();
        }

        private IEnumerator HighlightCheckLoop()
        {
            while (true)
            {
                if (highlightMultiple)
                {
                    UpdateMultipleHighlights();
                }
                else
                {
                    UpdateSingleHighlight();
                }
                yield return new WaitForSeconds(checkInterval);
            }
        }

        /// <summary>
        /// Gère la surbrillance de la cible la plus proche.
        /// </summary>
        private void UpdateSingleHighlight()
        {
            Targetable newTarget = targetDetector.CalculateClosestTarget();

            // Si la cible n'a pas changé, on ne fait rien
            if (newTarget == _currentSingleTarget) return;
            
            // La cible a changé : on éteint l'ancienne et on allume la nouvelle
            HideHighlightTarget(_currentSingleTarget);
            ShowHighlightTarget(newTarget);
            _currentSingleTarget = newTarget;
        }

        /// <summary>
        /// Gère la surbrillance de toutes les cibles dans la zone.
        /// </summary>
        private void UpdateMultipleHighlights()
        {
            Targetable[] targets = targetDetector.CalculateTargets();
            
            // 1. Convertir les cibles en un HashSet
            // ( HashSet est optimisé pour les recherches de type "Contains" )
            HashSet<Targetable> newTargets = new HashSet<Targetable>(targets);

            // 2. Trouver les cibles à éteindre
            // (Celles qui sont dans l'ancienne liste mais pas dans la nouvelle)
            _currentMultipleTargets.RemoveWhere(target => 
            {
                if (!newTargets.Contains(target))
                {
                    HideHighlightTarget(target);
                    return true; // Retirer de la liste _currentMultipleTargets
                }
                return false; // Garder dans la liste
            });

            // 3. Trouver les cibles à allumer
            // (Celles qui sont dans la nouvelle liste mais pas dans l'ancienne)
            foreach (var target in newTargets)
            {
                // .Add() retourne 'true' seulement si l'élément n'était pas déjà là
                if (_currentMultipleTargets.Add(target))
                {
                    ShowHighlightTarget(target);
                }
            }
        }

        /// <summary>
        /// Nettoie toutes les surbrillances actives.
        /// </summary>
        private void ClearAllHighlights()
        {
            HideHighlightTarget(_currentSingleTarget);
            _currentSingleTarget = null;

            foreach (var target in _currentMultipleTargets)
            {
                HideHighlightTarget(target);
            }
            _currentMultipleTargets.Clear();
        }

        // --- Fonctions "Hook" ---
        // C'est ici qu'on gère le composant visuel.
        // On utilise GetComponent, qui est plus sûr que ton "as".
        
        private void ShowHighlightTarget(Targetable target)
        {
            if (target == null) return;

            // On cherche le composant visuel sur l'objet cible
            var outline = target.GetComponent<HighlightedTargetable>();
            outline?.Outline?.AddOutline();
        }

        private void HideHighlightTarget(Targetable target)
        {
            if (target == null) return;
            
            var outline = target.GetComponent<HighlightedTargetable>();
            outline?.Outline?.RemoveOutline();
        }
    }
}