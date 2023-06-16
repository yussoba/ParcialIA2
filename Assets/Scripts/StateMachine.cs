using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public enum EnemyState
    {
        Idle,
        Patrolling,
        Chasing,
    }

    public class EnemyController : MonoBehaviour
    {
        public EnemyState currentState = EnemyState.Idle;
        public List<Node> path;
        private int currentNodeIndex = 0;
        private Node targetNode;
        private GameObject player;

        private void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        private void Update()
        {
            switch (currentState)
            {
                case EnemyState.Idle:
                    // Lógica de espera o inactividad
                    break;
                case EnemyState.Patrolling:
                    // Lógica de patrullaje
                    break;
                case EnemyState.Chasing:
                    if (targetNode != null)
                    {
                        MoveTowardsTargetNode();
                        if (HasReachedTargetNode())
                        {
                            currentNodeIndex++;
                            if (currentNodeIndex < path.Count)
                            {
                                targetNode = path[currentNodeIndex];
                            }
                            else
                            {
                                // Se ha alcanzado el último nodo de la ruta (jugador alcanzado)
                                // Realiza las acciones apropiadas (atacar, mostrar mensaje de derrota, etc.)
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private void MoveTowardsTargetNode()
        {
            // Implementa la lógica de movimiento del enemigo hacia el nodo objetivo
            // Utiliza Vector3.MoveTowards u otra función de movimiento según tu necesidad
        }

        private bool HasReachedTargetNode()
        {
            // Implementa la lógica para verificar si el enemigo ha alcanzado el nodo objetivo actual
            // Esto puede basarse en la posición actual del enemigo y la posición del nodo objetivo
            // Retorna true si ha alcanzado el nodo objetivo, de lo contrario, retorna false
            return false;
        }
    }
}
