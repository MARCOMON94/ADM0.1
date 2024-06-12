using System;
using UnityEngine;

[Serializable]
public class CharacterStatsSnapshot
{
    // Nombre del personaje
    public string name;
    // Cantidad de tiles que el personaje puede moverse
    public int move;
    // Altura que el personaje puede saltar
    public float jumpHeight;
    // Velocidad de movimiento del personaje
    public float moveSpeed;
    // Velocidad de salto del personaje
    public float jumpVelocity;
    // Velocidad del personaje, usada para determinar el orden de turnos
    public int speed;
    // Daño básico que el personaje puede infligir
    public int basicDamage;
    // Rango de ataque del personaje
    public float attackRange;
    // Vida del personaje
    public int health;
    // Tipo de ataque del personaje
    public AttackType attackType;
    // Indica si el personaje puede realizar un ataque en altura
    public bool heightAttack;
    // Indica si el personaje puede volar
    public bool canFly;

    // Constructor que inicializa las estadísticas del personaje a partir de un ScriptableObject
    public CharacterStatsSnapshot(CharacterStatsSO stats)
    {
        /* 
        Inicializa las variables del snapshot con los valores del 
        ScriptableObject pasado como argumento.
        */
        name = stats.name;
        move = stats.move;
        jumpHeight = stats.jumpHeight;
        moveSpeed = stats.moveSpeed;
        jumpVelocity = stats.jumpVelocity;
        speed = stats.speed;
        basicDamage = stats.basicDamage;
        attackRange = stats.attackRange;
        health = stats.health;
        attackType = stats.attackType;
        heightAttack = stats.heightAttack;
        canFly = stats.canFly;
    }

    // Restaura las estadísticas del personaje desde el snapshot
    public void Restore(CharacterStatsSO stats)
    {
        /* 
        Restaura las variables del ScriptableObject con los valores del 
        snapshot actual.
        */
        stats.name = name;
        stats.move = move;
        stats.jumpHeight = jumpHeight;
        stats.moveSpeed = moveSpeed;
        stats.jumpVelocity = jumpVelocity;
        stats.speed = speed;
        stats.basicDamage = basicDamage;
        stats.attackRange = attackRange;
        stats.health = health;
        stats.attackType = attackType;
        stats.heightAttack = heightAttack;
        stats.canFly = canFly;
    }
}
