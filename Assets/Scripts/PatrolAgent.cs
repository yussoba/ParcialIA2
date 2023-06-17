using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolAgent : MonoBehaviour
{
    public Color patrolColor;
    public Transform[] waypoints;
    public float moveSpeed = 3f;
    public float fieldOfViewAngle = 90f;
    public float detectionRange = 10f;

    private int currentWaypointIndex;
    private Transform player;
    private bool playerDetected;
    private Vector3 alertedPosition;
    private Vector3 currentWaypoint;
    private bool isPatrolling = true;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        currentWaypointIndex = 0;
    }

    private void Update()
    {
        if (playerDetected)
        {
            // El jugador ha sido detectado, buscar el mejor camino hacia su posición
            FindPathToPlayer();
        }
        else
        {
            if (isPatrolling)
            {
                // Si se ha llegado al waypoint actual, avanzar al siguiente
                if (Vector3.Distance(transform.position, currentWaypoint) < 0.1f)
                {
                    currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
                    currentWaypoint = waypoints[currentWaypointIndex].position;
                }

                // Verificar si el siguiente waypoint está en línea de visión
                Vector3 directionToNextWaypoint = currentWaypoint - transform.position;
                float angleToNextWaypoint = Vector3.Angle(directionToNextWaypoint, transform.forward);

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
                    if (currentWaypointIndex >= waypoints.Length)
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
                Vector3 directionToAlertedPosition = alertedPosition - transform.position;
                float angleToAlertedPosition = Vector3.Angle(directionToAlertedPosition, transform.forward);

                if (angleToAlertedPosition <= fieldOfViewAngle * 0.5f)
                {
                    // La posición alertada está en línea de visión, no es necesario calcular el camino
                    alertedPosition = Vector3.zero;
                    return;
                }
                else
                {
                    // Calcular el camino hacia la posición alertada
                    FindPathToAlertedPosition();
                }
            }
                // Continuar con el patrullaje
                Patrol();
        }
    }
    private void MoveToNextWaypoint()
    {
        // Moverse hacia el siguiente waypoint
        Vector3 targetPosition = currentWaypoint;
        Vector3 movement = targetPosition - transform.position;
        movement.Normalize();
        transform.Translate(movement * moveSpeed * Time.deltaTime);
    }
    private void FindPathToNextWaypoint()
    {
        // Obtener la posición actual del agente y la posición del siguiente waypoint
        Vector3 startPosition = transform.position;
        Vector3 goalPosition = waypoints[currentWaypointIndex].position;

        // Calcular el camino utilizando A*
        List<Vector3> path = AStar.FindPath(startPosition, goalPosition);

        // Verificar si se encontró un camino válido
        if (path != null && path.Count > 0)
        {
            // Actualizar la lista de waypoints del agente para seguir el nuevo camino
            waypoints = new Transform[path.Count];
            for (int i = 0; i < path.Count; i++)
            {
                waypoints[i] = new GameObject("Waypoint").transform;
                waypoints[i].position = path[i];
            }

            currentWaypoint = waypoints[currentWaypointIndex].position;
        }
    }
    private void Patrol()
    {
        // Moverse hacia el siguiente waypoint
        Vector3 targetPosition = waypoints[currentWaypointIndex].position;
        Vector3 movement = targetPosition - transform.position;
        movement.Normalize();
        transform.Translate(movement * moveSpeed * Time.deltaTime);

        // Si alcanza el waypoint, avanzar al siguiente
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }

        // Verificar si el jugador está en el rango de detección y dentro del campo de visión
        if (Vector3.Distance(transform.position, player.position) <= detectionRange)
        {
            Vector3 directionToPlayer = player.position - transform.position;
            float angleToPlayer = Vector3.Angle(directionToPlayer, transform.forward);

            if (angleToPlayer <= fieldOfViewAngle * 0.5f)
            {
                // El jugador está dentro del campo de visión, detectarlo
                playerDetected = true;

                // Alertar a los demás agentes
                BroadcastMessage("OnPlayerDetected", player.position);
            }
        }
    }

    private void FindPathToPlayer()
    {
        // Obtener la posición actual del agente y la posición del jugador
        Vector3 startPosition = transform.position;
        Vector3 goalPosition = player.position;

        // Calcular el camino utilizando A*
        List<Vector3> path = AStar.FindPath(startPosition, goalPosition);

        // Verificar si se encontró un camino válido
        if (path != null && path.Count > 0)
        {
            // Actualizar la lista de waypoints del agente para seguir el nuevo camino
            waypoints = new Transform[path.Count];
            for (int i = 0; i < path.Count; i++)
            {
                waypoints[i] = new GameObject("Waypoint").transform;
                waypoints[i].position = path[i];
            }

            currentWaypointIndex = 0;
            currentWaypoint = waypoints[currentWaypointIndex].position;
        }
    }

    private void FindPathToAlertedPosition()
    {
        // Obtener la posición actual del agente y la posición alertada
        Vector3 startPosition = transform.position;
        Vector3 goalPosition = alertedPosition;

        // Calcular el camino utilizando A*
        List<Vector3> path = AStar.FindPath(startPosition, goalPosition);

        // Verificar si se encontró un camino válido
        if (path != null && path.Count > 0)
        {
            // Actualizar la lista de waypoints del agente para seguir el nuevo camino
            waypoints = new Transform[path.Count];
            for (int i = 0; i < path.Count; i++)
            {
                waypoints[i] = new GameObject("Waypoint").transform;
                waypoints[i].position = path[i];
            }

            currentWaypointIndex = 0;
            currentWaypoint = waypoints[currentWaypointIndex].position;
        }
    }

    private void OnPlayerDetected(Vector3 playerPosition)
    {
        // Un agente vecino ha detectado al jugador, comprobar si está en línea de visión
        Vector3 directionToPlayer = playerPosition - transform.position;
        float angleToPlayer = Vector3.Angle(directionToPlayer, transform.forward);

        if (angleToPlayer <= fieldOfViewAngle * 0.5f)
        {
            // El jugador está dentro del campo de visión, actualizar la posición del jugador detectado
            playerDetected = true;
            player.position = playerPosition;
        }
        else
        {
            // El jugador no está en línea de visión, calcular el camino hacia la posición del jugador
            FindPathToPlayer();
        }
        // Guardar la posición alertada para el cálculo del camino
        alertedPosition = playerPosition;
        isPatrolling = false;
    }

    //asdasdasdasda
}
