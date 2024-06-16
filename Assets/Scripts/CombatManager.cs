using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;

    void Awake()
    {
        /* 
        Inicializa la instancia del CombatManager asegurando que solo haya 
        una instancia activa en el juego.
        */
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Attack(TacticsMove attacker, TacticsMove defender)
    {
        /* 
        Maneja el ataque de un atacante a un defensor.
        El atacante gira hacia el defensor, el defensor pierde salud,
        y si su salud llega a cero, se elimina del juego.
        */
        Vector3 directionToTarget = defender.transform.position - attacker.transform.position;
        directionToTarget.y = 0; // Mantener la rotación en el plano horizontal
        attacker.transform.rotation = Quaternion.LookRotation(directionToTarget);

        int damage = attacker.characterStats.basicDamage;
        defender.characterStats.health -= damage;
        defender.UpdateHealthUI();

        if (defender.characterStats.health <= 0)
        {
            TurnManager.RemoveUnit(defender);
             StartCoroutine(DestroyAfterDelay(defender.gameObject, 1f));
        }

        // Ajustar la rotación del atacante para que mire en una dirección recta (norte, sur, este, oeste)
        AdjustRotation(attacker);
        //attacker.StartCoroutine(WaitAndEndTurn(attacker, 1f));
    }

    private void AdjustRotation(TacticsMove unit)
    {
        /* 
        Ajusta la rotación del personaje para que mire en una dirección 
        recta (norte, sur, este, oeste).
        */
        Vector3 direction = unit.transform.forward;
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
        {
            direction.z = 0;
            direction.x = Mathf.Sign(direction.x);
        }
        else
        {
            direction.x = 0;
            direction.z = Mathf.Sign(direction.z);
        }
        unit.transform.rotation = Quaternion.LookRotation(direction);
    }

    public void AttackWithPierce(TacticsMove attacker, TacticsMove defender)
    {
        /* 
        Maneja un ataque de tipo "Pierce" de un atacante a un defensor.
        El atacante gira hacia el defensor, y se calcula el daño infligido
        a todos los personajes en línea recta entre el atacante y el defensor.
        */
        Vector3 direction = (defender.transform.position - attacker.transform.position).normalized;
        float distance = Vector3.Distance(attacker.transform.position, defender.transform.position);

        direction.y = 0; // Mantener la rotación en el plano horizontal
        attacker.transform.rotation = Quaternion.LookRotation(direction);

        int layerMask = LayerMask.GetMask("Default", "Player", "NPC");

        RaycastHit[] hits = Physics.RaycastAll(attacker.transform.position, direction, distance, layerMask);

        foreach (RaycastHit hit in hits)
        {
            TacticsMove target = hit.collider.GetComponent<TacticsMove>();
            if (target != null)
            {
                int damage = attacker.characterStats.basicDamage;
                target.characterStats.health -= damage;
                target.UpdateHealthUI();

                if (target.characterStats.health <= 0)
                {
                    TurnManager.RemoveUnit(target);
                    StartCoroutine(DestroyAfterDelay(target.gameObject, 1f));
                }
            }
            //attacker.StartCoroutine(WaitAndEndTurn(attacker, 1f));
        }
    }

    private IEnumerator DestroyAfterDelay(GameObject target, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(target);
    }

    /*private IEnumerator WaitAndEndTurn(TacticsMove unit, float delay)
    {
        yield return new WaitForSeconds(delay);
        unit.EndTurn();
    }*/


}
