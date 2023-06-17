using System.Collections.Generic;
using UnityEngine;

public enum EnemyState
{
    Idle,
    Chase,
    Patrol
}

public class EnemySFM : MonoBehaviour
{
    public int viewDistance;
    public int viewAngle;
    public float idleTimer;
    public List<Node> startNodes;
    public int movementSpeed;

    private EnemyState currentState;
    private List<Node> currentPath;
    private int currentPathIndex;
    private PlayerController player;
    private float idleCount;
    private int currentPatrolNodeIndex;

    private void Start()
    {
        currentState = EnemyState.Idle;
        player = FindObjectOfType<PlayerController>();
    }
    
    private void Update()
    {
        // Actualizar el comportamiento según el estado actual
        switch (currentState)
        {
            case EnemyState.Idle:
                IdleState();
                break;
            case EnemyState.Chase:
                ChaseState();
                break;
            case EnemyState.Patrol:
                PatrolState();
                break;
        }
    }

    private void IdleState()
    {
        if (DetectPlayer())
        {
            currentState = EnemyState.Chase;
        }

        idleCount += Time.deltaTime;
        if (idleTimer >= idleCount)
        {
            currentState = EnemyState.Patrol;
        }
    }

    private void ChaseState()
    {
        if (!DetectPlayer())
        {
            currentState = EnemyState.Idle;
            idleCount = 0;
            return;
        }

        Vector3 direction = player.transform.position - transform.position;
        direction.Normalize();

        RotateTowardsForward(direction);
        transform.position += transform.right * movementSpeed * Time.deltaTime;
    }

    private void PatrolState()
    {
        if (DetectPlayer())
        {
            currentState = EnemyState.Chase;
            return;
        }

        if (currentPath == null || currentPathIndex >= currentPath.Count)
        {
            // Buscar un nuevo camino hacia el nodo de patrulla actual
            var startNode = AStar.FindClosestNodeToPos(transform.position);
            var targetNode = startNodes[currentPatrolNodeIndex];
            currentPath = AStar.FindPath(startNode, targetNode);
            currentPathIndex = 0;
        }

        if (currentPath != null && currentPathIndex < currentPath.Count)
        {
            // Mueve al enemigo hacia el siguiente nodo del camino
            var nextPosition = currentPath[currentPathIndex].position;
            var direction = nextPosition - transform.position;
            direction.Normalize();

            // Rotar hacia la dirección hacia adelante sin modificar el movimiento en el escenario
            RotateTowardsForward(direction);

            // Mueve al enemigo hacia adelante con una velocidad específica
            transform.position += transform.right * movementSpeed * Time.deltaTime;

            // Comprueba si el enemigo ha alcanzado el nodo objetivo del camino
            if (Vector3.Distance(transform.position, nextPosition) < 0.1f)
            {
                currentPathIndex++;
            }
        }

        // Comprueba si el enemigo ha alcanzado el último nodo de patrulla y actualiza al siguiente nodo
        if (currentPathIndex >= currentPath.Count)
        {
            currentPatrolNodeIndex++;
            if (currentPatrolNodeIndex >= startNodes.Count)
            {
                currentPatrolNodeIndex = 0;
            }
        }
    }
    
    private bool DetectPlayer()
    {
        // Define la posición del jugador
        Vector2 playerPosition = player.transform.position;

        // Define la dirección hacia el jugador
        var direction = playerPosition - (Vector2)transform.position;

        // Calcula la distancia hasta el jugador
        var distance = direction.magnitude;

        // Dibuja una línea de debug para visualizar la distancia de detección
        Debug.DrawLine(transform.position, transform.position + (Vector3)direction.normalized * viewDistance, Color.yellow);

        // Obtiene la dirección hacia adelante del enemigo
        Vector2 forwardDirection = transform.right;

        // Calcula el ángulo entre la dirección hacia adelante y la dirección hacia el jugador
        var angle = Vector2.Angle(forwardDirection, direction);

        // Dibuja un arco de debug para visualizar el ángulo de visión del enemigo
        Debug.DrawRay(transform.position, Quaternion.Euler(0, 0, -viewAngle * 0.5f) * forwardDirection * viewDistance, Color.green);
        Debug.DrawRay(transform.position, Quaternion.Euler(0, 0, viewAngle * 0.5f) * forwardDirection * viewDistance, Color.green);
        Debug.DrawRay(transform.position, forwardDirection * viewDistance, Color.green);

        // Comprueba si el jugador está dentro del rango de detección y dentro del ángulo de visión del enemigo
        if (distance <= viewDistance && angle <= viewDistance * 0.5f)
        {
            // Realiza un raycast para detectar objetos en el camino
            var hit = Physics2D.Raycast(transform.position, direction, distance, LayerMask.GetMask("Obstacles"));

            // Si el raycast golpea un collider y es el jugador, lo ha detectado
            if (hit.collider != null && hit.collider.gameObject == player.gameObject)
            {
                return true;
            }
        }

        return false;
    }
        
    private void RotateTowardsForward(Vector3 direction)
    {
        var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
