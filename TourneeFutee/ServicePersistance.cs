using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace TourneeFutee
{
    /// <summary>
    /// Service de persistance permettant de sauvegarder et charger
    /// des graphes et des tournées dans une base de données MySQL.
    /// </summary>
    public class ServicePersistance
    {
        // ─────────────────────────────────────────────────────────────────────
        // Attributs privés
        // ─────────────────────────────────────────────────────────────────────

        private readonly string _connectionString;

        // ─────────────────────────────────────────────────────────────────────
        // Constructeur
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Instancie un service de persistance et se connecte automatiquement
        /// à la base de données <paramref name="dbname"/> sur le serveur
        /// à l'adresse IP <paramref name="serverIp"/>.
        /// Les identifiants sont définis par <paramref name="user"/> (utilisateur)
        /// et <paramref name="pwd"/> (mot de passe).
        /// </summary>
        /// <param name="serverIp">Adresse IP du serveur MySQL.</param>
        /// <param name="dbname">Nom de la base de données.</param>
        /// <param name="user">Nom d'utilisateur.</param>
        /// <param name="pwd">Mot de passe.</param>
        /// <exception cref="Exception">Levée si la connexion échoue.</exception>
        public ServicePersistance(string serverIp, string dbname, string user, string pwd)
        {
            _connectionString = $"server={serverIp};database={dbname};uid={user};pwd={pwd};";

            // Test de la connexion dès la construction comme demandé
            try
            {
                using (var conn = OpenConnection())
                {
                    // Si on arrive ici, la connexion s'est bien ouverte
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Échec de la connexion à la base de données : {ex.Message}");
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Méthodes publiques
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Sauvegarde le graphe <paramref name="g"/> en base de données
        /// (sommets et arcs inclus) et renvoie son identifiant.
        /// </summary>
        /// <param name="g">Le graphe à sauvegarder.</param>
        /// <returns>Identifiant du graphe en base de données (AUTO_INCREMENT).</returns>
        public uint SaveGraph(Graph g)
        {
            uint graphId = 0;
            var vertices = g.GetAllVertexNames();

            using (var conn = OpenConnection())
            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    // 1. Sauvegarder le Graphe
                    string sqlGraphe = "INSERT INTO Graphe (est_oriente) VALUES (@est_oriente); SELECT LAST_INSERT_ID();";
                    using (var cmd = new MySqlCommand(sqlGraphe, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@est_oriente", g.Directed ? 1 : 0);
                        graphId = Convert.ToUInt32(cmd.ExecuteScalar());
                    }

                    // 2. Sauvegarder les Sommets (en gardant la correspondance index C# -> ID DB)
                    uint[] dbSommetIds = new uint[vertices.Count];
                    string sqlSommet = "INSERT INTO Sommet (graphe_id, nom, valeur, indice) VALUES (@graphe_id, @nom, @valeur, @indice); SELECT LAST_INSERT_ID();";

                    for (int i = 0; i < vertices.Count; i++)
                    {
                        using (var cmd = new MySqlCommand(sqlSommet, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@graphe_id", graphId);
                            cmd.Parameters.AddWithValue("@nom", vertices[i]);
                            cmd.Parameters.AddWithValue("@valeur", g.GetVertexValue(vertices[i]));
                            cmd.Parameters.AddWithValue("@indice", i);
                            dbSommetIds[i] = Convert.ToUInt32(cmd.ExecuteScalar());
                        }
                    }

                    // 3. Sauvegarder les Arcs
                    string sqlArc = "INSERT INTO Arc (graphe_id, sommet_source, sommet_dest, poids) VALUES (@graphe_id, @source, @dest, @poids);";
                    for (int i = 0; i < vertices.Count; i++)
                    {
                        for (int j = 0; j < vertices.Count; j++)
                        {
                            // Si le graphe n'est pas orienté, on ne sauvegarde qu'une fois (i <= j) pour éviter les doublons
                            if (!g.Directed && i > j) continue;

                            try
                            {
                                float weight = g.GetEdgeWeight(vertices[i], vertices[j]);
                                // Si on n'a pas levé d'exception, l'arc existe
                                using (var cmd = new MySqlCommand(sqlArc, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@graphe_id", graphId);
                                    cmd.Parameters.AddWithValue("@source", dbSommetIds[i]);
                                    cmd.Parameters.AddWithValue("@dest", dbSommetIds[j]);
                                    cmd.Parameters.AddWithValue("@poids", weight);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            catch (ArgumentException)
                            {
                                // L'arc n'existe pas, on l'ignore silencieusement
                            }
                        }
                    }

                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
            return graphId;
        }

        /// <summary>
        /// Charge depuis la base de données le graphe identifié par <paramref name="id"/>
        /// et renvoie une instance de la classe <see cref="Graph"/>.
        /// </summary>
        /// <param name="id">Identifiant du graphe à charger.</param>
        /// <returns>Instance de <see cref="Graph"/> reconstituée.</returns>
        public Graph LoadGraph(uint id)
        {
            Graph g = null;
            Dictionary<uint, string> sommetDbIdToName = new Dictionary<uint, string>();

            using (var conn = OpenConnection())
            {
                // 1. Charger le Graphe
                string sqlGraphe = "SELECT est_oriente FROM Graphe WHERE id = @id;";
                using (var cmd = new MySqlCommand(sqlGraphe, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            bool directed = reader.GetInt32("est_oriente") == 1;
                            // On initialise avec une valeur par défaut de +infini pour correspondre à l'algo de Little
                            g = new Graph(directed, float.PositiveInfinity);
                        }
                        else
                        {
                            throw new ArgumentException($"Graphe avec l'ID {id} introuvable.");
                        }
                    }
                }

                // 2. Charger les Sommets (dans l'ordre strict grâce à 'ORDER BY indice')
                string sqlSommet = "SELECT id, nom, valeur FROM Sommet WHERE graphe_id = @id ORDER BY indice ASC;";
                using (var cmd = new MySqlCommand(sqlSommet, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            uint sommetId = reader.GetUInt32("id");
                            string nom = reader.GetString("nom");
                            float valeur = reader.IsDBNull(reader.GetOrdinal("valeur")) ? 0 : reader.GetFloat("valeur");

                            g.AddVertex(nom, valeur);
                            sommetDbIdToName[sommetId] = nom;
                        }
                    }
                }

                // 3. Charger les Arcs
                string sqlArc = "SELECT sommet_source, sommet_dest, poids FROM Arc WHERE graphe_id = @id;";
                using (var cmd = new MySqlCommand(sqlArc, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            uint sourceId = reader.GetUInt32("sommet_source");
                            uint destId = reader.GetUInt32("sommet_dest");
                            float poids = reader.GetFloat("poids");

                            string sourceName = sommetDbIdToName[sourceId];
                            string destName = sommetDbIdToName[destId];

                            // AddEdge gère automatiquement le double sens si le graphe est non orienté
                            g.AddEdge(sourceName, destName, poids);
                        }
                    }
                }
            }
            return g;
        }

        /// <summary>
        /// Sauvegarde la tournée <paramref name="t"/> (effectuée dans le graphe
        /// identifié par <paramref name="graphId"/>) en base de données
        /// et renvoie son identifiant.
        /// </summary>
        /// <param name="graphId">Identifiant BdD du graphe dans lequel la tournée a été calculée.</param>
        /// <param name="t">La tournée à sauvegarder.</param>
        /// <returns>Identifiant de la tournée en base de données (AUTO_INCREMENT).</returns>
        public uint SaveTour(uint graphId, Tour t)
        {
            uint tourId = 0;
            var vertices = t.Vertices;

            using (var conn = OpenConnection())
            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    // 1. Sauvegarder la Tournée
                    string sqlTournee = "INSERT INTO Tournee (graphe_id, cout_total) VALUES (@graphe_id, @cout); SELECT LAST_INSERT_ID();";
                    using (var cmd = new MySqlCommand(sqlTournee, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@graphe_id", graphId);
                        cmd.Parameters.AddWithValue("@cout", t.Cost);
                        tourId = Convert.ToUInt32(cmd.ExecuteScalar());
                    }

                    // 2. Sauvegarder les étapes
                    string sqlFindSommetId = "SELECT id FROM Sommet WHERE graphe_id = @graphe_id AND nom = @nom;";
                    string sqlEtape = "INSERT INTO EtapeTournee (tournee_id, numero_ordre, sommet_id) VALUES (@tournee_id, @ordre, @sommet_id);";

                    for (int i = 0; i < vertices.Count; i++)
                    {
                        uint sommetId = 0;
                        using (var cmdFind = new MySqlCommand(sqlFindSommetId, conn, transaction))
                        {
                            cmdFind.Parameters.AddWithValue("@graphe_id", graphId);
                            cmdFind.Parameters.AddWithValue("@nom", vertices[i]);
                            sommetId = Convert.ToUInt32(cmdFind.ExecuteScalar());
                        }

                        using (var cmdInsert = new MySqlCommand(sqlEtape, conn, transaction))
                        {
                            cmdInsert.Parameters.AddWithValue("@tournee_id", tourId);
                            cmdInsert.Parameters.AddWithValue("@ordre", i);
                            cmdInsert.Parameters.AddWithValue("@sommet_id", sommetId);
                            cmdInsert.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
            return tourId;
        }

        /// <summary>
        /// Charge depuis la base de données la tournée identifiée par <paramref name="id"/>
        /// et renvoie une instance de la classe <see cref="Tour"/>.
        /// </summary>
        /// <param name="id">Identifiant de la tournée à charger.</param>
        /// <returns>Instance de <see cref="Tour"/> reconstituée.</returns>
        public Tour LoadTour(uint id)
        {
            float coutTotal = 0;
            List<string> orderedCities = new List<string>();

            using (var conn = OpenConnection())
            {
                // 1. Charger la Tournée
                string sqlTournee = "SELECT cout_total FROM Tournee WHERE id = @id;";
                using (var cmd = new MySqlCommand(sqlTournee, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            coutTotal = reader.GetFloat("cout_total");
                        }
                        else
                        {
                            throw new ArgumentException($"Tournée avec l'ID {id} introuvable.");
                        }
                    }
                }

                // 2. Charger les étapes dans le bon ordre
                string sqlEtape = @"
                    SELECT S.nom 
                    FROM EtapeTournee ET 
                    JOIN Sommet S ON ET.sommet_id = S.id 
                    WHERE ET.tournee_id = @id 
                    ORDER BY ET.numero_ordre ASC;";

                using (var cmd = new MySqlCommand(sqlEtape, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            orderedCities.Add(reader.GetString("nom"));
                        }
                    }
                }
            }

            return new Tour(orderedCities, coutTotal);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Méthodes utilitaires privées
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Crée et retourne une nouvelle connexion MySQL ouverte.
        /// </summary>
        private MySqlConnection OpenConnection()
        {
            var conn = new MySqlConnection(_connectionString);
            conn.Open();
            return conn;
        }
    }
}