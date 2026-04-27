namespace TourneeFutee
{
    public class Graph
    {
        #region déclaration attributs

        // ordre du graphe
        private int order;

        // Indique si le graphe est orienté
        private bool directed;

        // La valeur représentant l'absence d'arc (ex: 0)
        private float noEdgeValue;

        // Liste des noms des sommets 
        private List<string> vertexNames;

        // Liste des valeurs des sommets
        // L'index correspond à celui dans vertexNames
        private List<float> vertexValues;

        // La matrice d'adjacence qui stocke les poids des arcs
        // On utilise ta nouvelle classe Matrix
        private Matrix adjacencyMatrix;
        #endregion

        #region constructeur 
        public Graph(bool directed, float noEdgeValue = 0)
        {
            // 1. On stocke les paramètres dans les attributs de l'objet
            // Le mot-clé 'this' sert à dire "l'attribut de la classe" (pour le différencier du paramètre)
            this.directed = directed;
            this.noEdgeValue = noEdgeValue;

            // 2. On initialise les listes pour qu'elles soient prêtes à recevoir des données
            this.vertexNames = new List<string>();
            this.vertexValues = new List<float>();

            // 3. On initialise la matrice d'adjacence
            // Au début, le graphe est vide, donc 0 lignes et 0 colonnes.
            // On lui passe 'noEdgeValue' pour qu'elle sache quelle valeur mettre par défaut quand on l'agrandira.
            this.adjacencyMatrix = new Matrix(0, 0, noEdgeValue);
        }
        #endregion

        #region propriétés
        // --- Propriétés ---

        // Propriété : ordre du graphe
        // Lecture seule
        public int Order
        {
            get
            {

                return this.vertexNames.Count;
            }
        }

        // Propriété : graphe orienté ou non
        // Lecture seule
        public bool Directed
        {
            get
            {
                return this.directed;
            }
        }
        #endregion

        #region gestion des sommets
        // --- Gestion des sommets ---

        // Ajoute le sommet de nom `name` et de valeur `value` (0 par défaut) dans le graphe
        // Lève une ArgumentException s'il existe déjà un sommet avec le même nom dans le graphe
        public void AddVertex(string name, float value = 0)
        {
            // Vérifier si un sommet avec ce nom existe déjà
            if (this.vertexNames.Contains(name))
            {
                throw new ArgumentException($"Un sommet nommé '{name}' existe déjà dans le graphe.");
            }

            // Ajouter le nouveau sommet
            this.vertexNames.Add(name);
            this.vertexValues.Add(value);

            // Agrandir la matrice d'adjacence : ajouter une ligne et une colonne
            int newIndex = this.vertexNames.Count - 1;
            this.adjacencyMatrix.AddRow(newIndex);
            this.adjacencyMatrix.AddColumn(newIndex);
        }


        // Supprime le sommet de nom `name` du graphe (et tous les arcs associés)
        // Lève une ArgumentException si le sommet n'a pas été trouvé dans le graphe
        public void RemoveVertex(string name)
        {
            //on regarde si le sommet existe   
            if (!this.vertexNames.Contains(name))
            {
                throw new ArgumentException($"Le sommet nommé '{name}' n'existe pas dans le graphe.");
            }

            int index = this.vertexNames.IndexOf(name);

            // Supprimer le sommet des listes
            this.vertexNames.RemoveAt(index);
            this.vertexValues.RemoveAt(index);

            // Supprimer la ligne et la colonne correspondante de la matrice
            this.adjacencyMatrix.RemoveRow(index);
            this.adjacencyMatrix.RemoveColumn(index);
        }

        // Renvoie la valeur du sommet de nom `name`
        // Lève une ArgumentException si le sommet n'a pas été trouvé dans le graphe
        public float GetVertexValue(string name)
        {
            //on regarde si le sommet existe
            if(!this.vertexNames.Contains(name))
                {
                    throw new ArgumentException($"Le sommet nommé '{name}' n'existe pas dans le graphe.");
                }
            int index = this.vertexNames.IndexOf(name);
            return this.vertexValues[index];
        }

        // Affecte la valeur du sommet de nom `name` à `value`
        // Lève une ArgumentException si le sommet n'a pas été trouvé dans le graphe
        public void SetVertexValue(string name, float value)
        {
            // on regarde si le sommet existe
            if (!this.vertexNames.Contains(name))
            {
                throw new ArgumentException($"Le sommet nommé '{name}' n'existe pas dans le graphe.");
            }
            int index = this.vertexNames.IndexOf(name);
            this.vertexValues[index] = value;

        }


        // Renvoie la liste des noms des voisins du sommet de nom `vertexName`
        // (si ce sommet n'a pas de voisins, la liste sera vide)
        // Lève une ArgumentException si le sommet n'a pas été trouvé dans le graphe
        public List<string> GetNeighbors(string vertexName)
        {
            List<string> neighborNames = new List<string>();

            //on regarde si le sommet existe
            if (!this.vertexNames.Contains(vertexName))
            {
                throw new ArgumentException($"Le sommet nommé '{vertexName}' n'existe pas dans le graphe.");
            }

            // Récupérer l'index du sommet
            int vertexIndex = this.vertexNames.IndexOf(vertexName);

            // Parcourir la ligne correspondante dans la matrice d'adjacence
            for (int i = 0; i < this.adjacencyMatrix.NbColumns; i++)
            {
                // Si une arête existe (valeur différente de noEdgeValue)
                if (this.adjacencyMatrix.GetValue(vertexIndex, i) != this.noEdgeValue)
                {
                    neighborNames.Add(this.vertexNames[i]);
                }
            }

            return neighborNames;
        }
        #endregion
        // --- Gestion des arcs ---

        /* Ajoute un arc allant du sommet nommé `sourceName` au sommet nommé `destinationName`, avec le poids `weight` (1 par défaut)
         * Si le graphe n'est pas orienté, ajoute aussi l'arc inverse, avec le même poids
         * Lève une ArgumentException dans les cas suivants :
         * - un des sommets n'a pas été trouvé dans le graphe (source et/ou destination)
         * - il existe déjà un arc avec ces extrémités
         */
        public void AddEdge(string sourceName, string destinationName, float weight = 1)
        {
            // Vérifier que les deux sommets existent
            if (!this.vertexNames.Contains(sourceName))
            {
                throw new ArgumentException($"Le sommet nommé '{sourceName}' n'existe pas dans le graphe.");
            }

            if (!this.vertexNames.Contains(destinationName))
            {
                throw new ArgumentException($"Le sommet nommé '{destinationName}' n'existe pas dans le graphe.");
            }

            int sourceIndex = this.vertexNames.IndexOf(sourceName);
            int destinationIndex = this.vertexNames.IndexOf(destinationName);

            // Vérifier que l'arc (source, destination) n'existe pas déjà
            if (this.adjacencyMatrix.GetValue(sourceIndex, destinationIndex) != this.noEdgeValue)
            {
                throw new ArgumentException($"Un arc existe déjà entre '{sourceName}' et '{destinationName}'.");
            }

            // Si le graphe n'est pas orienté, vérifier que l'arc inverse n'existe pas non plus
            if (!this.directed && this.adjacencyMatrix.GetValue(destinationIndex, sourceIndex) != this.noEdgeValue)
            {
                throw new ArgumentException($"Un arc existe déjà entre '{sourceName}' et '{destinationName}'.");
            }

            // Ajouter l'arc (source, destination)
            this.adjacencyMatrix.SetValue(sourceIndex, destinationIndex, weight);

            // Si le graphe n'est pas orienté, ajouter aussi l'arc inverse
            if (!this.directed)
            {
                this.adjacencyMatrix.SetValue(destinationIndex, sourceIndex, weight);
            }
        }

        /* Supprime l'arc allant du sommet nommé `sourceName` au sommet nommé `destinationName` du graphe
         * Si le graphe n'est pas orienté, supprime aussi l'arc inverse
         * Lève une ArgumentException dans les cas suivants :
         * - un des sommets n'a pas été trouvé dans le graphe (source et/ou destination)
         * - l'arc n'existe pas
         */
        public void RemoveEdge(string sourceName, string destinationName)
        {
            // Vérifier que les deux sommets existent
            if (!this.vertexNames.Contains(sourceName))
            {
                throw new ArgumentException($"Le sommet nommé '{sourceName}' n'existe pas dans le graphe.");
            }

            if (!this.vertexNames.Contains(destinationName))
            {
                throw new ArgumentException($"Le sommet nommé '{destinationName}' n'existe pas dans le graphe.");
            }

            int sourceIndex = this.vertexNames.IndexOf(sourceName);
            int destinationIndex = this.vertexNames.IndexOf(destinationName);

            // Vérifier que l'arc (source, destination) existe
            if (this.adjacencyMatrix.GetValue(sourceIndex, destinationIndex) == this.noEdgeValue)
            {
                throw new ArgumentException($"L'arc entre '{sourceName}' et '{destinationName}' n'existe pas.");
            }

            // Supprimer l'arc (source, destination)
            this.adjacencyMatrix.SetValue(sourceIndex, destinationIndex, this.noEdgeValue);

            // Si le graphe n'est pas orienté, supprimer aussi l'arc inverse
            if (!this.directed)
            {
                this.adjacencyMatrix.SetValue(destinationIndex, sourceIndex, this.noEdgeValue);
            }
        }

        /* Renvoie le poids de l'arc allant du sommet nommé `sourceName` au sommet nommé `destinationName`
         * Si le graphe n'est pas orienté, GetEdgeWeight(A, B) = GetEdgeWeight(B, A) 
         * Lève une ArgumentException dans les cas suivants :
         * - un des sommets n'a pas été trouvé dans le graphe (source et/ou destination)
         * - l'arc n'existe pas
         */
        public float GetEdgeWeight(string sourceName, string destinationName)
        {
            // Vérifier que les deux sommets existent
            if (!this.vertexNames.Contains(sourceName))
            {
                throw new ArgumentException($"Le sommet nommé '{sourceName}' n'existe pas dans le graphe.");
            }

            if (!this.vertexNames.Contains(destinationName))
            {
                throw new ArgumentException($"Le sommet nommé '{destinationName}' n'existe pas dans le graphe.");
            }

            int sourceIndex = this.vertexNames.IndexOf(sourceName);
            int destinationIndex = this.vertexNames.IndexOf(destinationName);

            float weight = this.adjacencyMatrix.GetValue(sourceIndex, destinationIndex);

            // Vérifier que l'arc existe
            if (weight == this.noEdgeValue)
            {
                throw new ArgumentException($"L'arc entre '{sourceName}' et '{destinationName}' n'existe pas.");
            }

            return weight;
        }

        /* Affecte le poids l'arc allant du sommet nommé `sourceName` au sommet nommé `destinationName` à `weight` 
         * Si le graphe n'est pas orienté, affecte le même poids à l'arc inverse
         * Lève une ArgumentException si un des sommets n'a pas été trouvé dans le graphe (source et/ou destination)
         */
        public void SetEdgeWeight(string sourceName, string destinationName, float weight)
        {
            // Vérifier que les deux sommets existent
            if (!this.vertexNames.Contains(sourceName))
            {
                throw new ArgumentException($"Le sommet nommé '{sourceName}' n'existe pas dans le graphe.");
            }

            if (!this.vertexNames.Contains(destinationName))
            {
                throw new ArgumentException($"Le sommet nommé '{destinationName}' n'existe pas dans le graphe.");
            }

            int sourceIndex = this.vertexNames.IndexOf(sourceName);
            int destinationIndex = this.vertexNames.IndexOf(destinationName);

            // Modifier le poids de l'arc (source, destination)
            this.adjacencyMatrix.SetValue(sourceIndex, destinationIndex, weight);

            // Si le graphe n'est pas orienté, modifier aussi l'arc inverse
            if (!this.directed)
            {
                this.adjacencyMatrix.SetValue(destinationIndex, sourceIndex, weight);
            }
        }
        public List<string> GetAllVertexNames()
        {
            return new List<string>(this.vertexNames);
        }

        // TODO : ajouter toutes les méthodes que vous jugerez pertinentes 

    }


}
