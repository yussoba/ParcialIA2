using System.Collections.Generic;
using UnityEngine;

public enum EnemyState
{
    Idle,
    Chase,
    Patrol,
    Search
}

public class EnemySFM : MonoBehaviour
{
    public int viewDistance;
    public int viewAngle;
    public float idleTimer;
    public List<Node> startNodes;
    public int movementSpeed;
    public bool playerFounded;
    public LayerMask wallLayer;

    private EnemyState currentState;
    private List<Node> currentPath;
    private int currentPathIndex;
    private PlayerController player;
    private float idleCount;
    private int currentPatrolNodeIndex;
    private Node targetNode;
    private EnemyManager enemyManager;
    
    private void Start()
    {
        currentState = EnemyState.Idle;
        player = FindObjectOfType<PlayerController>();
        enemyManager = FindObjectOfType<EnemyManager>();
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
            case EnemyState.Search:
                SearchState();
                break;
        }
    }

    private void IdleState()
    {
        if (DetectPlayer())
        {
            playerFounded = true;
            enemyManager.NotifyOtherEnemies(this);
            currentState = EnemyState.Chase;
        }
        else if (playerFounded)
        {
            playerFounded = !playerFounded;
        }
        else
        {
            idleCount += Time.deltaTime;
            if (idleTimer >= idleCount)
            {
                currentState = EnemyState.Patrol;
            }
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

        var direction = player.transform.position - transform.position;
        direction.Normalize();

        RotateTowardsForward(direction);
        transform.position += transform.right * movementSpeed * Time.deltaTime;
    }

    private void PatrolState()
    {
        if (DetectPlayer())
        {
            playerFounded = true;
            enemyManager.NotifyOtherEnemies(this);
            currentState = EnemyState.Chase;
        }
        else if (playerFounded)
        {
            currentState = EnemyState.Search;
        }
        else
        {
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
    }
    
    private void SearchState()
    {
        // Encontrar el nodo más cercano a la última posición conocida del jugador
        var closestNode = AStar.FindClosestNodeToPos(player.transform.position);

        // Si aún no se ha encontrado el nodo objetivo o el jugador ha cambiado de posición
        if (targetNode == null || closestNode != targetNode)
        {
            targetNode = closestNode;

            // Encontrar el camino hacia el nodo objetivo
            currentPath = AStar.FindPath(AStar.FindClosestNodeToPos(transform.position), targetNode);
        }

        // Si se ha encontrado un camino válido
        if (currentPath != null && currentPath.Count > 0)
        {
            // Moverse hacia el siguiente nodo en el camino
            var nextNode = currentPath[0];
            var nextPosition = nextNode.position;

            // Mover el enemigo hacia el siguiente nodo en el camino
            transform.position = Vector3.MoveTowards(transform.position, nextPosition, Time.deltaTime * movementSpeed);

            // Si el enemigo ha llegado al nodo objetivo
            if (transform.position == nextPosition)
            {
                // Eliminar el nodo actual del camino
                currentPath.RemoveAt(0);

                // Si se ha alcanzado el final del camino
                if (currentPath.Count == 0)
                {
                    // Invertir el camino
                    currentPath.Reverse();

                    // Cambiar al estado de patrullaje
                    currentState = EnemyState.Patrol;
                }
            }
        }
        else
        {
            currentState = EnemyState.Idle;
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

        if (Vector2.Distance(transform.position, playerPosition) > viewDistance)
        {
            return false;

        }
        if (Vector2.Angle(transform.forward, playerPosition - (Vector2)transform.position) > (viewAngle / 2))
        {
            return false;
        }
        Debug.Log(InLos(transform.position, playerPosition));
        return InLos(transform.position, playerPosition);
    }
        
    public bool InLos(Vector2 myPos, Vector2 playerPos)
    {
        var dir = playerPos - myPos;
        Debug.Log(!Physics2D.Raycast(myPos, dir, dir.magnitude, wallLayer));
        return !Physics2D.Raycast(myPos, dir, dir.magnitude, wallLayer);
    }

    private void RotateTowardsForward(Vector3 direction)
    {
        var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

}
