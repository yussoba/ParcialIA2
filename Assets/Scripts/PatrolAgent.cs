using System.Collections.Generic;
using UnityEngine;

public class PatrolAgent : MonoBehaviour
{
    public List<Node> waypoints;
    public float moveSpeed = 3f;
    public float fieldOfViewAngle = 90f;
    public float detectionRange = 10f;

    private int currentWaypointIndex;
    private Transform player;
    private bool playerDetected;
    private Vector3 alertedPosition;
    private Vector3 currentWaypoint;
    private bool isPatrolling = true;
    private List<Node> defaultWaypoints;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        currentWaypointIndex = 0;
        defaultWaypoints = waypoints;
    }

    private void Update()
    {
        if (playerDetected)
        {
            // El jugador ha sido detectado, buscar el mejor camino hacia su posición
            FindPathToPosition(player.position);
        }
        else
        {
            if (isPatrolling)
            {
                // Si se ha llegado al waypoint actual, avanzar al siguiente
                if (Vector3.Distance(transform.position, currentWaypoint) < 0.1f)
                {
                    currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
                    currentWaypoint = waypoints[currentWaypointIndex].position;
                }

                // Verificar si el siguiente waypoint está en línea de visión
                var directionToNextWaypoint = currentWaypoint - transform.position;
                var angleToNextWaypoint = Vector3.Angle(directionToNextWaypoint, transform.forward);

                if (angleToNextWaypoint <= fieldOfViewAngle * 0.5f)
                {
                    // El siguiente waypoint está en línea de visión, continuar con el patrullaje
                    MoveToNextWaypoint();
                }
                else
                {
                    // Calcular el camino hacia el siguiente waypoint utilizando A*
                    FindPathToNextWaypoint();
                }
            }
            else
            {
                // En este punto, se ha calculado un camino y se está siguiendo
                if (Vector3.Distance(transform.position, currentWaypoint) < 0.1f)
                {
                    // Si el agente ha alcanzado el waypoint actual, pasa al siguiente
                    currentWaypointIndex++;
                    if (currentWaypointIndex >= waypoints.Count)
                    {
                        currentWaypointIndex = 0; // Vuelve al primer waypoint si ha alcanzado el último
                    }
                    currentWaypoint = waypoints[currentWaypointIndex].position;
                }
                // Mueve al agente hacia el waypoint actual
                transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, moveSpeed * Time.deltaTime);
            }
            // Si se ha recibido una posición alertada, comprobar si está en línea de visión
            if (alertedPosition != Vector3.zero)
            {
                var directionToAlertedPosition = alertedPosition - transform.position;
                var angleToAlertedPosition = Vector3.Angle(directionToAlertedPosition, transform.forward);

                if (angleToAlertedPosition <= fieldOfViewAngle * 0.5f)
                {
                    // La posición alertada está en línea de visión, no es necesario calcular el camino
                    alertedPosition = Vector3.zero;
                    return;
                }
                else
                {
                    // Calcular el camino hacia la posición alertada
                    FindPathToPosition(alertedPosition);
                }
            }
            // Continuar con el patrullaje
            Patrol();
        }
    }
    private void MoveToNextWaypoint()
    {
        // Moverse hacia el siguiente waypoint
        var targetPosition = currentWaypoint;
        var movement = targetPosition - transform.position;
        movement.Normalize();
        transform.Translate(movement * moveSpeed * Time.deltaTime);
    }
    
    private void FindPathToNextWaypoint()
    {
        // Obtener la posición actual del agente y la posición del siguiente waypoint
        var startPosition = transform.position;
        var goalPosition = currentWaypoint;

        // Calcular el camino utilizando A*
        waypoints = AStar.FindPath(AStar.FindClosestNodeToPos(startPosition, waypoints), AStar.FindClosestNodeToPos(goalPosition, waypoints));
        currentWaypointIndex = 0;
        
        // Verificar si se encontró un camino válido
        if (waypoints != null && waypoints.Count > 0)
        {
            currentWaypoint = waypoints[currentWaypointIndex].position;
        }
        else
        {
            waypoints = defaultWaypoints;
            currentWaypoint = waypoints[0].position;
        }
    }
    
    private void Patrol()
    {
        // Moverse hacia el siguiente waypoint
        var targetPosition = waypoints[currentWaypointIndex].position;
        var movement = targetPosition - transform.position;
        movement.Normalize();
        transform.Translate(movement * moveSpeed * Time.deltaTime);

        // Si alcanza el waypoint, avanzar al siguiente
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
        }

        // Verificar si el jugador está en el rango de detección y dentro del campo de visión
        if (Vector3.Distance(transform.position, player.position) <= detectionRange)
        {
            var directionToPlayer = player.position - transform.position;
            var angleToPlayer = Vector3.Angle(directionToPlayer, transform.forward);

            if (angleToPlayer <= fieldOfViewAngle * 0.5f)
            {
                // El jugador está dentro del campo de visión, detectarlo
                playerDetected = true;

                // Alertar a los demás agentes
                AlertAgents();
            }
        }
    }

    private void FindPathToPosition(Vector3 position)
    {
        var startPosition = transform.position;

        // Calcular el camino utilizando A*
        waypoints = AStar.FindPath(AStar.FindClosestNodeToPos(startPosition), AStar.FindClosestNodeToPos(position));

        // Verificar si se encontró un camino válido
        if (waypoints != null && waypoints.Count > 0)
        {
            currentWaypointIndex = 0;
            currentWaypoint = waypoints[currentWaypointIndex].position;
        }
    }

    private void AlertAgents()
    {
        var agents = FindObjectsOfType<PatrolAgent>();

        foreach (var agent in agents)
        {
            if (!agent.Equals(this))
            {
                agent.OnPlayerDetected(player.position);
            }
        }
    }

    public void OnPlayerDetected(Vector3 playerPosition)
    {
        // Un agente vecino ha detectado al jugador, comprobar si está en línea de visión
        var directionToPlayer = playerPosition - transform.position;
        var angleToPlayer = Vector3.Angle(directionToPlayer, transform.forward);

        if (angleToPlayer <= fieldOfViewAngle * 0.5f)
        {
            // El jugador está dentro del campo de visión, actualizar la posición del jugador detectado
            playerDetected = true;
            player.position = playerPosition;
        }
        else
        {
            // El jugador no está en línea de visión, calcular el camino hacia la posición del jugador
            FindPathToPosition(playerPosition);
        }
        // Guardar la posición alertada para el cálculo del camino
        alertedPosition = playerPosition;
        isPatrolling = false;
    }
}
