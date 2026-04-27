// Création du graphe
using TourneeFutee;

Graph metroGraph = new Graph(false, float.PositiveInfinity);

// Ajout des stations
metroGraph.AddVertex("Châtelet");
metroGraph.AddVertex("Les Halles");
metroGraph.AddVertex("Gare du Nord");
metroGraph.AddVertex("Saint-Michel");

// Ajout des temps de trajet
metroGraph.AddEdge("Châtelet", "Les Halles", 60);
metroGraph.AddEdge("Châtelet", "Saint-Michel", 120);
metroGraph.AddEdge("Les Halles", "Gare du Nord", 300);
// AJOUT IMPORTANT : on relie les culs-de-sac pour fermer la boucle !
metroGraph.AddEdge("Gare du Nord", "Saint-Michel", 450);

// Connexion à la BDD
ServicePersistance db = new ServicePersistance("127.0.0.1", "new_schema", "root", "3003");

// Sauvegarde du graphe
uint idMetro = db.SaveGraph(metroGraph);
Console.WriteLine($"Graphe du métro sauvegardé avec l'ID : {idMetro}");

// Lancement de l'algorithme de Little
Little littleAlgo = new Little(metroGraph);
Tour meilleurTrajet = littleAlgo.ComputeOptimalTour();

// AJOUT IMPORTANT : On vérifie que la tournée existe avant de sauvegarder
if (meilleurTrajet != null)
{
    Console.WriteLine("Tournée trouvée !");
    meilleurTrajet.Print(); // Affiche le résultat dans la console

    uint idTournee = db.SaveTour(idMetro, meilleurTrajet);
    Console.WriteLine($"Tournée sauvegardée avec l'ID : {idTournee}");
}
else
{
    Console.WriteLine("Aucune tournée optimale trouvée : le graphe ne permet pas de faire une boucle fermée.");
}