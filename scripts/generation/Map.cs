using System;
using System.Collections.Generic;
using Godot;

public class Map
{
    //public List<SingleObject> SingleObjects = new();
    public List<SafeLine> SafeLines;    // TODO: Keep until testing

    private const float MapSize = 100f;

    /*public void AddSingleObject(SingleObject obj)
    {
        SingleObjects.Add(obj);
    }*/

    public static void CreateMap(int seed)
    {
        var random = new Random(seed);

        var map = new Map();

        const int safeLineCount = 50;
        List<SafeLine> safeLines = new(safeLineCount);

        // Generate the safe lines
        for (var i = 0; i < safeLineCount; i++)
        {
            var x0 = random.NextSingle() * 200 - 100;
            var y0 = random.NextSingle() * 200 - 100;
            var x1 = random.NextSingle() * 200 - 100;
            var y1 = random.NextSingle() * 200 - 100;
            var line = new SafeLine(new Vector2(x0, y0), new Vector2(x1, y1));

            safeLines.Add(line);
        }

        map.SafeLines = safeLines;
    }

    /// <summary>
    /// Defines the zones guaranteed to not be covered by any objects
    /// </summary>
    public struct SafeLine
    {
        public Vector2 Start;
        public Vector2 End;

        public SafeLine(Vector2 start, Vector2 end)
        {
            Start = start;
            End = end;
        }
    }

    public class SingleObject
    {
        public Vector2 Position;

        public SingleObject(Vector2 position)
        {
            Position = position;
        }
    }

}
