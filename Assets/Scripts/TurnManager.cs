using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class TurnManager : MonoBehaviour
{
    static List<TacticsMove> units = new List<TacticsMove>();
    static int currentIndex = 0;
    static bool isPlayerTurn = true;
    static int currentRound = 1;
    static bool isEndingTurn = false;

    public UIManager uiManager;

    void Start()
    {
        /* 
        Inicializa el TurnManager, registrando los indicadores de turno 
        de los personajes y configurando la cola de turnos.
        */
        string[] characterNames = new string[] { "Nekomaru", "Aya", "Umi", "Gaku", "Yuniti", "Hanami", "Flynn", "Kuroka" };
        for (int i = 0; i < characterNames.Length; i++)
        {
            UIManager.Instance.RegisterCharacterTurnIndicator(characterNames[i], i);
        }
        
        InitTurnQueue();
        UIManager.Instance.UpdateRoundText(currentRound);
    }

    void Update()
    {
        /* 
        Si no hay unidades en la cola, la inicializa.
        */
        if (units.Count == 0)
        {
            InitTurnQueue();
        }
    }

    static void InitTurnQueue()
    {
        /* 
        Ordena las unidades por velocidad y comienza el turno de la 
        primera unidad en la cola.
        */
        units = units.OrderByDescending(unit => unit.characterStats.speed).ToList();
        currentIndex = 0;
        isPlayerTurn = true;

        if (units.Count > 0)
        {
            StartTurn();
        }
    }

    public static void StartTurn()
    {
        /* 
        Inicia el turno de la unidad actual, mostrando u ocultando los
        controles del jugador según corresponda.
        */
        isEndingTurn = false;

        if (units[currentIndex] is PlayerMove)
        {
            UIManager.Instance.ShowPlayerControls();
            UIManager.Instance.SetCurrentPlayerMove(units[currentIndex] as PlayerMove);
        }
        else
        {
            UIManager.Instance.HidePlayerControls();
        }

        units[currentIndex].BeginTurn();
        UIManager.Instance.UpdateTurnFrame(units[currentIndex].characterStats.name);
    }

    public static void EndTurn()
    {
        /* 
        Finaliza el turno de la unidad actual, actualiza el índice de la
        unidad actual y verifica el estado del juego.
        */
        if (isEndingTurn) return;
        isEndingTurn = true;

        if (units.Count == 0)
        {
            isEndingTurn = false;
            return;
        }

        units[currentIndex].EndTurn();
        currentIndex++;

        if (currentIndex >= units.Count)
        {
            currentIndex = 0;
            currentRound++;
            UIManager.Instance.UpdateRoundText(currentRound);
        }

        CheckGameOver();
        isEndingTurn = false;

        if (units.Count > 0)
        {
            StartTurn();
        }
    }

    public static void AddUnit(TacticsMove unit)
    {
        /* 
        Añade una unidad a la lista y la ordena por velocidad.
        */
        units.Add(unit);
        units = units.OrderByDescending(u => u.characterStats.speed).ToList();
    }

    public static void RemoveUnit(TacticsMove unit)
    {
        /* 
        Elimina una unidad de la lista y ajusta el índice de la unidad
        actual según corresponda.
        */
        int index = units.IndexOf(unit);
        if (index != -1)
        {
            units.RemoveAt(index);

            if (index < currentIndex)
            {
                currentIndex--;
            }
            else if (index == currentIndex)
            {
                currentIndex = currentIndex % units.Count;
            }
            
            UIManager.Instance.DeactivateTurnFrame(unit.characterStats.name);
        }
    }

    public static List<TacticsMove> GetUnits()
    {
        /* 
        Devuelve la lista de unidades.
        */
        return units;
    }

    private static void CheckGameOver()
    {
        /* 
        Verifica si todos los jugadores están muertos y muestra la pantalla
        de fin de juego si es así.
        */
        bool allPlayersDead = !units.Any(unit => unit is PlayerMove);
        if (allPlayersDead)
        {
            UIManager.Instance.ShowGameOverScreen();
        }
    }

    public static void ResetTurnManager()
    {
        /* 
        Reinicia el TurnManager a su estado inicial.
        */
        units.Clear();
        currentIndex = 0;
        isPlayerTurn = true;
        currentRound = 1;
        isEndingTurn = false;
    }
}
