using System;
using System.Collections.Generic;

namespace TourneeFutee
{
    public class Little
    {
        //déclaration des arguments
        private Graph _graph;
        private float _bestCost;
        private Tour _bestTour;

        public Little(Graph graph)
        {
            _graph = graph;
            _bestCost = float.PositiveInfinity;
            _bestTour = null;
        }

        public Tour ComputeOptimalTour()
        {
            List<string> cities = _graph.GetAllVertexNames();
            int n = cities.Count;
            // 1. Initialisation de la matrice des coûts : la diagonale (i==j) et 
            // les arêtes inexistantes sont fixées à l'infini pour empêcher leur sélection.
            Matrix matrix = new Matrix(n, n, float.PositiveInfinity);
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i == j) continue;
                    try
                    {
                        matrix.SetValue(i, j, _graph.GetEdgeWeight(cities[i], cities[j]));
                    }
                    catch (ArgumentException) { }
                }
            }

            float rootValue = ReduceMatrix(matrix);

            // On sépare le suivi des lignes et des colonnes
            List<string> rowLabels = new List<string>(cities);
            List<string> colLabels = new List<string>(cities);

            Branch(matrix, rootValue, new List<(string source, string destination)>(), rowLabels, colLabels);

            return _bestTour;
        }

        public static float ReduceMatrix(Matrix m)
        {
            float total = 0f;
            // 2. Réduction de la matrice : on soustrait le minimum de chaque ligne 
            // puis de chaque colonne. La somme de ces minima constitue la borne inférieure.
            for (int i = 0; i < m.NbRows; i++)
            {
                float min = float.PositiveInfinity;
                for (int j = 0; j < m.NbColumns; j++)
                    if (m.GetValue(i, j) < min) min = m.GetValue(i, j);

                if (!float.IsPositiveInfinity(min) && min > 0f)
                {
                    for (int j = 0; j < m.NbColumns; j++)
                        if (!float.IsPositiveInfinity(m.GetValue(i, j)))
                            m.SetValue(i, j, m.GetValue(i, j) - min);
                    total += min;
                }
            }

            for (int j = 0; j < m.NbColumns; j++)
            {
                float min = float.PositiveInfinity;
                for (int i = 0; i < m.NbRows; i++)
                    if (m.GetValue(i, j) < min) min = m.GetValue(i, j);

                if (!float.IsPositiveInfinity(min) && min > 0f)
                {
                    for (int i = 0; i < m.NbRows; i++)
                        if (!float.IsPositiveInfinity(m.GetValue(i, j)))
                            m.SetValue(i, j, m.GetValue(i, j) - min);
                    total += min;
                }
            }

            return total;
        }

        public static (int i, int j, float value) GetMaxRegret(Matrix m)
        {
            int bestI = -1, bestJ = -1;
            float bestRegret = -1f;

            // 3. Calcul du regret : pour chaque zéro, on évalue la pénalité (somme des 
            // minima restants sur la ligne et la colonne) si l'on décide de NE PAS emprunter cette arête.
            for (int i = 0; i < m.NbRows; i++)
            {
                for (int j = 0; j < m.NbColumns; j++)
                {
                    if (m.GetValue(i, j) != 0f) continue;

                    float rowMin = float.PositiveInfinity;
                    for (int k = 0; k < m.NbColumns; k++)
                        if (k != j && m.GetValue(i, k) < rowMin)
                            rowMin = m.GetValue(i, k);

                    float colMin = float.PositiveInfinity;
                    for (int k = 0; k < m.NbRows; k++)
                        if (k != i && m.GetValue(k, j) < colMin)
                            colMin = m.GetValue(k, j);

                    float regret = (float.IsPositiveInfinity(rowMin) ? 0f : rowMin)
                                 + (float.IsPositiveInfinity(colMin) ? 0f : colMin);

                    if (regret > bestRegret)
                    {
                        bestRegret = regret;
                        bestI = i;
                        bestJ = j;
                    }
                }
            }

            return (bestI, bestJ, bestRegret);
        }

        public static bool IsForbiddenSegment(
            (string source, string destination) segment,
            List<(string source, string destination)> includedSegments,
            int nbCities)
        {
            var next = new Dictionary<string, string>();
            foreach (var (s, d) in includedSegments)
                next[s] = d;

            string current = segment.destination;
            int steps = 0;

            // 4. Prévention des sous-tours : on remonte le chemin potentiel pour s'assurer 
            // que l'ajout de ce segment ne crée pas une boucle fermée avant d'avoir visité toutes les villes.
            while (next.TryGetValue(current, out string nextCity))
            {
                steps++;
                if (nextCity == segment.source)
                    return (steps + 1) < nbCities;
                current = nextCity;
                if (steps >= nbCities) break;
            }

            return false;
        }

        private void Branch(
            Matrix matrix,
            float nodeValue,
            List<(string source, string destination)> includedSegments,
            List<string> rowLabels,
            List<string> colLabels)
        {
            if (nodeValue >= _bestCost) return;

            if (rowLabels.Count == 2)
            {
                var finalSegments = new List<(string source, string destination)>(includedSegments);

                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        if (!float.IsPositiveInfinity(matrix.GetValue(i, j)))
                        {
                            var seg = (rowLabels[i], colLabels[j]);
                            if (!finalSegments.Contains(seg))
                                finalSegments.Add(seg);
                        }
                    }
                }

                Tour candidate = BuildTour(finalSegments);
                if (candidate != null && candidate.Cost < _bestCost)
                {
                    _bestCost = candidate.Cost;
                    _bestTour = candidate;
                }
                return;
            }

            var (ri, rj, regret) = GetMaxRegret(matrix);
            if (ri < 0) return;

            string source = rowLabels[ri];
            string destination = colLabels[rj];

            // 5. Séparation (Branching) : L'arbre de recherche se divise ici. 
            // La branche DROITE modélise la décision D'INCLURE l'arête (réduction de la matrice), 
            // tandis que la branche GAUCHE modélise son EXCLUSION (mise à l'infini de la case).

            // --- Branche droite : trajet inclus ---
            var newIncluded = new List<(string source, string destination)>(includedSegments);
            newIncluded.Add((source, destination));

            var newRowLabels = new List<string>();
            var newColLabels = new List<string>();

            for (int i = 0; i < rowLabels.Count; i++)
                if (i != ri) newRowLabels.Add(rowLabels[i]);

            for (int j = 0; j < colLabels.Count; j++)
                if (j != rj) newColLabels.Add(colLabels[j]);

            int nn = newRowLabels.Count;
            Matrix subMatrix = new Matrix(nn, nn, float.PositiveInfinity);

            for (int i = 0; i < nn; i++)
            {
                int oldI = rowLabels.IndexOf(newRowLabels[i]);
                for (int j = 0; j < nn; j++)
                {
                    int oldJ = colLabels.IndexOf(newColLabels[j]);
                    subMatrix.SetValue(i, j, matrix.GetValue(oldI, oldJ));
                }
            }

            for (int i = 0; i < nn; i++)
            {
                for (int j = 0; j < nn; j++)
                {
                    if (!float.IsPositiveInfinity(subMatrix.GetValue(i, j)))
                    {
                        if (IsForbiddenSegment((newRowLabels[i], newColLabels[j]), newIncluded, _graph.Order))
                        {
                            subMatrix.SetValue(i, j, float.PositiveInfinity);
                        }
                    }
                }
            }

            float rightReduction = ReduceMatrix(subMatrix);
            float childValue = nodeValue + rightReduction;
            Branch(subMatrix, childValue, newIncluded, newRowLabels, newColLabels);

            // --- Branche gauche : trajet exclu ---
            Matrix excludeMatrix = CloneMatrix(matrix);
            excludeMatrix.SetValue(ri, rj, float.PositiveInfinity);

            float leftReduction = ReduceMatrix(excludeMatrix);
            Branch(excludeMatrix, nodeValue + leftReduction, includedSegments, rowLabels, colLabels);
        }

        private Matrix CloneMatrix(Matrix m)
        {
            Matrix clone = new Matrix(m.NbRows, m.NbColumns, float.PositiveInfinity);
            for (int i = 0; i < m.NbRows; i++)
                for (int j = 0; j < m.NbColumns; j++)
                    clone.SetValue(i, j, m.GetValue(i, j));
            return clone;
        }

        private Tour BuildTour(List<(string source, string destination)> segments)
        {
            int n = _graph.Order;
            if (segments.Count != n) return null;

            var next = new Dictionary<string, string>();
            foreach (var (s, d) in segments)
            {
                if (next.ContainsKey(s)) return null;
                next[s] = d;
            }

            List<string> allCities = _graph.GetAllVertexNames();
            string start = allCities[0];

            if (!next.ContainsKey(start)) return null;

            var ordered = new List<string>();
            string current = start;

            for (int step = 0; step < n; step++)
            {
                if (ordered.Contains(current)) return null;
                ordered.Add(current);
                if (!next.TryGetValue(current, out string nextCity)) return null;
                current = nextCity;
            }

            if (current != start) return null;
            if (ordered.Count != n) return null;

            float cost = 0f;
            for (int i = 0; i < n; i++)
            {
                try
                {
                    cost += _graph.GetEdgeWeight(ordered[i], ordered[(i + 1) % n]);
                }
                catch (ArgumentException)
                {
                    return null;
                }
            }

            return new Tour(ordered, cost);
        }
    }
}