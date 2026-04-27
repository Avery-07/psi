DROP TABLE IF EXISTS EtapeTournee;
DROP TABLE IF EXISTS Tournee;
DROP TABLE IF EXISTS Arc;
DROP TABLE IF EXISTS Sommet;
DROP TABLE IF EXISTS Graphe;

CREATE TABLE Graphe (
    id           INT UNSIGNED    NOT NULL AUTO_INCREMENT,
    est_oriente  TINYINT(1)      NOT NULL DEFAULT 0,
    PRIMARY KEY (id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE Sommet (
    id          INT UNSIGNED    NOT NULL AUTO_INCREMENT,
    graphe_id   INT UNSIGNED    NOT NULL,
    nom         VARCHAR(100)    NOT NULL, -- Ex: "Châtelet", "Gare du Nord"
    valeur      FLOAT           NULL,
    indice      INT UNSIGNED    NOT NULL, -- CRITIQUE : pour conserver l'ordre de la matrice
    PRIMARY KEY (id),
    FOREIGN KEY (graphe_id) REFERENCES Graphe(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE Arc (
    id              INT UNSIGNED    NOT NULL AUTO_INCREMENT,
    graphe_id       INT UNSIGNED    NOT NULL,
    sommet_source   INT UNSIGNED    NOT NULL,
    sommet_dest     INT UNSIGNED    NOT NULL,
    poids           FLOAT           NOT NULL, -- Ex: Temps en secondes entre deux stations
    PRIMARY KEY (id),
    FOREIGN KEY (graphe_id)     REFERENCES Graphe(id) ON DELETE CASCADE,
    FOREIGN KEY (sommet_source) REFERENCES Sommet(id) ON DELETE CASCADE,
    FOREIGN KEY (sommet_dest)   REFERENCES Sommet(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE Tournee (
    id          INT UNSIGNED    NOT NULL AUTO_INCREMENT,
    graphe_id   INT UNSIGNED    NOT NULL,
    cout_total  FLOAT           NOT NULL,
    PRIMARY KEY (id),
    FOREIGN KEY (graphe_id) REFERENCES Graphe(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE EtapeTournee (
    tournee_id      INT UNSIGNED    NOT NULL,
    numero_ordre    INT UNSIGNED    NOT NULL, 
    sommet_id       INT UNSIGNED    NOT NULL,
    PRIMARY KEY (tournee_id, numero_ordre),
    FOREIGN KEY (tournee_id) REFERENCES Tournee(id) ON DELETE CASCADE,
    FOREIGN KEY (sommet_id)  REFERENCES Sommet(id)  ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;