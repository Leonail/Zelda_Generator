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
    public List<List<Tile>> dungeons_paths;
    public List<Tile> dungeons;
    public Dictionary<int, GameObject> tileset; //= new Dictionary<int, GameObject>();
    public GameObject f, b, u, r, d, l, u_r, u_d, u_l, r_d, r_l, d_l, n_u, n_r, n_d, n_l, q;
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
        Temp(10, 10, 4);
        GenerateMap();
        InstanciateMap();
    }

    public void Temp(int width, int height, int dungeon_amount)
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

    public void GeneratePath(int x, int y, int max_size, int depth)
    {
        Tile tile = this.map[x][y];
        if (depth <= max_size)
        {
            tile.Update();
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
                Debug.Log("x : " + x + " y : " + y + " dir : " + available_neighbor[next_tile_index].Item2);
                tile.AddOpening(available_neighbor[next_tile_index].Item2);
                next_tile.AddOpening(available_neighbor[next_tile_index].Item3);
                // Add the tile to the path of the current dungeon
                this.paths.Add(tile);
                depth++;
                GeneratePath(next_tile.x, next_tile.y, max_size, depth);
            }
            else { tile.dungeon = true;}
        }
        else { tile.dungeon = true;}
    }

    public void GenerateMap()
    {
        GeneratePath(this.starting_point.Item1, this.starting_point.Item2, 8, 0);
        for (int i = 1; i < this.dungeon_amount; i++)
        {
            Debug.Log(paths.Count);
            int paths_index = (int)Random.Range(0.0f, this.paths.Count);
            GeneratePath(this.paths[paths_index].x, this.paths[paths_index].y, 8, 0);
        }

    }

    public void InstanciateTile(int x, int y)
    {
        /** Creates a new tile using the type id code, group it with common
    tiles, set it's position and store the gameobject. **/
        int tile_id = this.map[x][y].openings;
        Debug.Log("x : " + x + " y : " + y + " tile id : " + tile_id);
        GameObject tile_prefab = this.tileset[tile_id];
        GameObject tile_group = tile_groups[tile_id];
        GameObject tile = Instantiate(tile_prefab, tile_group.transform);

        tile.name = string.Format("tile_x{0}_y{1}_id{2}", x, y, tile_id);
        tile.transform.localPosition = new Vector3(x, y, 0);
        //tile.transform.localRotation = new Quaternion(-90, 0, 0, 0);
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
        //Debug.Log("X : " + this.x + " Y : " + this.y);
        //Debug.Log(this.neighbor[0] != null && this.neighbor[0].openings == 0);
        if (this.neighbor[0] != null && this.neighbor[0].openings == 0) { this.available_directions = (byte)(this.available_directions | (byte)direction.up); }
        //Debug.Log(this.neighbor[1] != null);
        if (this.neighbor[1] != null && this.neighbor[1].openings == 0) { this.available_directions = (byte)(this.available_directions | (byte)direction.right); }
        //Debug.Log(this.neighbor[2] != null);
        if (this.neighbor[2] != null && this.neighbor[2].openings == 0) { this.available_directions = (byte)(this.available_directions | (byte)direction.down); }
        //Debug.Log(this.neighbor[3] != null);
        if (this.neighbor[3] != null && this.neighbor[3].openings == 0) { this.available_directions = (byte)(this.available_directions | (byte)direction.left); }
        //Debug.Log("End update");
    }
    public void SetNeighbor(Map map)
    {
        if(this.x-1 >= 0) { this.neighbor[3] = map.map[this.x-1][this.y]; }
        else { this.neighbor[0] = map.out_of_bound_tile;}
        if (this.y+1 < map.width) { this.neighbor[0] = map.map[this.x][this.y+1]; }
        else { this.neighbor[1] = map.out_of_bound_tile; }
        if (this.x+1 < map.height) { this.neighbor[1] = map.map[this.x+1][this.y]; }
        else { this.neighbor[2] = map.out_of_bound_tile; }
        if (this.y-1 >= 0) { this.neighbor[2] = map.map[this.x][this.y-1]; }
        else { this.neighbor[3] = map.out_of_bound_tile; }
    }

    public void AddOpening(byte added_direction)
    {
        this.openings = (byte)(this.openings | added_direction);
    }

    public void UpdateNeighbor()
    {

    }
}

/*
public class Tile
{
    public MapGenerator parent;
    public int x;
    public int y;
    public int tile_id = 0;
    public byte available_direction = 0;
    public List<(int, int, byte)> available_direction_list = new List<(int, int, byte)>();
    public byte tile_opening = 0;

    public Tile(MapGenerator parent, int x, int y)
    {
        this.parent = parent;
        this.x = x;
        this.y = y;
        //this.GetAvailableDirection();
    }

    public void GetAvailableDirection()
    {
        available_direction = 0;
        available_direction_list = new List<(int, int, byte)>();
        if (this.parent.IsValid(x - 1, y)) 
        {
            this.available_direction = (byte)(this.available_direction | 8);
            this.available_direction_list.Add((x-1, y, 8));
        }
        if (this.parent.IsValid(x, y + 1))
        {
            this.available_direction = (byte)(this.available_direction | 1);
            this.available_direction_list.Add((x, y+1, 1));
        }
        if (this.parent.IsValid(x + 1, y)) 
        { 
            this.available_direction = (byte)(this.available_direction | 2);
            this.available_direction_list.Add((x+1, y, 2));
        }
        if (this.parent.IsValid(x, y - 1))
        {
            this.available_direction = (byte)(this.available_direction | 4);
            this.available_direction_list.Add((x, y-1, 4));
        }
    }

    public void AddDirection(byte new_direction)
    {
        this.tile_id = (this.tile_opening | new_direction);
        this.tile_opening = (byte)this.tile_id;
        //Debug.Log(this.tile_opening);
    }
}

public class MapGenerator : MonoBehaviour
{
    Dictionary<int, GameObject> tileset;
    Dictionary<int, GameObject> tile_groups;
    public GameObject prefab_plains;
    public GameObject prefab_forest;
    public GameObject prefab_hills;
    public GameObject prefab_mountains;
    public GameObject f, b, u, r, d, l, u_r, u_d, u_l, r_d, r_l, d_l, n_u, n_r, n_d, n_l, q;

    int map_width = 10;  // seems to cause problems when above 10 ?
    int map_height = 10;
    int dungeon_amount = 4;
    int max_depth = 10;



    byte up = 1;
    byte right = 2;
    byte down = 4;
    byte left = 8;


    (int, int) starting_point;


    List<List<int>> noise_grid = new List<List<int>>();
    List<List<GameObject>> tile_grid = new List<List<GameObject>>();
    List<List<Tile>> path_tile_grid = new List<List<Tile>>();
    List<Tile> path_tile_list = new List<Tile>();

    // recommend 4 to 20
    float magnification = 7.0f;

    int x_offset = 0; // <- +>
    int y_offset = 0; // v- +^

    void Start()
    {
        starting_point = ((int)Random.Range(0.0f, map_height), (int)Random.Range(0.0f, map_width));

        for (int x = 0; x < map_width; x++)
        {
            path_tile_grid.Add(new List<Tile>());
            for (int y = 0; y < map_height; y++)
            {
                path_tile_grid[x].Add(new Tile(this, x, y));
            }
        }
        CreateTileset();
        CreateTileGroups();
        BuildMap();
        GenerateMap();
        byte test_byte = 3;
        Debug.Log(test_byte & (byte)direction.right);
        Debug.Log((byte)direction.right_down);
    }

    void CreateTileset()
    {
        /** Collect and assign ID codes to the tile prefabs, for ease of access.
            Best ordered to match land elevation. **/
/*
        tileset = new Dictionary<int, GameObject>();
        tileset.Add(0, f);
        tileset.Add(1, u);
        tileset.Add(2, r);
        tileset.Add(3, u_r);
        tileset.Add(4, d);
        tileset.Add(5, u_d);
        tileset.Add(6, r_d);
        tileset.Add(7, n_l);
        tileset.Add(8, l);
        tileset.Add(9, u_l);
        tileset.Add(10, r_l);
        tileset.Add(11, n_d);
        tileset.Add(12, d_l);
        tileset.Add(13, n_r);
        tileset.Add(14, n_u);
        tileset.Add(15, q);
        tileset.Add(16, b);
    }

    void CreateTileGroups()
    {
        /** Create empty gameobjects for grouping tiles of the same type, ie
            forest tiles **/
/*
        tile_groups = new Dictionary<int, GameObject>();
        foreach (KeyValuePair<int, GameObject> prefab_pair in tileset)
        {
            GameObject tile_group = new GameObject(prefab_pair.Value.name);
            tile_group.transform.parent = gameObject.transform;
            tile_group.transform.localPosition = new Vector3(0, 0, 0);
            tile_groups.Add(prefab_pair.Key, tile_group);
        }
    }

    /// <summary>
    /// Check if the given tile is valid and available
    /// </summary>
    /// <param name="x">int, tile x coordinate</param>
    /// <param name="y">int, tile y coordinate</param>
    /// <returns>bool</returns>
    public bool IsValid(int x, int y)
    {
        if (x < map_height & x >= 0 & y < map_width & y >= 0)
        { 
            if (path_tile_grid[x][y].tile_id == 0)
            { return true; }
            else 
            { return false; }
        }
        else
        { return false; }
    }

    /// <summary>
    /// A recursive function to draw a random path in the map
    /// </summary>
    /// <param name="x">int, Current tile x coordinate</param>
    /// <param name="y">int, Current tile y coordinate</param>
    /// <param name="depth">int, Current recursive depth</param>
    void DrawPath(int x, int y, int depth)
    {
        if (depth <= max_depth)
        {
            /*Dictionary<int, (int, int, byte)> possible_direction = new Dictionary<int, (int, int, byte)>();
            int index = 0;
            if (IsValid(x - 1, y))
            {
                possible_direction.Add(index, (x - 1, y, 8));
                index++;
            }
            if (IsValid(x, y + 1))
            {
                possible_direction.Add(index, (x, y + 1, 1));
                index++;
            }
            if (IsValid(x + 1, y))
            {
                possible_direction.Add(index, (x + 1, y, 2));
                index++;
            }
            if (IsValid(x, y - 1))
            {
                possible_direction.Add(index, (x, y - 1, 4));
                index++;
            }*/
            // If there is at least one valid direction, choose one at random
            /*
            Tile current_tile = path_tile_grid[x][y];
            current_tile.GetAvailableDirection();
            int available_direction_amount = current_tile.available_direction_list.Count;
            if (available_direction_amount != 0)
            {
                int next_direction = (int)Random.Range(0.0f, available_direction_amount);

                if (depth == max_depth) { current_tile.tile_id = 16; }
                else
                {
                    current_tile.AddDirection(current_tile.available_direction_list[next_direction].Item3);
                    path_tile_list.Add(current_tile);
                }
                depth++;
                DrawPath(current_tile.available_direction_list[next_direction].Item1, current_tile.available_direction_list[next_direction].Item2, depth);
            }
            else { current_tile.tile_id = 16; }
            /*if (index != 0)
            {
                int next_dir = (int)Random.Range(0.0f, index);
                //Debug.Log("index " + index + " next dir " + next_dir);

                // TODO if depth == last depth : put the dungeon.
                if (depth == max_depth) { path_tile_grid[x][y].tile_id = 16; }
                else 
                {
                    //path_tile_grid[x][y].tile_id = possible_direction[next_dir].Item3;
                    path_tile_grid[x][y].AddDirection(possible_direction[next_dir].Item3);
                    path_tile_list.Add(path_tile_grid[x][y]);
                }
                depth++;
                DrawPath(possible_direction[next_dir].Item1, possible_direction[next_dir].Item2, depth);
            }
            else 
            {
                path_tile_grid[x][y].tile_id = 16;
            }*/
            // If there is no valid direction, end the program
            /*
        }

    }

    void BuildMap()
    {
        DrawPath(starting_point.Item1, starting_point.Item2, 0);
        for (int i = 1; i < dungeon_amount; i++)
        {
            int path_tile_index = (int)Random.Range(0.0f, path_tile_list.Count);
            DrawPath(path_tile_list[path_tile_index].x, path_tile_list[path_tile_index].y, 0);
        }
        
    }

    void GenerateMap()
    {
        /** Generate a 2D grid using the Perlin noise fuction, storing it as
            both raw ID values and tile gameobjects **/
        
        //Debug.Log(starting_point);
        /*

        for (int x = 0; x < map_width; x++)
        {
            noise_grid.Add(new List<int>());
            tile_grid.Add(new List<GameObject>());

            for (int y = 0; y < map_height; y++)
            {
                int tile_id = path_tile_grid[x][y].tile_id;
                
                //Create a special tile for the spawn point
                if (x == starting_point.Item1 & y == starting_point.Item2)
                {
                tile_id = 15;
                }
                noise_grid[x].Add(tile_id);
                CreateTile(tile_id, x, y);
            }
        }
    }

    int GetIdUsingPerlin(int x, int y)
    {
        /** Using a grid coordinate input, generate a Perlin noise value to be
            converted into a tile ID code. Rescale the normalised Perlin value
            to the number of tiles available. **/
        /*
        float raw_perlin = Mathf.PerlinNoise(
            (x - x_offset) / magnification,
            (y - y_offset) / magnification
        );
        float clamp_perlin = Mathf.Clamp01(raw_perlin); // Thanks: youtu.be/qNZ-0-7WuS8&lc=UgyoLWkYZxyp1nNc4f94AaABAg
        float scaled_perlin = clamp_perlin * tileset.Count;

        // Replaced 4 with tileset.Count to make adding tiles easier
        if (scaled_perlin == tileset.Count)
        {
            scaled_perlin = (tileset.Count - 1);
        }
        return Mathf.FloorToInt(scaled_perlin);
    }

    void CreateTile(int tile_id, int x, int y)
    {
        /** Creates a new tile using the type id code, group it with common
            tiles, set it's position and store the gameobject. **/
        /*
        GameObject tile_prefab = tileset[tile_id];
        GameObject tile_group = tile_groups[tile_id];
        GameObject tile = Instantiate(tile_prefab, tile_group.transform);

        tile.name = string.Format("tile_x{0}_y{1}_id{2}", x, y, tile_id);
        tile.transform.localPosition = new Vector3(x, y, 0);
        //tile.transform.localRotation = new Quaternion(-90, 0, 0, 0);

        tile_grid[x].Add(tile);
    }
}*/


