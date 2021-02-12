using UnityEngine;
using System;
using System.Text.RegularExpressions;

public class Regionn
{
    private string name;
    private Vector2Int? coordinates;
    private Texture2D map;

    public Regionn(string name)
    {
        if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
        this.name = name;
    }

    public Regionn(Vector2Int coordinates)
    {
        this.coordinates = coordinates;
    }

    public void GetCoords(Action<string> onFail, Action<Vector2Int> onSuccess)
    {
        if (coordinates.HasValue) onSuccess(coordinates.Value);
        else
        {
            WWWFormPlus form = new WWWFormPlus();
            form.Request(
                $"https://cap.secondlife.com/cap/0/d661249b-2b5a-4436-966a-3d3b8d7a574f?var=coords&sim_name={name}",
                onFail,
                x => { coordinates = ParseCordsResult(x); onSuccess(coordinates.Value); }
            );
        }
    }

    private Vector2Int ParseCordsResult(string result)
    {
        Match match = Regex.Match(result, "{(['\"])x\\1\\s*:\\s*(\\d+),\\s*\\1y\\1\\s*:\\s*(\\d+)\\s*}");
        if (match.Groups.Count == 4)
        {
            return new Vector2Int(int.Parse(match.Groups[2].Value), int.Parse(match.Groups[3].Value));
        }
        else
        {
            Debug.LogError("Couldn't parse location result: " + result);
            return Vector2Int.zero;
        }
    }

    public void GetName(Action<string> onFail, Action<string> onSuccess)
    {
        if (name != null) onSuccess(name);
        else
        {
            WWWFormPlus form = new WWWFormPlus();
            form.Request(
                $"https://cap.secondlife.com/cap/0/b713fe80-283b-4585-af4d-a3b7d9a32492?var=region&grid_x={coordinates.Value.x}&grid_y={coordinates.Value.y}",
                onFail,
                x => { name = ParseNameResult(x); onSuccess(name); }
            );
        }
    }

    private string ParseNameResult(string result)
    {
        Debug.Log("TODO PARSE RESULT: " + result);
        return result;
    }

    public void GetMap(Action<string> onFail, Action<Texture2D> onSuccess)
    {
        if (map != null) onSuccess(map);
        else
        {
            GetCoords(onFail, coords =>
            {
                WWWFormPlus.RequestTexture(
                    $"https://map.secondlife.com/map-1-{coords.x}-{coords.y}-objects.jpg",
                    onFail,
                    tex => { map = tex; onSuccess(tex); }
                );
            });
        }
    }
}