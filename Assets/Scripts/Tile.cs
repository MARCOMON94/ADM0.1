using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    // Indica si el tile es transitable
    public bool walkable = true; 
    // Indica si el tile es el actual
    public bool current = false; 
    // Indica si el tile es el objetivo
    public bool target = false; 
    // Indica si el tile es seleccionable
    public bool selectable = false; 

    // Lista para almacenar los tiles adyacentes
    public List<Tile> adjacencyList = new List<Tile>();

    // Indica si el tile ha sido visitado
    public bool visited = false; 
    // Tile padre en la ruta
    public Tile parent = null; 
    // Distancia desde el inicio
    public int distance = 0; 

    // Variables adicionales para A*
    // Costo total (g + h)
    public float f = 0; 
    // Costo desde el inicio hasta el nodo actual
    public float g = 0; 
    // Heurística (estimación del costo desde el nodo actual hasta el objetivo)
    public float h = 0; 
    // Ignora si el tile está ocupado
    public bool ignoreOccupied = false;

    // Método Update que se llama una vez por frame
    void Update()
    {
        /* 
        Cambia el color del tile basado en su estado.
        Si es el tile actual, lo pinta de magenta.
        Si es el tile objetivo, lo pinta de verde.
        Si es seleccionable, lo pinta de rojo.
        De lo contrario, lo pinta de blanco.
        */
        if (current)
        {
            GetComponent<Renderer>().material.color = Color.magenta;
        }
        else if (target)
        {
            GetComponent<Renderer>().material.color = Color.green;
        }
        else if (selectable)
        {
            GetComponent<Renderer>().material.color = Color.red;
        }
        else
        {
            GetComponent<Renderer>().material.color = Color.white;
        }
    }

    // Resetea el estado del tile
    public void Reset()
    {
        /* 
        Limpia la lista de adyacencia y restablece todas las variables
        al estado inicial.
        */
        adjacencyList.Clear();
        current = false;
        target = false;
        selectable = false;
        visited = false;
        parent = null;
        distance = 0;
        f = g = h = 0;
    }

    // Encuentra los vecinos del tile que son transitables
    public void FindNeighbors(float jumpHeight, Tile target, bool ignoreOccupied)
    {
        /* 
        Resetea el estado actual del tile y luego revisa los tiles en las 
        direcciones: adelante, atrás, derecha e izquierda para encontrar 
        tiles transitables y añadirlos a la lista de adyacencia.
        */
        Reset();

        CheckTile(Vector3.forward, jumpHeight, target, ignoreOccupied);
        CheckTile(-Vector3.forward, jumpHeight, target, ignoreOccupied);
        CheckTile(Vector3.right, jumpHeight, target, ignoreOccupied);
        CheckTile(-Vector3.right, jumpHeight, target, ignoreOccupied);
    }

    // Revisa si hay un tile transitable en la dirección dada
    public void CheckTile(Vector3 direction, float jumpHeight, Tile target, bool ignoreOccupied)
    {
        /* 
        Crea un área de colisión para detectar otros tiles en la dirección
        especificada. Si el tile detectado es transitable y no está ocupado 
        (o se debe ignorar si está ocupado), se añade a la lista de adyacencia.
        */
        Vector3 halfExtents = new Vector3(0.25f, (1 + jumpHeight) / 2.0f, 0.25f);
        Collider[] colliders = Physics.OverlapBox(transform.position + direction, halfExtents);

        foreach (Collider item in colliders)
        {
            Tile tile = item.GetComponent<Tile>();
            if (tile != null && tile.walkable)
            {
                RaycastHit hit;
                // Modificar la condición para ignorar la ocupación si ignoreOccupied es true
                if (ignoreOccupied || !Physics.Raycast(tile.transform.position, Vector3.up, out hit, 1) || (tile == target))
                {
                    adjacencyList.Add(tile);
                }
            }
        }
    }
}
