using System;
using System.Collections.Generic;

namespace TourneeFutee
{
    public class Tour
    {
        private List<(string source, string destination)> _segments;
        private float _cost;

        // Initialise une tournée vide sans segments ni coût.
        public Tour()
        {
            _segments = new List<(string source, string destination)>();
            _cost = 0f;
        }
        
        // Initialise une tournée à partir d'une liste ordonnée de villes et d'un coût total.
        // Les segments sont construits en reliant chaque ville à la suivante, en bouclant sur la première.
        public Tour(List<string> orderedCities, float cost)
        {
            _cost = cost;
            _segments = new List<(string source, string destination)>();

            for (int i = 0; i < orderedCities.Count; i++)
            {
                string src = orderedCities[i];
                string dest = orderedCities[(i + 1) % orderedCities.Count];
                _segments.Add((src, dest));
            }
        }

        // Retourne le coût total de la tournée.
        public float Cost
        {
            get { return _cost; }
        }

        // Retourne le nombre de segments composant la tournée.
        public int NbSegments
        {
            get { return _segments.Count; }
        }

        // Retourne la liste ordonnée des sommets de la tournée.
        // Extrait les sources de chaque segment (qui correspondent aux villes visitées dans l'ordre).
        public IList<string> Vertices
        {
            get
            {
                List<string> vertices = new List<string>();
                foreach (var seg in _segments)
                {
                    vertices.Add(seg.source);
                }
                return vertices;
            }
        }

        // Vérifie si un segment donné (source, destination) est présent dans la tournée.
        public bool ContainsSegment((string source, string destination) segment)
        {
            foreach (var seg in _segments)
                if (seg.source == segment.source && seg.destination == segment.destination)
                    return true;

            return false;
        }

        // Affiche dans la console le coût total et la liste des segments de la tournée.
        public void Print()
        {
            Console.WriteLine("Coût total : " + _cost);
            Console.WriteLine("Trajets :");
            foreach (var seg in _segments)
                Console.WriteLine("  " + seg.source + " -> " + seg.destination);
        }
    }
}
