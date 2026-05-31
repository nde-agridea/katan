# Katan – Règles du jeu

> Traduction française de `rules.md`.
> Les ambiguïtés et questions ouvertes sont signalées par ⚠️.

---

## Objectif

Le premier joueur à atteindre **10 points de victoire** remporte la partie.

---

## Points de victoire

| Source | Points |
|---|---|
| Colonie | 1 |
| Ville | 2 |
| Carte développement « Point de victoire » | 1 |
| Bonus Plus longue route | 2 |
| Bonus Plus grande armée | 2 |

---

## Contenu du jeu

| Élément | Quantité |
|---|---|
| Tuiles de la carte | 19 (4 forêt, 3 carrière, 3 champ, 3 pâturage, 3 montagne, 2 mine, 1 désert) |
| Jetons numéros | 19 (valeurs 2 à 6 et 8 à 12 pour les tuiles ressources ; valeur 7 pour la tuile désert) |
| Colonies | 25 (5 par joueur) |
| Villes | 20 (4 par joueur) |
| Routes | 75 (15 par joueur) |
| Armées | 50 (10 par joueur) |
| Voleur | 1 |
| Dés | 2 |
| Cartes ressources | (pioche) |
| Cartes développement | (pioche) |

---

## Tuiles et ressources

Chaque tuile produit une ressource spécifique lorsque son numéro est obtenu aux dés.

| Tuile | Ressource |
|---|---|
| Forêt | Bois |
| Carrière | Brique |
| Champ | Blé |
| Pâturage | Mouton |
| Montagne | Pierre |
| Mine | Fer |
| Désert | Aucune |

---

## Carte

La carte de jeu est composée de **19 tuiles hexagonales** disposées selon une forme aléatoire, générée à chaque nouvelle partie. Contrairement à une disposition fixe, la forme varie et doit respecter les contraintes ci-dessous.

### Contraintes de forme

- Les 19 tuiles doivent former **un seul groupe contigu et connecté** — aucun groupe isolé n'est autorisé.
- La carte ne doit comporter **aucun trou fermé** (aucun espace vide entièrement entouré de tuiles).
- **Aucune péninsule étroite ne peut dépasser 2 tuiles de longueur.** Une péninsule étroite est une chaîne de tuiles dans laquelle chaque tuile intérieure n'a exactement que 2 voisins.

### Placement du désert

La tuile désert est placée après que la forme générale a été déterminée.

- Le désert **ne doit pas être une tuile de bord**. Une tuile de bord est une tuile ayant au moins un côté exposé (moins de 6 voisins dans la carte).
- Autrement dit, le désert doit avoir ses 6 voisins tous occupés par d'autres tuiles.

### Placement des jetons numéros

- Chacune des 18 tuiles non-désert reçoit un jeton numéro parmi l'ensemble {2, 3, 4, 5, 6, 8, 9, 10, 11, 12}.
- La tuile désert reçoit le **jeton 7**. Ce jeton est purement visuel : il ne produit aucune ressource et n'entraîne aucun effet supplémentaire. Le mécanisme du voleur est déclenché par tout résultat de 7, indépendamment de ce jeton.
- **Règle d'adjacence des jetons à haute valeur** : aucune paire de tuiles appartenant à l'ensemble {5, 6, 8, 9} ne peut être adjacente.

### Distribution des tuiles ressources

Les 18 tuiles ressources non-désert sont distribuées aléatoirement, avec une seule contrainte :

- **Aucun groupe de trois tuiles mutuellement adjacentes ne peut être entièrement du même type de ressource** (contrainte de triangle).

### Ports

La carte comporte **8 ports** : 6 ports spécialisés (2:1, un par type de ressource) et 2 ports génériques (3:1).

- Les ports sont placés sur les côtés exposés des tuiles côtières (tuiles ayant au moins un côté exposé).
- **Deux ports ne peuvent pas être placés sur des côtés côtiers adjacents.**
- L'attribution des ressources aux ports spécialisés est aléatoire.

### Validation de la carte

Après la génération de la carte par le serveur, tous les joueurs voient la carte complète avant le début de la phase de placement initial.

- Les joueurs votent pour accepter ou rejeter la carte. Un **vote majoritaire** rejette la carte et déclenche une nouvelle génération.
- La carte peut être régénérée au maximum **3 fois**. Après la troisième régénération, la carte obtenue est utilisée quelle que soit l'issue du vote.

---

## Mise en place

La partie commence avec le **joueur le plus grand**. Le jeu se déroule dans le **sens des aiguilles d'une montre**.

### Phase de placement initial

Avant le premier tour, les joueurs placent leurs pièces de départ dans un **ordre en serpentin** : un premier passage dans le sens des aiguilles d'une montre (du premier au dernier joueur), puis un passage en sens inverse (du dernier au premier). Chaque joueur place donc deux fois au total.

Lorsque c'est au tour d'un joueur de placer :

1. Placer **1 colonie** sur un croisement de tuiles au choix.
2. Placer **2 routes**, chacune reliée à la colonie ou au réseau de routes du joueur.
3. Recevoir **1 carte ressource pour chaque tuile adjacente à la colonie placée** (les tuiles désert ne rapportent rien).

Tous les placements sont **gratuits** (aucun coût en ressources). Il n'y a **pas de règle de distance** — une colonie peut être placée à côté de n'importe quelle autre colonie.

---

## Déroulement d'un tour

Chaque tour se déroule dans l'ordre suivant :

### 1. Lancer les dés

Le joueur actif lance 2 dés et additionne les deux résultats.

#### Résultat normal (2–6, 8–12)

Toutes les tuiles dont le jeton numéro correspond au résultat produisent des ressources. Chaque joueur possédant une colonie ou une ville sur ces tuiles reçoit :

- **1 ressource** par colonie
- **2 ressources** par ville

La tuile où se trouve le **voleur** ne produit **aucune ressource**, quel que soit le résultat.

Après la distribution des ressources, **le tribut du voleur est collecté** : tout joueur possédant une colonie ou une ville sur la tuile du voleur doit céder 1 ressource de son choix (voir [Voleur](#voleur)).

#### Résultat 7

1. Tout joueur possédant **plus de 7 cartes ressources** doit en défausser la moitié, arrondie à l'inférieur.
2. Le joueur actif déplace le **voleur** sur une tuile de son choix (voir [Voleur](#voleur)).

### 2. Échanges (optionnel)

Le joueur actif peut échanger des ressources de l'une ou plusieurs des façons suivantes :

- **Échange entre joueurs** : le joueur actif propose des échanges aux autres joueurs ; ceux-ci peuvent accepter ou refuser. Seul le joueur actif peut initier une offre — les joueurs non actifs ne peuvent pas faire de contre-proposition.
- **Échange avec la banque** : 4 ressources identiques contre 1 ressource au choix (4:1).
- **Échange via un port** : utiliser un port adjacent à l'une de ses colonies ou villes (voir [Ports](#ports)).

### 3. Construire et acheter (optionnel)

Le joueur actif peut dépenser des ressources pour construire des bâtiments ou acheter des cartes développement (voir [Coûts de construction](#coûts-de-construction)).

### 4. Fin de tour

Le tour passe au joueur suivant dans le sens des aiguilles d'une montre.

---

## Ports

Pour utiliser un port, un joueur doit posséder une colonie ou une ville sur une tuile adjacente à ce port.

| Type de port | Taux |
|---|---|
| Port générique (3:1) | Échanger 3 ressources identiques contre 1 ressource au choix |
| Port spécialisé (2:1) | Échanger 2 ressources d'un type spécifique contre 1 ressource au choix |

Il existe un port spécialisé par type de ressource (6 au total) et 2 ports génériques. Les positions des ports et l'attribution des ressources aux ports spécialisés sont déterminées aléatoirement lors de la génération de la carte (voir [Carte](#carte)).

---

## Coûts de construction

| Construction | Coût |
|---|---|
| Route | 1 Bois + 1 Brique |
| Colonie | 2 Bois + 1 Brique + 1 Blé + 1 Mouton |
| Ville | 3 Pierre + 2 Blé |
| Armée | 2 Fer + 1 Blé |
| Carte développement | 1 Pierre + 1 Mouton + 1 Fer |

### Règles de placement

**Colonies**
- Doivent être placées sur un croisement de tuiles.
- À l'exception de la toute première colonie posée en début de partie, une colonie doit être reliée à une autre colonie du joueur par une route.

**Villes**
- Construites en améliorant une colonie existante. La colonie est remplacée par la ville.

**Routes**
- Relient les colonies et les villes entre elles.

**Armées**
- Doivent être placées sur une tuile où le joueur possède déjà une colonie ou une ville.
- Chaque joueur dispose d'un maximum de **10 armées**. La construction est bloquée une fois cette limite atteinte. Les armées détruites au combat retournent dans la réserve du joueur.

---

## Cartes développement

Les cartes développement peuvent être achetées pendant le tour d'un joueur. Une carte achetée doit être utilisée immédiatement lors de ce même tour — elle ne peut pas être conservée pour un tour ultérieur.

Lorsque la dernière carte est piochée, l'ensemble des 25 cartes est immédiatement remélangé pour former une nouvelle pioche.

### Types de cartes

#### Chevalier *(11 cartes)*

Le joueur déplace immédiatement le voleur sur une tuile de son choix. Il peut voler une carte ressource au hasard à un joueur possédant une colonie ou une ville sur cette tuile.

#### Point de victoire *(5 cartes)*

Donne 1 point de victoire.

#### Construction de route *(3 cartes)*

Donne 1 route gratuite, placée immédiatement.

#### Monopole *(2 cartes)*

Le joueur choisit un type de ressource. Chaque autre joueur doit lui donner une carte de cette ressource. Les joueurs qui n'en ont pas ne donnent rien.

#### Excédent *(3 cartes)*

Le joueur reçoit 2 cartes ressources de son choix depuis la banque.

#### Catastrophe *(1 carte)*

Le joueur choisit une tuile de la carte.
- Si une colonie ou une ville s'y trouve, elle est détruite.
- Toutes les armées présentes sur cette tuile sont divisées par deux, arrondies à l'inférieur.

---

## Voleur

Le voleur est déplacé lorsqu'un joueur obtient **7** aux dés ou joue une carte **Chevalier**.

- La tuile occupée par le voleur **ne produit aucune ressource**.
- **Lors du premier placement sur une tuile** : si un autre joueur possède une colonie ou une ville sur cette tuile, le joueur qui a déplacé le voleur peut lui voler une carte ressource au hasard. Ce vol n'a lieu qu'une seule fois par placement.
- **À chaque tour suivant** : tant que le voleur reste sur une tuile, tout joueur possédant une colonie ou une ville sur cette tuile doit céder une ressource de son choix **après la production des ressources** de son tour (c'est-à-dire après la distribution des ressources dues au lancer de dés, mais avant les échanges).

---

## Armées

### Construction

Une armée est construite en dépensant **2 Fer + 1 Blé**. Elle doit être placée sur une tuile où le joueur possède une colonie ou une ville.

### Propriété des tuiles

Les tuiles démarrent la partie comme **non revendiquées**. Une tuile est revendiquée lorsqu'un joueur y place une armée pour la première fois — en la construisant ou en y déplaçant une armée. Une fois revendiquée, une tuile reste la propriété du joueur et ne peut jamais redevenir non revendiquée. La propriété est transférée au vainqueur après un combat réussi.

### Déplacement

Un joueur peut déplacer autant d'armées qu'il le souhaite pendant son tour, avant ou après une attaque. Une armée peut se déplacer d'une tuile vers une tuile adjacente, à condition que la destination :

- appartienne au joueur, **ou**
- contienne une colonie ou une ville du joueur, **ou**
- soit non revendiquée (s'y déplacer la revendique).

### Combat

Un joueur peut attaquer une tuile adjacente contenant des armées ennemies.

**Lancer d'attaque :**
- Chaque armée attaquante et défenseure contribue **1 dé**.
- Les deux camps lancent tous leurs dés simultanément et additionnent les résultats.
- Le camp avec le total le plus élevé gagne l'échange. **En cas d'égalité, le défenseur l'emporte.**
- Le combat est résolu en **un seul tour de dés**.

**Si l'attaquant gagne** (toutes les armées du défenseur sont éliminées) :
- La tuile est conquise.
- Si la tuile contenait une colonie ou une ville du défenseur, elle devient celle de l'attaquant.
- Si la tuile ne contenait pas de colonie ou de ville, l'attaquant peut en construire une.
- L'attaquant doit déplacer au moins 1 armée sur la tuile conquise.

**Si l'attaque échoue :**
- L'attaquant perd la moitié de ses armées attaquantes, arrondie à l'inférieur.

---

## Bonus spéciaux

Le meneur actuel pour chaque bonus est suivi et **affiché en temps réel pendant la partie** pour tous les joueurs. Les **+2 points de victoire ne sont attribués qu'en fin de partie**, lorsqu'un joueur atteint 10 points de victoire.

### Plus longue route

Le joueur possédant la route continue la plus longue reçoit **+2 points de victoire**. La plus longue route est le plus long chemin sans répétition dans le réseau de routes du joueur (calculé par parcours en profondeur). Les embranchements ne sont pas cumulés — seul le chemin unique le plus long est retenu. La colonie ou la ville d'un adversaire sur un croisement **rompt la chaîne de routes** à cet endroit ; les segments de routes de part et d'autre ne peuvent pas être reliés à travers ce croisement.

### Plus grande armée

Le joueur possédant la plus grande armée (le plus grand nombre d'unités) reçoit **+2 points de victoire**.

---

## Fin de partie

La partie se termine immédiatement lorsqu'un joueur atteint **10 points de victoire** à n'importe quel moment de son tour (y compris après la résolution des dés, une construction ou le jeu d'une carte développement).
