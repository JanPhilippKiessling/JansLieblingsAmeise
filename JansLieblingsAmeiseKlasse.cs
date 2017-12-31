

using AntMe.Deutsch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;




namespace AntMe.Player.JansLieblingsAmeise
{
    /// <summary>
    /// Diese Datei enthält die Beschreibung für deine Ameise. Die einzelnen Code-Blöcke 
    /// (Beginnend mit "public override void") fassen zusammen, wie deine Ameise in den 
    /// entsprechenden Situationen reagieren soll. Welche Befehle du hier verwenden kannst, 
    /// findest du auf der Befehlsübersicht im Wiki (http://wiki.antme.net/de/API1:Befehlsliste).
    /// 
    /// Wenn du etwas Unterstützung bei der Erstellung einer Ameise brauchst, findest du
    /// in den AntMe!-Lektionen ein paar Schritt-für-Schritt Anleitungen.
    /// (http://wiki.antme.net/de/Lektionen)
    /// </summary>
    [Spieler(
        Volkname = "JansLieblingsAmeise",   // Hier kannst du den Namen des Volkes festlegen
        Vorname = "Jack",       // An dieser Stelle kannst du dich als Schöpfer der Ameise eintragen
        Nachname = "OfAllTrades"       // An dieser Stelle kannst du dich als Schöpfer der Ameise eintragen
    )]

    /// Kasten stellen "Berufsgruppen" innerhalb deines Ameisenvolkes dar. Du kannst hier mit
    /// den Fähigkeiten einzelner Ameisen arbeiten. Wie genau das funktioniert kannst du der 
    /// Lektion zur Spezialisierung von Ameisen entnehmen (http://wiki.antme.net/de/Lektion7).
    [Kaste(
        Name = "Standard",                  // Name der Berufsgruppe
        AngriffModifikator = 0,             // Angriffsstärke einer Ameise
        DrehgeschwindigkeitModifikator = 0, // Drehgeschwindigkeit einer Ameise
        EnergieModifikator = 0,             // Lebensenergie einer Ameise
        GeschwindigkeitModifikator = 0,     // Laufgeschwindigkeit einer Ameise
        LastModifikator = 0,                // Tragkraft einer Ameise
        ReichweiteModifikator = 0,          // Ausdauer einer Ameise
        SichtweiteModifikator = 0           // Sichtweite einer Ameise
    )]
    public class JansLieblingsAmeiseKlasse : Basisameise
    {
        bool bLockedOnAmeise = false;
        Ameise BuddyLockedTo;
        teDistanceControlState DistanceControlState;
        int TicksSinceBirth = 0;
        int DistanceControler_StartZeitMessung = 0;


        enum teEntdeckung
        {
            eFreundliche_Ameise_Gefunden,
            eZucker_Gefunden,
            eApfel_Gefunden
        };
        #region Kasten

        /// <summary>
        /// Jedes mal, wenn eine neue Ameise geboren wird, muss ihre Berufsgruppe
        /// bestimmt werden. Das kannst du mit Hilfe dieses Rückgabewertes dieser 
        /// Methode steuern.
        /// Weitere Infos unter http://wiki.antme.net/de/API1:BestimmeKaste
        /// </summary>
        /// <param name="anzahl">Anzahl Ameisen pro Kaste</param>
        /// <returns>Name der Kaste zu der die geborene Ameise gehören soll</returns>
        public override string BestimmeKaste(Dictionary<string, int> anzahl)
        {
            // Gibt den Namen der betroffenen Kaste zurück.
            return "Standard";
        }

        #endregion

        #region Fortbewegung

        /// <summary>
        /// Wenn die Ameise keinerlei Aufträge hat, wartet sie auf neue Aufgaben. Um dir das 
        /// mitzuteilen, wird diese Methode hier aufgerufen.
        /// Weitere Infos unter http://wiki.antme.net/de/API1:Wartet
        /// </summary>
        public override void Wartet()
        {
            GeheGeradeaus();
        }

        /// <summary>
        /// Erreicht eine Ameise ein drittel ihrer Laufreichweite, wird diese Methode aufgerufen.
        /// Weitere Infos unter http://wiki.antme.net/de/API1:WirdM%C3%BCde
        /// </summary>
        public override void WirdMüde()
        {
        }

        /// <summary>
        /// Wenn eine Ameise stirbt, wird diese Methode aufgerufen. Man erfährt dadurch, wie 
        /// die Ameise gestorben ist. Die Ameise kann zu diesem Zeitpunkt aber keinerlei Aktion 
        /// mehr ausführen.
        /// Weitere Infos unter http://wiki.antme.net/de/API1:IstGestorben
        /// </summary>
        /// <param name="todesart">Art des Todes</param>
        public override void IstGestorben(Todesart todesart)
        {
        }


        enum teDistanceControlState
        {
            Starte_EntferungsMessung,
            Messe_Entferung,
            BewegeVonAmeiseWeg,
            BewegeAufAmeiseZu,
            WarteWaehrendDieAmeiseLaeuft,
            ende,
        };

        public int GetSysTime()
        {
            return TicksSinceBirth;
        }

        private void vFsm_ControlDistanceToLockedBuddys()
        {
            /*
           bLockedOnAmeise 
           BuddyLockedTo;
           */
            /*
                @startuml 

parse das hier zeile für zeile und suche "A -> B : C"  raus um Zustandübergänge mit (kommentar) Bedingungen zu generieren
Wenn nur A : D da steht, kommt das als kommentar in den case
das enum kann dazu auch gleich generiert werden, indem man alles hinter -> und vor : bzw \n einmal nimmt

                title Abstand zu den anderen Ameisen regeln
                [*] --> Starte_EntferungsMessung

                Starte_EntferungsMessung: MoveTo(BuddyAmeise)

                Starte_EntferungsMessung -> Messung_Entferung

                Messe_Entferung -> BewegeAufAmeiseZu: Abstand >= 20
                Messe_Entferung -> BewegeVonAmeiseWeg: Abstand <= 15
                Messe_Entferung -> ende : 15 < Abstand < 20

                BewegeVonAmeiseWeg: Turn(180 degree)

                BewegeAufAmeiseZu -> WarteWaehrendDieAmeiseLaeuft
                BewegeVonAmeiseWeg -> WarteWaehrendDieAmeiseLaeuft

                WarteWaehrendDieAmeiseLaeuft -> Starte_Messung : 5 Ticks vorbei

                ende: BuddyAmeise = NextBuddyAmeise
                
                @enduml
             * */

            // das gehört eigentlich in ein eigenes objekt, dann könnte ich auch jeder locked Ameise einen eigenen regler zuordnen!
            if (bLockedOnAmeise)     // defensive programming
            {
                switch (DistanceControlState)
                {
                    case teDistanceControlState.Starte_EntferungsMessung:
                    {
                        DistanceControler_StartZeitMessung = GetSysTime();
                        GeheZuZiel(GetLockedBuddy_CurrentlyControllingDistanceFor());
                        DistanceControlState = teDistanceControlState.Messe_Entferung;
                    }
                    break;

                    case teDistanceControlState.Messe_Entferung:
                    {
                        /*
                        if (GetSysTime() - DistanceControler_StartZeitMessung >= 3)
                        {
                            if ( RestStrecke >= 20 )
                            {
                                DistanceControlState = teDistanceControlState.BewegeAufAmeiseZu;
                            } 
                            else if (RestStrecke <= 15)
                            {
                                DistanceControlState = teDistanceControlState.BewegeVonAmeiseWeg;
                            }
                            else
                            {
                                DistanceControlState = teDistanceControlState.ende;
                            }
                        }*/
                    }
                    break;

                    case teDistanceControlState.BewegeVonAmeiseWeg:
                    {
                        GeheZuZiel(GetLockedBuddy_CurrentlyControllingDistanceFor());
                        DreheUmWinkel(180);
                        DistanceControler_StartZeitMessung = GetSysTime();
                        DistanceControlState = teDistanceControlState.WarteWaehrendDieAmeiseLaeuft;
                    }
                    break;

                    case teDistanceControlState.BewegeAufAmeiseZu:
                    {
                        GeheZuZiel(GetLockedBuddy_CurrentlyControllingDistanceFor());
                        DistanceControler_StartZeitMessung = GetSysTime();
                        DistanceControlState = teDistanceControlState.WarteWaehrendDieAmeiseLaeuft;
                    }
                    break;

                    case teDistanceControlState.WarteWaehrendDieAmeiseLaeuft:
                    {
                        if (GetSysTime() - DistanceControler_StartZeitMessung >= 5)
                        {
                            DistanceControlState = teDistanceControlState.Starte_EntferungsMessung;
                        }
                    }
                    break;

                    case teDistanceControlState.ende:
                    {
                        SwitchToNextLockedBuddy_CurrentlyControllingDistanceFor();
                    }
                    break;

                    default:
                    {

                    }break;
                }
            }
            return;
        }

        private void SwitchToNextLockedBuddy_CurrentlyControllingDistanceFor()
        {
            return; // hier wird spaeter die Liste eins weiter geschaltet
        }


        private Ameise GetLockedBuddy_CurrentlyControllingDistanceFor()
        {
            return BuddyLockedTo;
        }

        /// <summary>
        /// Diese Methode wird in jeder Simulationsrunde aufgerufen - ungeachtet von zusätzlichen 
        /// Bedingungen. Dies eignet sich für Aktionen, die unter Bedingungen ausgeführt werden 
        /// sollen, die von den anderen Methoden nicht behandelt werden.
        /// Weitere Infos unter http://wiki.antme.net/de/API1:Tick
        /// </summary>
        public override void Tick()
        {
            TicksSinceBirth++;
            if (bLockedOnAmeise)
            {
                vFsm_ControlDistanceToLockedBuddys();
            }

        }

        #endregion

        #region Nahrung

        /// <summary>
        /// Sobald eine Ameise innerhalb ihres Sichtradius einen Apfel erspäht wird 
        /// diese Methode aufgerufen. Als Parameter kommt das betroffene Stück Obst.
        /// Weitere Infos unter "http://wiki.antme.net/de/API1:Sieht(Obst)"
        /// </summary>
        /// <param name="obst">Das gesichtete Stück Obst</param>
        public override void Sieht(Obst obst)
        {
            if (AktuelleLast == 0 && BrauchtNochTräger(obst))
            {
                GeheZuZiel(obst);
            }
        }

        /// <summary>
        /// Sobald eine Ameise innerhalb ihres Sichtradius einen Zuckerhügel erspäht wird 
        /// diese Methode aufgerufen. Als Parameter kommt der betroffene Zuckerghügel.
        /// Weitere Infos unter "http://wiki.antme.net/de/API1:Sieht(Zucker)"
        /// </summary>
        /// <param name="zucker">Der gesichtete Zuckerhügel</param>
        public override void Sieht(Zucker zucker)
        {
            SprüheMarkierung((int)teEntdeckung.eZucker_Gefunden, 100);
            if ( AktuelleLast == 0)
            {
                GeheZuZiel(zucker);
            }
        }

        /// <summary>
        /// Hat die Ameise ein Stück Obst als Ziel festgelegt, wird diese Methode aufgerufen, 
        /// sobald die Ameise ihr Ziel erreicht hat. Ab jetzt ist die Ameise nahe genug um mit 
        /// dem Ziel zu interagieren.
        /// Weitere Infos unter "http://wiki.antme.net/de/API1:ZielErreicht(Obst)"
        /// </summary>
        /// <param name="obst">Das erreichte Stück Obst</param>
        public override void ZielErreicht(Obst obst)
        {
            if (BrauchtNochTräger(obst))
            { 
                Nimm(obst);
                GeheZuBau();
            }
        }

        /// <summary>
        /// Hat die Ameise eine Zuckerhügel als Ziel festgelegt, wird diese Methode aufgerufen, 
        /// sobald die Ameise ihr Ziel erreicht hat. Ab jetzt ist die Ameise nahe genug um mit 
        /// dem Ziel zu interagieren.
        /// Weitere Infos unter "http://wiki.antme.net/de/API1:ZielErreicht(Zucker)"
        /// </summary>
        /// <param name="zucker">Der erreichte Zuckerhügel</param>
        public override void ZielErreicht(Zucker zucker)
        {
            Nimm(zucker);
            GeheZuBau();
        }

        #endregion

        #region Kommunikation

        /// <summary>
        /// Markierungen, die von anderen Ameisen platziert werden, können von befreundeten Ameisen 
        /// gewittert werden. Diese Methode wird aufgerufen, wenn eine Ameise zum ersten Mal eine 
        /// befreundete Markierung riecht.
        /// Weitere Infos unter "http://wiki.antme.net/de/API1:RiechtFreund(Markierung)"
        /// </summary>
        /// <param name="markierung">Die gerochene Markierung</param>
        public override void RiechtFreund(Markierung markierung)
        {
            if (((teEntdeckung)markierung.Information == teEntdeckung.eZucker_Gefunden)
                && (AktuelleLast == 0) )
            {
                GeheZuZiel(markierung);
            }
        }

        /// <summary>
        /// So wie Ameisen unterschiedliche Nahrungsmittel erspähen können, entdecken Sie auch 
        /// andere Spielelemente. Entdeckt die Ameise eine Ameise aus dem eigenen Volk, so 
        /// wird diese Methode aufgerufen.
        /// Weitere Infos unter "http://wiki.antme.net/de/API1:SiehtFreund(Ameise)"
        /// </summary>
        /// <param name="ameise">Erspähte befreundete Ameise</param>
        public override void SiehtFreund(Ameise ameise)
        {
            if ((AktuelleLast == 0))
            {
                GeheZuZiel(ameise);
                bLockedOnAmeise = true;
                BuddyLockedTo = ameise;
            }
        }

        /// <summary>
        /// So wie Ameisen unterschiedliche Nahrungsmittel erspähen können, entdecken Sie auch 
        /// andere Spielelemente. Entdeckt die Ameise eine Ameise aus einem befreundeten Volk 
        /// (Völker im selben Team), so wird diese Methode aufgerufen.
        /// Weitere Infos unter "http://wiki.antme.net/de/API1:SiehtVerb%C3%BCndeten(Ameise)"
        /// </summary>
        /// <param name="ameise">Erspähte verbündete Ameise</param>
        public override void SiehtVerbündeten(Ameise ameise)
        {
        }

        #endregion

        #region Kampf

        /// <summary>
        /// So wie Ameisen unterschiedliche Nahrungsmittel erspähen können, entdecken Sie auch 
        /// andere Spielelemente. Entdeckt die Ameise eine Ameise aus einem feindlichen Volk, 
        /// so wird diese Methode aufgerufen.
        /// Weitere Infos unter "http://wiki.antme.net/de/API1:SiehtFeind(Ameise)"
        /// </summary>
        /// <param name="ameise">Erspähte feindliche Ameise</param>
        public override void SiehtFeind(Ameise ameise)
        {
        }

        /// <summary>
        /// So wie Ameisen unterschiedliche Nahrungsmittel erspähen können, entdecken Sie auch 
        /// andere Spielelemente. Entdeckt die Ameise eine Wanze, so wird diese Methode aufgerufen.
        /// Weitere Infos unter "http://wiki.antme.net/de/API1:SiehtFeind(Wanze)"
        /// </summary>
        /// <param name="wanze">Erspähte Wanze</param>
        public override void SiehtFeind(Wanze wanze)
        {
        }

        /// <summary>
        /// Es kann vorkommen, dass feindliche Lebewesen eine Ameise aktiv angreifen. Sollte 
        /// eine feindliche Ameise angreifen, wird diese Methode hier aufgerufen und die 
        /// Ameise kann entscheiden, wie sie darauf reagieren möchte.
        /// Weitere Infos unter "http://wiki.antme.net/de/API1:WirdAngegriffen(Ameise)"
        /// </summary>
        /// <param name="ameise">Angreifende Ameise</param>
        public override void WirdAngegriffen(Ameise ameise)
        {
        }

        /// <summary>
        /// Es kann vorkommen, dass feindliche Lebewesen eine Ameise aktiv angreifen. Sollte 
        /// eine Wanze angreifen, wird diese Methode hier aufgerufen und die Ameise kann 
        /// entscheiden, wie sie darauf reagieren möchte.
        /// Weitere Infos unter "http://wiki.antme.net/de/API1:WirdAngegriffen(Wanze)"
        /// </summary>
        /// <param name="wanze">Angreifende Wanze</param>
        public override void WirdAngegriffen(Wanze wanze)
        {
        }

        #endregion
    }
}
