using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStar : MonoBehaviour
{
    private class Node
    {
        public Vector3 position;
        public List<Node> neighbors;
        public float gScore; // Costo acumulado desde el inicio hasta este nodo
        public float hScore; // Heur�stica estimada desde este nodo hasta el objetivo
        public float fScore => gScore + hScore; // Suma de gScore y hScore
        public Node cameFrom; // Nodo anterior en el camino �ptimo

        public Node(Vector3 position)
        {
            this.position = position;
            neighbors = new List<Node>();
            gScore = Mathf.Infinity;
            hScore = 0f;
            cameFrom = null;
        }
    }

    public static List<Vector3> FindPath(Vector3 start, Vector3 goal)
    {
        // Obtener los nodos de inicio y objetivo
        Node startNode = new Node(start);
        Node goalNode = new Node(goal);

        // Construir el grafo del mapa con los nodos necesarios
        List<Node> allNodes = BuildMapGraph();

        // Inicializar la lista abierta y cerrada del algoritmo A*
        List<Node> openSet = new List<Node> { startNode };
        List<Node> closedSet = new List<Node>();

        // Asignar gScore de inicio a 0
        startNode.gScore = 0f;

        while (openSet.Count > 0)
        {
            // Obtener el nodo con el fScore m�s bajo de la lista abierta
            Node currentNode = GetNodeWithLowestFScore(openSet);

            // Si el nodo actual es el objetivo, se ha encontrado el camino �ptimo
            if (currentNode == goalNode)
                return ReconstructPath(currentNode);

            // Mover el nodo actual de la lista abierta a la cerrada
            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            // Explorar los vecinos del nodo actual
            foreach (Node neighbor in currentNode.neighbors)
            {
                // Si el vecino est� en la lista cerrada, continuar con el siguiente vecino
                if (closedSet.Contains(neighbor))
                    continue;

                // Calcular el nuevo gScore para el vecino
                float tentativeGScore = currentNode.gScore + Vector3.Distance(currentNode.position, neighbor.position);

                // Si el vecino no est� en la lista abierta o el nuevo gScore es menor
                if (!openSet.Contains(neighbor) || tentativeGScore < neighbor.gScore)
                {
                    // Actualizar la informaci�n del vecino
                    neighbor.cameFrom = currentNode;
                    neighbor.gScore = tentativeGScore;
                    neighbor.hScore = HeuristicEstimate(neighbor.position, goalNode.position);

                    // Si el vecino no est� en la lista abierta, agregarlo
                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        // No se encontr� un camino v�lido
        return null;
    }

    private static List<Vector3> ReconstructPath(Node node)
    {
        List<Vector3> path = new List<Vector3>();
        Node current = node;

        while (current != null)
        {
            path.Add(current.position);
            current = current.cameFrom;
        }

        path.Reverse();
        return path;
    }

    private static Node GetNodeWithLowestFScore(List<Node> nodes)
    {
        Node lowestFScoreNode = nodes[0];

        for (int i = 1; i < nodes.Count; i++)
        {
            if (nodes[i].fScore < lowestFScoreNode.fScore)
                lowestFScoreNode = nodes[i];
        }

        return lowestFScoreNode;
    }

    private static float HeuristicEstimate(Vector3 start, Vector3 goal)
    {
        // Utiliza una heur�stica simple, la distancia en l�nea recta
        return Vector3.Distance(start, goal);
    }

    // M�todo de ejemplo para construir el grafo del mapa con nodos y vecinos
    private static List<Node> BuildMapGraph()
    {
        // Implementa la l�gica para construir el grafo del mapa con nodos y vecinos
        // ...
        // Aseg�rate de tener una lista con todos los nodos necesarios y establece los vecinos adecuados para cada nodo
        // ...

        return new List<Node>();
    }
}
