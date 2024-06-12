using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerMove : TacticsMove
{
    private enum PlayerAction { None, Move, Attack }
    private PlayerAction currentAction = PlayerAction.None;
    private bool hasMoved = false;
    private Animator animator;

    void Start()
    {
        /* 
        Inicializa el movimiento del jugador, configurando su estado inicial 
        y actualizando su UI de salud.
        */
        Init(true);
        UpdateHealthUI();
        animator = GetComponentInChildren<Animator>();
        animator.SetBool("isMoving", false);
    }

    void Update()
    {
        /* 
        En cada frame, si es el turno del jugador, maneja la lógica de
        movimiento o ataque dependiendo de la acción seleccionada.
        */
        if (!turn)
        {
            return;
        }

        if (!moving)
        {
            if (currentAction == PlayerAction.None)
            {
                // Espera a que se seleccione una acción mediante botones
            }
            else if (currentAction == PlayerAction.Move)
            {
                FindSelectableTiles();
                CheckMouseMovement();
            }
            else if (currentAction == PlayerAction.Attack)
            {
                FindAttackableTiles();
                ShowAttackableEnemies();
                CheckMouseAttack();
            }
        }
        else
        {
            Move();
        }
    }

    public void SetActionMove()
    {
        /* 
        Establece la acción actual del jugador como movimiento.
        */
        currentAction = PlayerAction.Move;
    }

    public void SetActionAttack()
    {
        /* 
        Establece la acción actual del jugador como ataque y encuentra
        los tiles seleccionables.
        */
        currentAction = PlayerAction.Attack;
        FindSelectableTiles(true);
    }

    void CheckMouseMovement()
    {
        /* 
        Maneja el movimiento del jugador basado en la entrada del ratón.
        */
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.tag == "Tile")
                {
                    Tile t = hit.collider.GetComponent<Tile>();

                    if (t.selectable)
                    {
                        MoveToTile(t);
                        currentAction = PlayerAction.None;
                        hasMoved = true;
                        animator.SetBool("IsMoving", true);

                    }
                }
            }
        }
    }

    void ShowAttackableEnemies()
    {
        /* 
        Muestra a los enemigos que están dentro del rango de ataque.
        */
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("NPC");
        foreach (GameObject enemy in enemies)
        {
            TacticsMove enemyMove = enemy.GetComponent<TacticsMove>();
            Tile enemyTile = GetTargetTile(enemy);

            float heightDifference = Mathf.Abs(enemy.transform.position.y - transform.position.y);

            if (enemyTile != null && selectableTiles.Contains(enemyTile) && 
                (characterStats.heightAttack || heightDifference <= 0.1f))
            {
                Renderer renderer = enemy.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = Color.red;
                }
            }
        }
    }

    public void CheckMouseAttack()
    {
        /* 
        Maneja el ataque del jugador basado en la entrada del ratón.
        */
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.tag == "NPC")
                {
                    TacticsMove enemy = hit.collider.GetComponent<TacticsMove>();

                    if (enemy != null)
                    {
                        float distanceToEnemy = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                                                                new Vector3(enemy.transform.position.x, 0, enemy.transform.position.z));
                        float heightDifference = Mathf.Abs(enemy.transform.position.y - transform.position.y);

                        if (distanceToEnemy <= characterStats.attackRange + 0.05f && 
                            (characterStats.heightAttack || heightDifference <= 0.1f))
                        {
                            if (characterStats.attackType == AttackType.Normal)
                            {
                                CombatManager.Instance.Attack(this, enemy);
                            }
                            else if (characterStats.attackType == AttackType.Pierce)
                            {
                                CombatManager.Instance.AttackWithPierce(this, enemy);
                            }
                            animator.SetTrigger("Attack"); // Trigger de animación de ataque

                            EndTurn();
                        }
                    }
                }
            }
        }
    }

    public void EndTurn()
    {
        /* 
        Finaliza el turno del jugador y restablece los estados y colores
        de los enemigos y tiles.
        */
        currentAction = PlayerAction.None;
        hasMoved = false;
        

        

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("NPC");
        foreach (GameObject enemy in enemies)
        {
            Renderer renderer = enemy.GetComponent<Renderer>();
            if (renderer != null)       
            

            {
                renderer.material.color = Color.white;
            
            }        

        }

        foreach (Tile tile in selectableTiles)
        {
            tile.selectable = false;
            tile.GetComponent<Renderer>().material.color = Color.white;
            

        }
        selectableTiles.Clear();
        animator.SetBool("IsMoving", false);
        TurnManager.EndTurn();

        
    }

    public void BeginTurn()
    {
        /* 
        Inicia el turno del jugador, restableciendo los estados necesarios.
        */
        turn = true;
        currentAction = PlayerAction.None;
        hasMoved = false;
    }

    public override void UpdateHealthUI()
    {
        /* 
        Actualiza la UI de salud del jugador.
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
