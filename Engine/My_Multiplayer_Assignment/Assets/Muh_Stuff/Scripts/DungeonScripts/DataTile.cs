using UnityEngine;

public class DataTile
{
    public DataTile(RoomBehaviour room, int gCost)
    {
        GCost = gCost;
        Room = room;
    }
    public int GCost;
    public RoomBehaviour Room;
}
