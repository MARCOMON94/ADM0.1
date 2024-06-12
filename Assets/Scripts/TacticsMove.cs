using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TacticsMove : MonoBehaviour
{
    // Indica si es el turno del personaje
    public bool turn = false;

    // Lista de tiles seleccionables
    protected List<Tile> selectableTiles = new List<Tile>();

    // Array de todos los tiles en el mapa
    GameObject[] tiles;

    // Pila que representa el camino a seguir
    Stack<Tile> path = new Stack<Tile>();

    // Tile actual en el que se encuentra el personaje
    Tile currentTile;

    // Indica si el personaje está en movimiento
    public bool moving = false;

    // Usar ScriptableObject para estadísticas del personaje
    public CharacterStatsSO characterStats;

    // Velocidad actual del personaje
    Vector3 velocity = new Vector3();

    // Dirección del movimiento
    Vector3 heading = new Vector3();

    // Mitad de la altura del colisionador del personaje
    float halfHeight = 0;

    // Indicadores de estado de movimiento
    bool fallingDown = false;
    bool jumpingUp = false;
    bool movingEdge = false;

    // Objetivo del salto
    Vector3 jumpTarget;

    // Tile objetivo final
    public Tile actualTargetTile;

    // Referencia al Animator
    protected Animator animator;

    // Método de inicialización
    protected void Init(bool isPlayer)
    {
        /* 
        Encuentra todos los tiles en el mapa, obtiene la mitad de la altura
        del colisionador y añade este personaje al TurnManager.
        */
        tiles = GameObject.FindGameObjectsWithTag("Tile");
        halfHeight = GetComponent<Collider>().bounds.extents.y;
        TurnManager.AddUnit(this);
        animator = GetComponentInChildren<Animator>(); // Obtén el Animator del modelo
    }

    // Método para iniciar el turno del personaje
    public virtual void BeginTurn()
    {
        /* 
        Marca el turno como activo.
        */
        turn = true;
        animator?.SetBool("IsMoving", false); // Asegúrate de que IsMoving esté en false al comienzo del turno
    }

    // Método para finalizar el turno del personaje
    public virtual void EndTurn()
    {
        /* 
        Marca el turno como inactivo, elimina los tiles seleccionables y 
        notifica al TurnManager.
        */
        turn = false;
        RemoveSelectableTiles();
        animator?.SetBool("IsMoving", false); // Asegúrate de que IsMoving esté en false al final del turno

        if (!(this is NPCMove))
        {
            TurnManager.EndTurn();
        }
    }

    // Obtiene el tile actual en el que se encuentra el personaje
    public void GetCurrentTile()
    {
        /* 
        Obtiene el tile debajo del personaje y lo marca como actual.
        */
        currentTile = GetTargetTile(gameObject);
        currentTile.current = true;
    }

    // Obtiene el tile debajo de un GameObject específico
    public Tile GetTargetTile(GameObject target)
    {
        /* 
        Lanza un rayo hacia abajo desde la posición del target y obtiene
        el componente Tile del objeto golpeado.
        */
        RaycastHit hit;
        Tile tile = null;

        if (Physics.Raycast(target.transform.position, -Vector3.up, out hit, 1))
        {
            tile = hit.collider.GetComponent<Tile>();
        }
        return tile;
    }

    // Calcula la lista de tiles adyacentes teniendo en cuenta la altura de salto
    public void ComputeAdjacencyLists(float jumpHeight, Tile target, bool ignoreOccupied)
    {
        /* 
        Encuentra los vecinos de cada tile en el mapa y los almacena en 
        sus listas de adyacencia.
        */
        foreach (GameObject tile in tiles)
        {
            Tile t = tile.GetComponent<Tile>();
            t.FindNeighbors(jumpHeight, target, ignoreOccupied);
        }
    }

    // Encuentra los tiles seleccionables dentro del rango de movimiento
    public void FindSelectableTiles(bool ignoreOccupied = false)
    {
        /* 
        Encuentra los tiles dentro del rango de movimiento del personaje
        y los marca como seleccionables.
        */
        ComputeAdjacencyLists(characterStats.jumpHeight, null, ignoreOccupied);
        GetCurrentTile();

        Queue<Tile> process = new Queue<Tile>();
        process.Enqueue(currentTile);
        currentTile.visited = true;

        while (process.Count > 0)
        {
            Tile t = process.Dequeue();
            selectableTiles.Add(t);
            t.selectable = true;

            if (t.distance < characterStats.move)
            {
                foreach (Tile tile in t.adjacencyList)
                {
                    if (!tile.visited)
                    {
                        tile.parent = t;
                        tile.visited = true;

                        float heightDifference = Mathf.Abs(tile.transform.position.y - t.transform.position.y);
                        int additionalCost = (int)(heightDifference * 2);
                        int newDistance = t.distance + 1 + additionalCost;

                        if (newDistance <= characterStats.move)
                        {
                            tile.distance = newDistance;
                            process.Enqueue(tile);
                        }
                    }
                }
            }
        }
    }

    // Mueve al personaje hacia un tile específico
    public void MoveToTile(Tile tile)
    {
        /* 
        Limpia el camino actual, marca el tile como objetivo y marca que
        el personaje está en movimiento.
        */
        path.Clear();
        tile.target = true;
        moving = true;

        Tile next = tile;
        while (next != null)
        {
            path.Push(next);
            next = next.parent;
        }
    }

    // Método para mover al personaje
    public void Move()
    {
        /* 
        Mueve al personaje a lo largo del camino calculado hacia el tile
        objetivo.
        */
        if (path.Count > 0)
        {
            Tile t = path.Peek();
            Vector3 target = t.transform.position;
            target.y += halfHeight + t.GetComponent<Collider>().bounds.extents.y;

            if (Vector3.Distance(transform.position, target) >= 0.05f)
            {
                bool jump = transform.position.y != target.y;

                if (jump)
                {
                    Jump(target);
                }
                else
                {
                    CalculateHeading(target);
                    SetHorizontalVelocity();
                }

                transform.forward = heading;
                transform.position += velocity * Time.deltaTime;
            }
            else
            {
                transform.position = target;
                path.Pop();
                ResetMovementStates();
            }
        }
        else
        {
            RemoveSelectableTiles();
            moving = false;
            ResetMovementStates();
            animator?.SetBool("IsMoving", false); // Desactiva el bool de movimiento cuando el movimiento termina
            TurnManager.EndTurn();
        }
    }

    // Resetea los estados de movimiento del personaje
    void ResetMovementStates()
    {
        /* 
        Resetea los estados de movimiento del personaje.
        */
        fallingDown = false;
        jumpingUp = false;
        movingEdge = false;
    }

    // Elimina los tiles seleccionables
    void RemoveSelectableTiles()
    {
        /* 
        Resetea el estado de todos los tiles seleccionables y los elimina
        de la lista.
        */
        if (currentTile != null)
        {
            currentTile.current = false;
            currentTile = null;
        }

        foreach (Tile tile in selectableTiles)
        {
            tile.Reset();
        }

        selectableTiles.Clear();
    }

    // Calcula la dirección hacia el objetivo
    void CalculateHeading(Vector3 target)
    {
        /* 
        Calcula la dirección hacia el objetivo.
        */
        heading = target - transform.position;
        heading.Normalize();
    }

    // Establece la velocidad horizontal
    void SetHorizontalVelocity()
    {
        /* 
        Establece la velocidad horizontal del personaje basada en la 
        dirección y la velocidad de movimiento.
        */
        velocity = heading * characterStats.moveSpeed;
    }

    // Método para manejar el salto
    void Jump(Vector3 target)
    {
        /* 
        Maneja el salto del personaje.
        */
        if (fallingDown)
        {
            FallDownward(target);
        }
        else if (jumpingUp)
        {
            JumpUpward(target);
        }
        else if (movingEdge)
        {
            MoveToEdge();
        }
        else
        {
            PrepareJump(target);
        }
    }

    // Prepara el salto
    void PrepareJump(Vector3 target)
    {
        /* 
        Prepara el personaje para saltar.
        */
        float targetY = target.y;
        Vector3 localTarget = target;
        localTarget.y = transform.position.y;

        CalculateHeading(localTarget);

        if (transform.position.y > targetY)
        {
            fallingDown = false;
            jumpingUp = false;
            movingEdge = true;
            jumpTarget = transform.position + (localTarget - transform.position) / 2.0f;
        }
        else
        {
            fallingDown = false;
            jumpingUp = true;
            movingEdge = false;
            velocity = heading * characterStats.moveSpeed / 3.0f;
            float difference = targetY - transform.position.y;
            velocity.y = characterStats.jumpVelocity * (0.5f + difference / 2.0f);
        }
    }

    // Maneja la caída del personaje
    void FallDownward(Vector3 target)
    {
        /* 
        Maneja la caída del personaje.
        */
        velocity += Physics.gravity * Time.deltaTime;

        if (transform.position.y <= target.y)
        {
            fallingDown = false;
            jumpingUp = false;
            movingEdge = false;

            Vector3 p = transform.position;
            p.y = target.y;
            transform.position = p;
            velocity = new Vector3();
        }
    }

    // Maneja el ascenso del personaje
    void JumpUpward(Vector3 target)
    {
        /* 
        Maneja el ascenso del personaje.
        */
        velocity += Physics.gravity * Time.deltaTime;

        if (transform.position.y > target.y)
        {
            jumpingUp = false;
            fallingDown = true;
        }
    }

    // Mueve al personaje hacia el borde
    void MoveToEdge()
    {
        /* 
        Mueve al personaje hacia el borde.
        */
        if (Vector3.Distance(transform.position, jumpTarget) >= 0.05f)
        {
            SetHorizontalVelocity();
        }
        else
        {
            movingEdge = false;
            fallingDown = true;
            velocity /= 5.0f;
            velocity.y = 1.5f;
        }
    }

    // Encuentra el tile con el menor costo total (f)
    protected Tile FindLowestF(List<Tile> list)
    {
        /* 
        Encuentra el tile con el menor costo total (f) en una lista de
        tiles.
        */
        Tile lowest = list[0];

        foreach (Tile t in list)
        {
            if (t.f < lowest.f)
            {
                lowest = t;
            }
        }

        list.Remove(lowest);
        return lowest;
    }

    // Encuentra el tile final en el camino
    protected Tile FindEndTile(Tile t)
    {
        /* 
        Encuentra el tile final en el camino.
        */
        Stack<Tile> tempPath = new Stack<Tile>();
        Tile next = t.parent;

        while (next != null)
        {
            tempPath.Push(next);
            next = next.parent;
        }

        if (tempPath.Count <= characterStats.move)
        {
            return t.parent;
        }

        Tile endTile = null;
        for (int i = 0; i <= characterStats.move; i++)
        {
            endTile = tempPath.Pop();
        }

        return endTile;
    }

    // Encuentra el camino hacia el tile objetivo usando A*
    public void FindPath(Tile target)
    {
        /* 
        Encuentra el camino hacia el tile objetivo utilizando el algoritmo
        A*.
        */
        ComputeAdjacencyLists(characterStats.jumpHeight, target, false);
        GetCurrentTile();

        List<Tile> openList = new List<Tile>();
        List<Tile> closedList = new List<Tile>();

        openList.Add(currentTile);
        currentTile.h = Vector3.Distance(currentTile.transform.position, target.transform.position);
        currentTile.f = currentTile.h;

        while (openList.Count > 0)
        {
            Tile t = FindLowestF(openList);
            closedList.Add(t);

            if (t == target)
            {
                actualTargetTile = FindEndTile(t);
                MoveToTile(actualTargetTile);
                return;
            }

            foreach (Tile tile in t.adjacencyList)
            {
                if (closedList.Contains(tile))
                {
                    continue;
                }

                if (!openList.Contains(tile))
                {
                    tile.parent = t;
                    tile.g = t.g + Vector3.Distance(tile.transform.position, t.transform.position);
                    float heightDifference = Mathf.Abs(tile.transform.position.y - t.transform.position.y);
                    int additionalCost = (int)(heightDifference * 2);
                    tile.g += additionalCost;
                    tile.h = Vector3.Distance(tile.transform.position, target.transform.position);
                    tile.f = tile.g + tile.h;
                    openList.Add(tile);
                }
                else
                {
                    float tempG = t.g + Vector3.Distance(tile.transform.position, t.transform.position);
                    float heightDifference = Mathf.Abs(tile.transform.position.y - t.transform.position.y);
                    int additionalCost = (int)(heightDifference * 2);
                    tempG += additionalCost;

                    if (tempG < tile.g)
                    {
                        tile.parent = t;
                        tile.g = tempG;
                        tile.f = tile.g + tile.h;
                    }
                }
            }
        }
    }

    // Encuentra los tiles en el rango de ataque
    public void FindAttackableTiles()
    {
        /* 
        Encuentra los tiles dentro del rango de ataque del personaje y los
        marca como seleccionables.
        */
        ComputeAdjacencyLists(characterStats.jumpHeight, null, true);
        GetCurrentTile();

        Queue<Tile> process = new Queue<Tile>();
        process.Enqueue(currentTile);
        currentTile.visited = true;

        while (process.Count > 0)
        {
            Tile t = process.Dequeue();
            selectableTiles.Add(t);
            t.selectable = true;

            if (t.distance < characterStats.attackRange)
            {
                foreach (Tile tile in t.adjacencyList)
                {
                    if (!tile.visited)
                    {
                        tile.parent = t;
                        tile.visited = true;
                        tile.distance = 1 + t.distance;
                        process.Enqueue(tile);
                    }
                }
            }
        }
    }

    public virtual void UpdateHealthUI()
    {
        // Método virtual para ser sobrescrito por clases derivadas.
    }
}
