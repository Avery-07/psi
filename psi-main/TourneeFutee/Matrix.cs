namespace TourneeFutee
{
    public class Matrix
    {
        private float[,] data;
        private float defaultValue;

        /* Crée une matrice de dimensions `nbRows` x `nbColums`.
         * Toutes les cases de cette matrice sont remplies avec `defaultValue`.
         * Lève une ArgumentOutOfRangeException si une des dimensions est négative
         */
        public Matrix(int nbRows = 0, int nbColumns = 0, float defaultValue = 0)
        {
            if (nbRows < 0 || nbColumns < 0)
            {
                throw new ArgumentOutOfRangeException("Les dimensions sont négatives.");
            }

            this.defaultValue = defaultValue;
            this.data = new float[nbRows, nbColumns];

            for (int i = 0; i < nbRows; i++)
            {
                for (int j = 0; j < nbColumns; j++)
                {
                    this.data[i, j] = defaultValue;
                }
            }
        }

        // Propriété : valeur par défaut utilisée pour remplir les nouvelles cases
        // Lecture seule
        public float DefaultValue
        {
            get { return this.defaultValue; }
        }

        // Propriété : nombre de lignes
        // Lecture seule
        public int NbRows
        {
            get { return this.data.GetLength(0); }
        }

        // Propriété : nombre de colonnes
        // Lecture seule
        public int NbColumns
        {
            get { return this.data.GetLength(1); }
        }

        /* Insère une ligne à l'indice `i`. Décale les lignes suivantes vers le bas.
         * Toutes les cases de la nouvelle ligne contiennent DefaultValue.
         * Si `i` = NbRows, insère une ligne en fin de matrice
         * Lève une ArgumentOutOfRangeException si `i` est en dehors des indices valides
         */
        public void AddRow(int i)
        {
            if (i < 0 || i > this.NbRows)
            {
                throw new ArgumentOutOfRangeException(nameof(i), "L'indice de ligne est négatif ou trop grand.");
            }

            float[,] newData = new float[this.NbRows + 1, this.NbColumns];

            for (int row = 0; row < i; row++)
            {
                for (int col = 0; col < this.NbColumns; col++)
                {
                    newData[row, col] = this.data[row, col];
                }
            }

            for (int col = 0; col < this.NbColumns; col++)
            {
                newData[i, col] = this.defaultValue;
            }

            for (int row = i; row < this.NbRows; row++)
            {
                for (int col = 0; col < this.NbColumns; col++)
                {
                    newData[row + 1, col] = this.data[row, col];
                }
            }

            this.data = newData;
        }

        /* Insère une colonne à l'indice `j`. Décale les colonnes suivantes vers la droite.
         * Toutes les cases de la nouvelle colonne contiennent DefaultValue.
         * Si `j` = NbColums, insère une colonne en fin de matrice
         * Lève une ArgumentOutOfRangeException si `j` est en dehors des indices valides
         */
        public void AddColumn(int j)
        {
            if (j < 0 || j > this.NbColumns)
            {
                throw new ArgumentOutOfRangeException(nameof(j), "L'indice de colonne est négatif ou trop grand.");
            }

            float[,] newData = new float[this.NbRows, this.NbColumns + 1];

            for (int row = 0; row < this.NbRows; row++)
            {
                for (int col = 0; col < j; col++)
                {
                    newData[row, col] = this.data[row, col];
                }

                newData[row, j] = this.defaultValue;

                for (int col = j; col < this.NbColumns; col++)
                {
                    newData[row, col + 1] = this.data[row, col];
                }
            }

            this.data = newData;
        }

        // Supprime la ligne à l'indice `i`. Décale les lignes suivantes vers le haut.
        // Lève une ArgumentOutOfRangeException si `i` est en dehors des indices valides
        public void RemoveRow(int i)
        {
            if (i < 0 || i >= this.NbRows)
            {
                throw new ArgumentOutOfRangeException(nameof(i), "L'indice de ligne est négatif ou trop grand.");
            }

            float[,] newData = new float[this.NbRows - 1, this.NbColumns];

            for (int row = 0; row < i; row++)
            {
                for (int col = 0; col < this.NbColumns; col++)
                {
                    newData[row, col] = this.data[row, col];
                }
            }

            for (int row = i + 1; row < this.NbRows; row++)
            {
                for (int col = 0; col < this.NbColumns; col++)
                {
                    newData[row - 1, col] = this.data[row, col];
                }
            }

            this.data = newData;
        }

        // Supprime la colonne à l'indice `j`. Décale les colonnes suivantes vers la gauche.
        // Lève une ArgumentOutOfRangeException si `j` est en dehors des indices valides
        public void RemoveColumn(int j)
        {
            if (j < 0 || j >= this.NbColumns)
            {
                throw new ArgumentOutOfRangeException(nameof(j), "L'indice de colonne est négatif ou trop grand.");
            }

            float[,] newData = new float[this.NbRows, this.NbColumns - 1];

            for (int row = 0; row < this.NbRows; row++)
            {
                for (int col = 0; col < j; col++)
                {
                    newData[row, col] = this.data[row, col];
                }

                for (int col = j + 1; col < this.NbColumns; col++)
                {
                    newData[row, col - 1] = this.data[row, col];
                }
            }

            this.data = newData; 
        }

        

        

        // Renvoie la valeur à la ligne `i` et colonne `j`
        // Lève une ArgumentOutOfRangeException si `i` ou `j` est en dehors des indices valides
        public float GetValue(int i, int j)
        {
            if (i < 0 || i >= this.NbRows || j < 0 || j >= this.NbColumns)
            {
                throw new ArgumentOutOfRangeException("Les indices sont en dehors des limites valides.");
            }

            return this.data[i, j];
        }

        // Affecte la valeur à la ligne `i` et colonne `j` à `v`
        // Lève une ArgumentOutOfRangeException si `i` ou `j` est en dehors des indices valides
        public void SetValue(int i, int j, float v)
        {
            if (i < 0 || i >= this.NbRows || j < 0 || j >= this.NbColumns)
            {
                throw new ArgumentOutOfRangeException("Les indices sont en dehors des limites valides.");
            }

            this.data[i, j] = v;
        }

        // Affiche la matrice
        public void Print()
        {
            for (int i = 0; i < this.NbRows; i++)
            {
                for (int j = 0; j < this.NbColumns; j++)
                {
                    Console.Write(this.data[i, j].ToString() + "\t");
                }
                Console.WriteLine();
            }
        }
    }
}

