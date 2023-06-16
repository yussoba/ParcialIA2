using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Obtener entrada de movimiento del usuario
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        // Calcular dirección de movimiento
        Vector3 movement = new Vector3(moveHorizontal, moveVertical, 0f);
        movement.Normalize(); // Normalizar la magnitud para evitar movimientos diagonales más rápidos

        // Aplicar movimiento al agente
        rb.velocity = movement * moveSpeed;
    }
}
