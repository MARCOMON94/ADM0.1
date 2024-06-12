using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackType
{
    Normal,
    Pierce
}

// Esta clase define un ScriptableObject que almacena las estadísticas de un personaje.
[CreateAssetMenu(fileName = "CharacterStats", menuName = "Character Stats", order = 51)]
public class CharacterStatsSO : ScriptableObject
{
    // Nombre del personaje
    public string name = "Character";

    // Cantidad de tiles que el personaje puede moverse
    public int move = 5;

    // Altura que el personaje puede saltar
    public float jumpHeight = 2;

    // Velocidad de movimiento del personaje
    public float moveSpeed = 2;

    // Velocidad de salto del personaje
    public float jumpVelocity = 4.5f;

    // Velocidad del personaje, usada para determinar el orden de turnos
    public int speed = 5;

    // Daño básico que el personaje puede infligir
    public int basicDamage = 10;

    // Rango de ataque del personaje
    public float attackRange = 1.5f;

    // Vida del personaje
    public int health = 100;

    // Tipo de ataque del personaje
    public AttackType attackType = AttackType.Normal;

    // Indica si el personaje puede realizar un ataque en altura
    public bool heightAttack;

    // Indica si el personaje puede volar
    public bool canFly;
}
