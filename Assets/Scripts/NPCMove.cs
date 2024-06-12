using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NPCMove : TacticsMove
{
    GameObject target;

    void Start()
    {
        /* 
        Inicializa el NPC, configurando su estado inicial y actualizando
        su UI de salud.
        */
        Init(false);
        UpdateHealthUI();
        animator = GetComponentInChildren<Animator>();
        animator.SetBool("IsMoving", false);
    }

    void Update()
    {
        /* 
        En cada frame, se dibuja un rayo en la dirección en la que está
        mirando el NPC. Si es su turno, busca el objetivo más cercano y
        decide si atacar o moverse hacia él.
        */
        Debug.DrawRay(transform.position, transform.forward);

        if (!turn)
        {
            return;
        }

        if (!moving)
        {
            FindNearestTarget();

            if (target == null)
            {
                EndTurn();
                return;
            }

            float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

            if (distanceToTarget <= characterStats.attackRange)
            {
                AttackTarget();
            }
            else
            {
                CalculatePath();
                FindSelectableTiles(true);
                actualTargetTile.target = true;
                MoveToTile(actualTargetTile);
                animator.SetBool("IsMoving", true);
            }
        }
        else
        {
            Move();
        }
    }

    void CalculatePath()
    {
        /* 
        Calcula el camino hacia el objetivo utilizando el método FindPath
        de la clase base.
        */
        Tile targetTile = GetTargetTile(target);
        FindPath(targetTile);
    }

    void FindNearestTarget()
    {
        /* 
        Encuentra el objetivo más cercano al NPC.
        */
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Player");

        GameObject nearest = null;
        float distance = Mathf.Infinity;

        foreach (GameObject obj in targets)
        {
            float d = Vector3.Distance(transform.position, obj.transform.position);

            if (d < distance)
            {
                distance = d;
                nearest = obj;
            }
        }

        target = nearest;
    }

    void AttackTarget()
    {
        /* 
        Maneja el ataque del NPC a su objetivo. Si el objetivo está en
        rango de ataque, realiza el ataque y finaliza el turno.
        */
        if (target != null)
        {
            TacticsMove targetMove = target.GetComponent<TacticsMove>();
            if (targetMove != null)
            {
                float distanceToTarget = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                                                          new Vector3(targetMove.transform.position.x, 0, targetMove.transform.position.z));
                float heightDifference = Mathf.Abs(targetMove.transform.position.y - transform.position.y);

                if (distanceToTarget <= characterStats.attackRange + 0.05f && 
                    (characterStats.heightAttack || heightDifference <= characterStats.jumpHeight))
                {
                    if (characterStats.attackType == AttackType.Normal)
                    {
                        CombatManager.Instance.Attack(this, targetMove);
                    }
                    else if (characterStats.attackType == AttackType.Pierce)
                    {
                        CombatManager.Instance.AttackWithPierce(this, targetMove);
                    }
                    
                    animator.SetTrigger("Attack");
                    AdjustRotation(this);
                    EndTurn();
                }
                else
                {
                    EndTurn();
                }
            }
            else
            {
                EndTurn();
            }
        }
        else
        {
            EndTurn();
        }
    }

    private void AdjustRotation(TacticsMove unit)
    {
        /* 
        Ajusta la rotación del NPC para que mire en una dirección recta
        (norte, sur, este, oeste).
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

    public override void EndTurn()
    {
        /* 
        Finaliza el turno del NPC y notifica al TurnManager.
        */
        base.EndTurn();
        TurnManager.EndTurn();
    }

    public override void UpdateHealthUI()
    {
        /* 
        Actualiza la UI de salud del NPC.
        */
        int characterIndex = GetCharacterIndex();
        if (characterIndex != -1)
        {
            UIManager.Instance.UpdateCharacterHealth(characterIndex, characterStats.health);
        }
    }

    private int GetCharacterIndex()
    {
        /* 
        Devuelve el índice del personaje basado en su nombre.
        */
        if (characterStats.name == "Nekomaru") return 0;
        if (characterStats.name == "Aya") return 1;
        if (characterStats.name == "Umi") return 2;
        if (characterStats.name == "Gaku") return 3;
        if (characterStats.name == "Yuniti") return 4;
        if (characterStats.name == "Hanami") return 5;
        if (characterStats.name == "Flynn") return 6;
        if (characterStats.name == "Kuroka") return 7;

        return -1;
    }
}
