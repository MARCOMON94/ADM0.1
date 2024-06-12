using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticsCamera : MonoBehaviour
{
    // Un pivote para que la cámara no se desconfigure al cambiarle la posición, para quitar la que venía por defecto en el tutorial.
    public Transform cameraPivot;

    // Método para rotar la cámara hacia la izquierda
    public void RotateLeft()
    {
        /* 
        Rota la cámara 90 grados hacia la izquierda alrededor del eje Y
        mundial.
        */
        if (cameraPivot != null)
        {
            cameraPivot.Rotate(Vector3.up, 90, Space.World);
        }
    }

    // Método para rotar la cámara hacia la derecha
    public void RotateRight()
    {
        /* 
        Rota la cámara 90 grados hacia la derecha alrededor del eje Y
        mundial.
        */
        if (cameraPivot != null)
        {
            cameraPivot.Rotate(Vector3.up, -90, Space.World);
        }
    }
}
