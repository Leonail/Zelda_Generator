using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile
{
    public int x, y;
    public bool dungeon = false;
    public bool out_of_bound = false;
    public byte openings = 0;
    public byte available_directions = 0;

    //public List<Tile> neighbor = new List<Tile>();
    public Tile[] neighbor = new Tile[4]; // { new Tile(-1, -1), new Tile(-1, -1) , new Tile(-1, -1) , new Tile(-1, -1) };

    // TODO gestion des neighbor defectueurse

    //public List<List<RoomTile>> room = List<List<RoomTile>>();

    public Tile(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
    public void Update()
    {
        if (this.neighbor[0] != null && this.neighbor[0].openings == 0) { this.available_directions = (byte)(this.available_directions | (byte)direction.up); }
        if (this.neighbor[1] != null && this.neighbor[1].openings == 0) { this.available_directions = (byte)(this.available_directions | (byte)direction.right); }
        if (this.neighbor[2] != null && this.neighbor[2].openings == 0) { this.available_directions = (byte)(this.available_directions | (byte)direction.down); }
        if (this.neighbor[3] != null && this.neighbor[3].openings == 0) { this.available_directions = (byte)(this.available_directions | (byte)direction.left); }
    }
    public void SetNeighbor(Map map)
    {
        if (this.x - 1 >= 0) { this.neighbor[3] = map.map[this.x - 1][this.y]; }
        else { this.neighbor[0] = map.out_of_bound_tile; }
        if (this.y + 1 < map.width) { this.neighbor[0] = map.map[this.x][this.y + 1]; }
        else { this.neighbor[1] = map.out_of_bound_tile; }
        if (this.x + 1 < map.height) { this.neighbor[1] = map.map[this.x + 1][this.y]; }
        else { this.neighbor[2] = map.out_of_bound_tile; }
        if (this.y - 1 >= 0) { this.neighbor[2] = map.map[this.x][this.y - 1]; }
        else { this.neighbor[3] = map.out_of_bound_tile; }
    }

    public void AddOpening(byte added_direction)
    {
        this.openings = (byte)(this.openings | added_direction);
    }

    public void UpdateNeighbor()
    {
        foreach (Tile neighnb in neighbor)
        {
            if(neighnb != null && !neighnb.out_of_bound)
            {
                neighnb.Update();
            }
        }
    }
}