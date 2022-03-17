using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Danndx 2021 (youtube.com/danndx)
From video: youtu.be/qNZ-0-7WuS8
thanks - delete me! :) */

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
        this.GetAvailableDirection();
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
        Debug.Log(this.tile_opening);
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
    }

    void CreateTileset()
    {
        /** Collect and assign ID codes to the tile prefabs, for ease of access.
            Best ordered to match land elevation. **/

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

        GameObject tile_prefab = tileset[tile_id];
        GameObject tile_group = tile_groups[tile_id];
        GameObject tile = Instantiate(tile_prefab, tile_group.transform);

        tile.name = string.Format("tile_x{0}_y{1}_id{2}", x, y, tile_id);
        tile.transform.localPosition = new Vector3(x, y, 0);
        //tile.transform.localRotation = new Quaternion(-90, 0, 0, 0);

        tile_grid[x].Add(tile);
    }
}


