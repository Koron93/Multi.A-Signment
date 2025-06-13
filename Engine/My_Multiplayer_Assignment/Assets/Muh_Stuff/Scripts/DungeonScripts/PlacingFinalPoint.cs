using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

public class PlacingFinalPoint : MonoBehaviour
{
    List<DataTile> AllRooms;
    private HashSet<DataTile> Rooms(DataTile StartTile)
    {
        // SortedDictionary to handle tiles sorted by their GCost.
        SortedDictionary<int, Queue<DataTile>> OpenSet = new SortedDictionary<int, Queue<DataTile>>();
        HashSet<DataTile> ClosedSet = new HashSet<DataTile>();

        StartTile.GCost = 0;

        // Add the start tile to the OpenSet
        if (!OpenSet.ContainsKey(StartTile.GCost))
        {
            OpenSet[StartTile.GCost] = new Queue<DataTile>();
        }
        OpenSet[StartTile.GCost].Enqueue(StartTile);

        while (OpenSet.Count > 0)
        {
            // Get the key with the lowest GCost (first key in the SortedDictionary)
            var lowestCostKey = OpenSet.Keys.First();
            var lowestCostCells = OpenSet[lowestCostKey];

            // Dequeue the tile with the lowest GCost
            DataTile currentTile = lowestCostCells.Dequeue();

            // Remove the key if there are no more tiles with this GCost
            if (lowestCostCells.Count == 0)
            {
                OpenSet.Remove(lowestCostKey);
            }

            // Add the current tile to the ClosedSet
            ClosedSet.Add(currentTile);

            // Get neighbors of the current tile
            List<DataTile> neighbors = GetNeighbors(currentTile);

            foreach (DataTile neighbor in neighbors)
            {
                // Skip the neighbor if it's already in the ClosedSet
                if (ClosedSet.Contains(neighbor))
                    continue;

                // Calculate the GCost for the neighbor
                int PreEmptiveGCost = currentTile.GCost + ManhattanCals(neighbor, currentTile);

                // If the neighbor is not in OpenSet or if we found a better GCost for the neighbor
                if (!OpenSet.Any(kv => kv.Value.Contains(neighbor)) || PreEmptiveGCost < neighbor.GCost)
                {
                    // Update the GCost
                    neighbor.GCost = PreEmptiveGCost;

                    // Debugging log to check the updated GCost of the neighbor
                    Debug.Log("Neighbor's new GCost: " + neighbor.GCost);

                    // If the neighbor already exists in OpenSet, remove it
                    if (OpenSet.Any(kv => kv.Value.Contains(neighbor)))
                    {
                        foreach (var key in OpenSet.Keys)
                        {
                            if (OpenSet[key].Contains(neighbor))
                            {
                                // Remove the neighbor from its current queue
                                Queue<DataTile> tempQueue = OpenSet[key];
                                tempQueue = new Queue<DataTile>(tempQueue.Where(tile => tile != neighbor));
                                OpenSet[key] = tempQueue;
                                break;
                            }
                        }
                    }

                    // Add the neighbor to OpenSet under the new GCost
                    if (!OpenSet.ContainsKey(neighbor.GCost))
                    {
                        OpenSet[neighbor.GCost] = new Queue<DataTile>();
                    }
                    OpenSet[neighbor.GCost].Enqueue(neighbor);
                }
            }
        }

        Debug.Log(ClosedSet.Count);  // Log the number of closed tiles
        return ClosedSet;  // Return the closed set of visited tiles
    }
    public void FindAllRooms() 
    {
        GameObject[] allRooms = GameObject.FindGameObjectsWithTag("Room");
        AllRooms = new List<DataTile>();
        foreach(GameObject room in allRooms)
        {
            DataTile dataTile = new DataTile(room.GetComponent<RoomBehaviour>(), 0);
            AllRooms.Add(dataTile);
        }
    }
    private DataTile FindTile(Vector3 Start)
    {
        print("got this far");
        float threshhold = 0.1f;
        foreach(DataTile dataTile in AllRooms)
        {
            print("Got here atleast");
            if(Vector3.Distance(Start, dataTile.Room.transform.position) < threshhold)
            {
                print("should work");
                return dataTile;
            }
        }
        return null;
    }
    private DataTile FinalTile(HashSet<DataTile> Set)
    {
        // Initialize EndTile with a default value
        DataTile EndTile = new DataTile(null, 0);
        int HighestGCost = int.MinValue;  // Start with the smallest possible value for HighestGCost

        foreach (DataTile dataTile in Set)
        {
            // Update EndTile if the current tile has a higher GCost than the current highest
            if (HighestGCost < dataTile.GCost)
            {
                HighestGCost = dataTile.GCost;  // Update the highest GCost
                EndTile = dataTile;  // Update EndTile to the current tile
                Debug.Log(EndTile.Room.transform.position);  // Log the position of the room associated with the EndTile
            }
        }

        return EndTile;  // Return the tile with the highest GCost
    }
    public Vector3 PlacingFinalTile(Vector3 Start)
    {
        DataTile newtile = FindTile(Start);
        HashSet<DataTile>newSet = Rooms(newtile);
        DataTile finaly = FinalTile(newSet);
        Debug.Log(finaly.Room.transform.position);
        return finaly.Room.transform.position;
    }
    private int ManhattanCals (DataTile a, DataTile b)
    {
        float cost = Mathf.Abs(a.Room.transform.position.x - b.Room.transform.position.x) + Mathf.Abs(a.Room.transform.position.y - b.Room.transform.position.y);
        int GCost = Mathf.FloorToInt(cost / 6.4f);
        return GCost;
    }
    private List<DataTile> GetNeighbors(DataTile Cell)
    {
        List<DataTile> neighbors = new List<DataTile>();
        float x = Cell.Room.transform.position.x;
        float z = Cell.Room.transform.position.z;
        List<Vector3> directions = new List<Vector3>();

        if (Cell.Room.doors[0].activeSelf) directions.Add(new Vector3(0, 0, 6.4f)); // Door up
        if (Cell.Room.doors[1].activeSelf) directions.Add(new Vector3(0, 0, -6.4f));  // Door down
        if (Cell.Room.doors[2].activeSelf) directions.Add(new Vector3(6.4f, 0, 0));  // Door right
        if (Cell.Room.doors[3].activeSelf) directions.Add(new Vector3(-6.4f, 0, 0)); // Door left

        foreach (var direction in directions)
        {
            Vector3 neighborPosition = new Vector3(x + direction.x, 0, z + direction.z);
            DataTile neighborCell = FindTile(neighborPosition);
            if (neighborCell != null)
            {
                neighbors.Add(neighborCell);
            }
        }

        return neighbors;
    }
}
