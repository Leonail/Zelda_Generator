using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum direction : byte
{
    up = 1,
    right = 2,
    down = 4,
    left = 8,
    up_right = up | right,
    up_down = up | down,
    up_left = up | left,
    right_down = right | down,
    right_left = right | left,
    down_left = down | left,
    no_up = right | down | left,
    no_right = up | down | left,
    no_down = up | right | left,
    no_left = up | right | down,
    quadri = up | right | down | left
}

public class Map : MonoBehaviour
{
    public int width, height, dungeon_amount;
    public (int, int) starting_point;
    public List<List<Tile>> map;
    public List<Tile> paths; // temp until dungeon_paths works
    public List<Tile>[] dungeons_paths;
    public List<Tile> dungeons;
    public Dictionary<int, GameObject> tileset; //= new Dictionary<int, GameObject>();
    public GameObject f, b, u, r, d, l, u_r, u_d, u_l, r_d, r_l, d_l, n_u, n_r, n_d, n_l, q, dj, sp;
    public Dictionary<int, GameObject> tile_groups; //= new Dictionary<int, GameObject>();
    public List<List<GameObject>> tile_grid; //= new List<List<GameObject>>();
    public Tile out_of_bound_tile; //= new Tile(-1, -1);

    public void Start()
    {
        //Map new_map = new Map(10, 10, 4);
        this.paths = new List<Tile>();
        this.map = new List<List<Tile>>();
        this.out_of_bound_tile = new Tile(-1, -1);
        this.tileset = new Dictionary<int, GameObject>();
        this.tile_groups = new Dictionary<int, GameObject>();
        this.tile_grid = new List<List<GameObject>>();
        Temp(10, 10, 40);
        GenerateMap();
        InstanciateMap();
        GetPossibleBranch();
    }

    public void Temp(int width, int height, int dungeon_amount)
    {
        this.width = width;
        this.height = height;
        this.dungeon_amount = dungeon_amount;
        this.dungeons_paths = new List<Tile>[this.dungeon_amount];
        // Set out of bound tile
        this.out_of_bound_tile.out_of_bound = true;
        // init tileset
        this.tileset.Add(0, f);
        this.tileset.Add(1, u);
        this.tileset.Add(2, r);
        this.tileset.Add(3, u_r);
        this.tileset.Add(4, d);
        this.tileset.Add(5, u_d);
        this.tileset.Add(6, r_d);
        this.tileset.Add(7, n_l);
        this.tileset.Add(8, l);
        this.tileset.Add(9, u_l);
        this.tileset.Add(10, r_l);
        this.tileset.Add(11, n_d);
        this.tileset.Add(12, d_l);
        this.tileset.Add(13, n_r);
        this.tileset.Add(14, n_u);
        this.tileset.Add(15, q);
        this.tileset.Add(16, b);
        this.tileset.Add(17, sp);
        this.tileset.Add(18, dj);
        // init tile group
        foreach (KeyValuePair<int, GameObject> prefab_pair in this.tileset)
        {
            GameObject tile_group = new GameObject(prefab_pair.Value.name);
            tile_group.transform.parent = gameObject.transform;
            tile_group.transform.localPosition = new Vector3(0, 0, 0);
            tile_groups.Add(prefab_pair.Key, tile_group);
        }
        // 
        this.starting_point = ((int)Random.Range(0.0f, this.height), (int)Random.Range(0.0f, this.width));
        for (int x = 0; x < this.width; x++)
        {
            this.map.Add(new List<Tile>());
            for (int y = 0; y < this.height; y++)
            {
                this.map[x].Add(new Tile(x, y));
            }
        }
        for (int x = 0; x < this.width; x++)
        {
            for (int y = 0; y < this.height; y++)
            {
                this.map[x][y].SetNeighbor(this);
            }
        }
        this.map[this.starting_point.Item1][this.starting_point.Item2].AddOpening((byte)direction.quadri);
    }

    public Map(int width, int height, int dungeon_amount)
    {
        this.width = width;
        this.height = height;
        this.dungeon_amount = dungeon_amount;
        // Set out of bound tile
        this.out_of_bound_tile.out_of_bound = true;
        // init tileset
        this.tileset.Add(0, f);
        this.tileset.Add(1, u);
        this.tileset.Add(2, r);
        this.tileset.Add(3, u_r);
        this.tileset.Add(4, d);
        this.tileset.Add(5, u_d);
        this.tileset.Add(6, r_d);
        this.tileset.Add(7, n_l);
        this.tileset.Add(8, l);
        this.tileset.Add(9, u_l);
        this.tileset.Add(10, r_l);
        this.tileset.Add(11, n_d);
        this.tileset.Add(12, d_l);
        this.tileset.Add(13, n_r);
        this.tileset.Add(14, n_u);
        this.tileset.Add(15, q);
        this.tileset.Add(16, b);
        this.tileset.Add(17, sp);
        this.tileset.Add(18, dj);
        // init tile group
        foreach (KeyValuePair<int, GameObject> prefab_pair in this.tileset)
        {
            GameObject tile_group = new GameObject(prefab_pair.Value.name);
            tile_group.transform.parent = gameObject.transform;
            tile_group.transform.localPosition = new Vector3(0, 0, 0);
            tile_groups.Add(prefab_pair.Key, tile_group);
        }
        // 
        this.starting_point = ((int)Random.Range(0.0f, this.height), (int)Random.Range(0.0f, this.width));
        for (int x = 0; x < this.width; x++)
        {
            this.map.Add(new List<Tile>());
            for (int y = 0; y < this.height; y++)
            {
                this.map[x].Add(new Tile(x, y));
            }
        }
        this.map[this.starting_point.Item1][this.starting_point.Item2].AddOpening((byte)direction.quadri);
    }

    public bool TileIsValid(int x, int y)
    {
        if (x < this.height & x >= 0 & y < this.width & y >= 0)
        {
            if (this.map[x][y].openings == 0)
            { return true; }
            else
            { return false; }
        }
        else
        { return false; }
    }

    public bool TileCanBeDungeon(int x, int y, int depth)
    {
        //Debug.Log("x : " + x + " y : " + y);
        if (depth < 4)
        {
            depth++;
            Tile tile = this.map[x][y];
            foreach (Tile next_tile in tile.neighbor)
            {
                if (next_tile != null && !next_tile.out_of_bound)
                {
                    if (next_tile.dungeon) { /*Debug.Log(" x : " + next_tile.x + " y : " + next_tile.y);*/ return false; }
                    else { TileCanBeDungeon(next_tile.x, next_tile.y, depth); }
                }
            }
        }
        return true;

    }

    public bool TileCanBeBranch(int x, int y)
    {
        Tile tile = this.map[x][y];
        if (!tile.out_of_bound && !tile.dungeon && tile.available_directions != 0) { return true; }
        else { return false; }
    }

    public Tile GetPossibleBranch()
    {
        List<Tile> paths_copy = new List<Tile>();
        for (int i = 0; i < this.paths.Count; i++)
        {
            paths_copy.Add(this.paths[i]);
        }
        while (paths_copy.Count > 0)
        {
            int paths_index = (int)Random.Range(0.0f, paths_copy.Count);
            Tile tile = paths_copy[paths_index];
            if (TileCanBeBranch(tile.x, tile.y)) { return tile; }
            paths_copy.RemoveAt(paths_index);
        }
        return this.out_of_bound_tile;
    }

    public void GeneratePath(int x, int y, int dungeon_index, int max_size, int depth)
    {
        Debug.Log("Building path for dungeon " + dungeon_index + " adding tile x " + x + " y " + y);

        Tile tile = this.map[x][y];
        tile.Update();
        //tile.UpdateNeighbor();

        Debug.Log("Tile available dir are " + tile.available_directions);
        if (depth <= max_size)
        {
            //tile.Update();
            List<(Tile, byte, byte)> available_neighbor = new List<(Tile, byte, byte)>();
            if ((tile.available_directions & (byte)direction.up) != 0 && !tile.neighbor[0].out_of_bound) { available_neighbor.Add((tile.neighbor[0], (byte)direction.up, (byte)direction.down)); }
            if ((tile.available_directions & (byte)direction.right) != 0 && !tile.neighbor[1].out_of_bound) { available_neighbor.Add((tile.neighbor[1], (byte)direction.right, (byte)direction.left)); }
            if ((tile.available_directions & (byte)direction.down) != 0 && !tile.neighbor[2].out_of_bound) { available_neighbor.Add((tile.neighbor[2], (byte)direction.down, (byte)direction.up)); }
            if ((tile.available_directions & (byte)direction.left) != 0 && !tile.neighbor[3].out_of_bound) { available_neighbor.Add((tile.neighbor[3], (byte)direction.left, (byte)direction.right)); }
            int available_neighbor_amount = available_neighbor.Count;
            if (available_neighbor_amount != 0)
            {
                int next_tile_index = (int)Random.Range(0.0f, available_neighbor_amount);
                Tile next_tile = available_neighbor[next_tile_index].Item1;
                tile.AddOpening(available_neighbor[next_tile_index].Item2);
                next_tile.AddOpening(available_neighbor[next_tile_index].Item3);
                // Add the tile to the path of the current dungeon
                this.dungeons_paths[dungeon_index].Add(tile);
                this.paths.Add(tile);
                depth++;
                GeneratePath(next_tile.x, next_tile.y, dungeon_index, max_size, depth);
            }
            else { /*Debug.Log(TileCanBeDungeon(tile.x, tile.y, 0));*/ tile.dungeon = true;/* dungeons.Add(tile);*/ }
        }
        else { /*Debug.Log(TileCanBeDungeon(tile.x, tile.y, 0));*/ tile.dungeon = true; /*dungeons.Add(tile);*/ }
    }

    public void GenerateMap()
    {
        this.dungeons_paths[0] = new List<Tile>();
        GeneratePath(this.starting_point.Item1, this.starting_point.Item2, 0, 8, 0);
        for (int i = 1; i < this.dungeon_amount; i++)
        {
            this.dungeons_paths[i] = new List<Tile>();

            Tile branch_tile = GetPossibleBranch();
            if(branch_tile.out_of_bound) { Debug.Log("This map is not viable"); }


            //int dungeon_paths_index = (int)Random.Range(0.0f, i-1); // choose a random existing dungeon path
            //int paths_index = (int)Random.Range(0.0f, this.dungeons_paths[dungeon_paths_index].Count); // choose a random tile in the dungeon path


            //int paths_index = (int)Random.Range(0.0f, this.paths.Count);
            //GeneratePath(this.dungeons_paths[dungeon_paths_index][paths_index].x, this.dungeons_paths[dungeon_paths_index][paths_index].y, i, 8, 0);
            GeneratePath(branch_tile.x, branch_tile.y, i, 8, 0);

            /*
            Debug.Log("Current path length is : " + this.dungeons_paths[i].Count);
            Debug.Log("It started on the path of the dungeon : " + dungeon_paths_index);
            Debug.Log("It's length is : " + this.dungeons_paths[dungeon_paths_index].Count + " and it begins at : " + paths_index);
            int distance = this.dungeons_paths[dungeon_paths_index].Count - paths_index + this.dungeons_paths[i].Count;
            Debug.Log("The distance from previous dugneon is : " + distance);
            */

            //GeneratePath(this.paths[paths_index].x, this.paths[paths_index].y, i, 8, 0);
        }

    }

    public void InstanciateTile(int x, int y)
    {
        /** Creates a new tile using the type id code, group it with common
    tiles, set it's position and store the gameobject. **/
        int tile_id = this.map[x][y].openings;
        if (map[x][y].dungeon) { tile_id = 18; }
        if (x == this.starting_point.Item1 && y == this.starting_point.Item2) { tile_id = 17; }
        GameObject tile_prefab = this.tileset[tile_id];
        GameObject tile_group = tile_groups[tile_id];
        GameObject tile = Instantiate(tile_prefab, tile_group.transform);

        tile.name = string.Format("tile_x{0}_y{1}_id{2}", x, y, tile_id);
        tile.transform.localPosition = new Vector3(x, y, 0);
        this.tile_grid[x].Add(tile);
    }

    public void InstanciateMap()
    {
        for (int x = 0; x < this.width; x++)
        {
            this.tile_grid.Add(new List<GameObject>());
            for (int y = 0; y < this.height; y++)
            {
                InstanciateTile(x, y);
            }
        }
    }
}
