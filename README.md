Readme Sequencies Hofherr PIP3 04.03.2026

Spielinformation:

Das Game wird über die Scene im Scenes/Menu Ordner gestartet. WICHTIG!

Der Gamer steuert ein fast blindes Medium, welches ein Haunted House befreien soll.
Geräusche werden hier zu licht, Der Gamer sieht. Geräusche wiederum ziehen Ghosts an,
und rauben dem Player Energie.
Der Player muss Objekte suchen mit denen er werfen und Geräusche erzeugen kann.
Des weiteren kann er Klatschen.
Unterschiedliches Material erzeugt unterschiedliche Geräusche.
Bisher sind nur Steingeräusche eingepflegt, dies kann aber problemlos erweitert werden.

Ein spezielles Objekt ist ein Kreuz, welches dem Enemy schaden zufügt.
Es ist nur einmal vorhanden und wird mit der Anwendung schwächer, Bis es verschwindet.
Ein weiteres ist der Drink, damit kann der Player sich heilen.

Zur Auswahl im Inventar das Icon anklicken, welches geworfen werden soll. 
Ist das Item leer, wird das nächstliegende automatisch ausgewählt. 
Geworfenes kann wieder aufgesammelt werden.

Wichtig:
Speicherfunktionen sind nicht implementiert.
Level 2 -> SC_Level_02 ist (wahrscheinlich) nicht vollständig
Level 3 -> Ein Duplikat von Level 1
Dies konnte aus Zeitgründen nicht mehr fertig gestellt werden.


Steuerung:

Movement. "WASD", alternativ "Pfeiltasten"
Klatschen. "Space" Taste
Türe öffnen: "E" Taste.
Zielen und werfen, Objekt auswählen im Inventar: "linke Maustaste"


Dateiinformationen:
Unity Projektordner

Im Ordner Assets befindet sich folgende Ordnerstruktur:

Animations:
	Door		Animation der Holztür
	Explosion	Animation der Explosion
	Player		Player movement Animation
	
ART
	Sprites
		HealthBars			Sprites, Fills für die HP Anzeige (selbst erstellt)
		Inventory			Grosse Sprites, für die Inventory Darstellung (selbst erstellt)
		Items				Sprites für sammelbare Items und Projectile (selbst erstellt)
		PenzillaGhost		Ghost Sprite aus PenzillaGhost Tilesheet extrahiert
		PixelArtTDBasic_MH	Sprites, aus PixelArtTDBasic, extrahiert, Farbe geändert, für die Tree01 - 02 prefabs
		TinyRPGForest		Player Sprites (blau eingefärbt)
		
	Tilesheets
		Door				Door Spritesheet (selbst erstellt, aus Tür vom HousePack)
		MoonriverExplosion	Explosion Tilesheet
		PixelArtTDBasic_MH	
			TX_GrassAssets	Tilesheet für TilePalette -> TM_TX_Grass (Farbe angepasst)
			TX_PlantAssets	Tilesheet für TilePalette -> TM_TX_Plant (Farbe angepasst, Nur noch Büsche. 
																	  Bäume für Sprites/PixelArtTDBasic_MH extrahiert)
			TX_WallAssets	Tilesheet für Tilepalette -> TM_TX_Wall (massiv erweitert und angeordnet)
			TempMap			Tilesheet für Sequencies Tile Palette (nur als "Sandbox" in der Scene _Template verwendet)
		TM_HousePack_MH		House Tilesheet (überarbeitet)
		TM_RF_Castle_MH		Castle Tilesheet (massiv überarbeitet)
		
	UI_Images	Eigenes BG Foto, mit chatGPT zum Pixelart gemacht. InventoryBar (weiter verarbeitet) 
	
Fonts			Cinzel Font. Im game verwendeter Font.

Media
	Audio		Mix aus eigenen und heruntergeladenen Audiofiles (alle einheitlich auf 16Bit 48KHz -6dB)
	
Prefabs
	Character	
		Ghost	Ghost Prefab	
		Player	Player Prefab
		
	Items		Items Prefabs		(sammelbaren gegenstände)
	Projectiles	Projectiles Prefabs (werfbare Gegenstände)
	World		Door, Trees, Windows, Explosion
	
Scenes
	_Template	"SandBox" Scene, zum testen der Fake Physik und anderer Game Mechaniken (nicht im Build!)
	
	Levels
		SC_Level_01		Stage01, außerhalb des Gebäudes, auf dem Grundstück
		SC_Level_02		Stage02, innerhalb des Erdgeschosses
		SC_Level_02		Stage03, innerhalb des ersten Stockes
	
	Menu		MainMenu. Start des Games!
	System		SC_GameOver und SC_Winner
	
Scripts
	Audio		Audio relevante Scripte
	Camera		Kamera relevante Scripte
	Characters
		Enemy		Enemy relevante Scripte
		Player		Player relevante Scripte
	Gameplay
		Inventory	Invetar relevante Scripte
		Projectile	Projektil relevante Scripte
		System		System relevante Scripte
		World		GamePlay relevante Systeme
		

Für alle verwendeten Medien wie Tilesheets, sprites, Audio u.w. siehe mitgelieferten "Quellennachweis"

Persistenz

In jeder Levelrunde werden relevante Spieldaten lokal gespeichert.
Die Persistenten Instanzen sorgen dafür, das Die Levels bestimmte Eigenschaften übernehmen.
Dies betrifft das Inventar, Player und Enemy health und das Audiosystem.

Die Scripte sind weitgehend modular aufgebaut.
Als Beispiel verwaltet das Script ThrowableFlight.cs die Landeposition eines geworfenen Objekts und sendet 
entsprechende Events an das persistente FX_SoundSystem, um passende Geräusche abzuspielen.
Hier findet auch die Fake Physik statt. Teile der Physik Parameter werden von PlayerThrower.cs (am Player)
übermittel an ThrowableFlight.cs (die hängt an den Projektilen)

Viel Spaß

LG
Matthias