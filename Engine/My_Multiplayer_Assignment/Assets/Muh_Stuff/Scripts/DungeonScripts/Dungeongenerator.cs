using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.Netcode;

public class Dungeongenerator : MonoBehaviour
{
    public class Cell 
    {
        public bool visited= false;
        public bool[] status = new bool[4];
    }
    private int Vector2x;
    private int Vector2y;
    public Vector2 size;
    public int startPos = 0;
    List<Cell> board;
    public GameObject room;
    public Vector2 offset;
    public bool Dungeon = false;
    public void  DungeonStart()
    {
        Vector2x = Random.Range(3, 5);
        Vector2y = Random.Range(3, 5);
        size = new Vector2(Vector2x, Vector2y);
        MazeGenerator();
    }
    void MazeGenerator() 
    {
        board = new List<Cell>();

        for (int i = 0; i < size.x; i++) 
        {
            for (int j = 0; j < size.y; j++)
            {
                board.Add(new Cell());
            }
        }
        int currentCell = startPos;

        Stack<int> path = new Stack<int>();

        int k = 0;

        while (k < 1000)
        {
            k++;

            board[currentCell].visited = true;

            //Check neighbor cells
            List<int> neighbors = CheckNeighbors(currentCell);

            if (neighbors.Count == 0) 
            { 
                if(path.Count == 0)
                {
                    break;
                }
                else
                {
                    currentCell = path.Pop();
                }
            }
            else
            {
                path.Push(currentCell);

                int newCell = neighbors[Random.Range(0, neighbors.Count)];

                if(newCell > currentCell)
                {
                    //down or right
                    if(newCell -1 == currentCell)
                    {
                        board[currentCell].status[2] = true;
                        currentCell = newCell;
                        board[currentCell].status[3] = true;
                    }
                    else
                    {
                        board[currentCell].status[1] = true;
                        currentCell = newCell;
                        board[currentCell].status[0] = true;
                    }
                }
                else
                {
                    //up or left
                    if (newCell + 1 == currentCell)
                    {
                        board[currentCell].status[3] = true;
                        currentCell = newCell;
                        board[currentCell].status[2] = true;
                    }
                    else
                    {
                        board[currentCell].status[0] = true;
                        currentCell = newCell;
                        board[currentCell].status[1] = true;
                    }
                }
            }
        }
        GenerateDungeon();
        Dungeon = true;
    }

    List<int> CheckNeighbors(int cell) 
    {
        List<int> neighbors = new List<int>();
        // check neighbor up
        if (cell - size.x >= 0 && !board[(Mathf.FloorToInt(cell - size.x))].visited)
        {
            neighbors.Add(Mathf.FloorToInt(cell - size.x));
        }
        // check neighbor down
        if (cell + size.x < board.Count && !board[Mathf.FloorToInt(cell + size.x)].visited)
        {
            neighbors.Add(Mathf.FloorToInt(cell + size.x));
        }

        // check neighbor right
        if ((cell + 1) % size.x != 0 && !board[Mathf.FloorToInt(cell + 1)].visited)
        {
            neighbors.Add(Mathf.FloorToInt(cell + 1));
        }

        // check neighbor left
        if (cell % size.x != 0 && !board[Mathf.FloorToInt(cell - 1)].visited)
        {
            neighbors.Add(Mathf.FloorToInt(cell - 1));
        }

        return neighbors;
    }
    void GenerateDungeon()
    {
        Debug.Log("GenerateDungeon called!");
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                // Get the current cell data (assuming currentCell holds the room status)
                Cell currentCell = board[Mathf.FloorToInt(i + j * size.x)];

                // Spawn the room using ObjectPoolManager, ensuring it is networked
                GameObject newRoomObj = ObjectPoolManager.SpawnObjectServerRpc(room, new Vector3(i * offset.x, 0, -j * offset.y), Quaternion.identity, 0);
                print("instantiated newRoom");
                RoomBehaviour newRoom = newRoomObj.GetComponent<RoomBehaviour>();

                // Ensure that the object has a NetworkObject component attached
                NetworkObject networkObject = newRoomObj.GetComponent<NetworkObject>();

                if (networkObject == null)
                {
                    Debug.LogError("The spawned room object is missing a NetworkObject component.");
                    continue; // Skip this iteration if no NetworkObject is found
                }

                // Spawn the networked object for all clients
                print("About to spawn newRoom");
                newRoom.name += " " + i + "-" + j;
                newRoom.UpdateRoomClientRpc(currentCell.status);// Update the room's state via RPC
                print("Spawned newRoom");
                // Optionally, ensure the object is being synchronized by logging network status
                if (networkObject.IsSpawned)
                {
                    Debug.Log("Room object successfully spawned and synchronized with clients.");
                }
                else
                {
                    Debug.LogError("Failed to spawn room object on the network.");
                }
            }
        }
    }
}
