-- Script d'initialisation de la base de données pour TourneeFutee
-- À exécuter une seule fois pour créer les tables

-- Créer la base de données si elle n'existe pas
CREATE DATABASE IF NOT EXISTS tourneefutee_test;
USE tourneefutee_test;

-- Table Graphe
CREATE TABLE IF NOT EXISTS Graphe (
  id INT AUTO_INCREMENT PRIMARY KEY,
  est_oriente TINYINT NOT NULL DEFAULT 0,
  date_creation TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Table Sommet
CREATE TABLE IF NOT EXISTS Sommet (
  id INT AUTO_INCREMENT PRIMARY KEY,
  graphe_id INT NOT NULL,
  nom VARCHAR(255) NOT NULL,
  valeur FLOAT DEFAULT 0,
  indice INT NOT NULL,
  FOREIGN KEY (graphe_id) REFERENCES Graphe(id) ON DELETE CASCADE,
  UNIQUE KEY unique_sommet_per_graph (graphe_id, nom)
);

-- Table Arc
CREATE TABLE IF NOT EXISTS Arc (
  id INT AUTO_INCREMENT PRIMARY KEY,
  graphe_id INT NOT NULL,
  sommet_source INT NOT NULL,
  sommet_dest INT NOT NULL,
  poids FLOAT NOT NULL,
  FOREIGN KEY (graphe_id) REFERENCES Graphe(id) ON DELETE CASCADE,
  FOREIGN KEY (sommet_source) REFERENCES Sommet(id) ON DELETE CASCADE,
  FOREIGN KEY (sommet_dest) REFERENCES Sommet(id) ON DELETE CASCADE,
  UNIQUE KEY unique_arc (graphe_id, sommet_source, sommet_dest)
);

-- Table Tournee
CREATE TABLE IF NOT EXISTS Tournee (
  id INT AUTO_INCREMENT PRIMARY KEY,
  graphe_id INT NOT NULL,
  cout_total FLOAT NOT NULL,
  date_creation TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (graphe_id) REFERENCES Graphe(id) ON DELETE CASCADE
);

-- Table EtapeTournee
CREATE TABLE IF NOT EXISTS EtapeTournee (
  id INT AUTO_INCREMENT PRIMARY KEY,
  tournee_id INT NOT NULL,
  numero_ordre INT NOT NULL,
  sommet_id INT NOT NULL,
  FOREIGN KEY (tournee_id) REFERENCES Tournee(id) ON DELETE CASCADE,
  FOREIGN KEY (sommet_id) REFERENCES Sommet(id) ON DELETE CASCADE,
  UNIQUE KEY unique_etape (tournee_id, numero_ordre)
);

-- Créer les index pour les performances
CREATE INDEX idx_sommet_graphe ON Sommet(graphe_id);
CREATE INDEX idx_arc_graphe ON Arc(graphe_id);
CREATE INDEX idx_arc_source ON Arc(sommet_source);
CREATE INDEX idx_arc_dest ON Arc(sommet_dest);
CREATE INDEX idx_tournee_graphe ON Tournee(graphe_id);
CREATE INDEX idx_etape_tournee ON EtapeTournee(tournee_id);