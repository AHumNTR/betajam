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

    public static void CreateMap(int seed, float newMapSize = 100f)
    {
        float MapSize = newMapSize; // TODO: REMOVE THIS WHEN DECIDED

        var random = new Random(seed);

        var map = new Map();

        const int safePivotCount = 50;
        List<Vector2> safePivots = new(safePivotCount);

        // Generate safe pivots
        for (var i = 0; i < safePivotCount; i++)
        {
            var x = (random.NextSingle() * 2 - 1) * MapSize;
            var y = (random.NextSingle() * 2 - 1) * MapSize;
            safePivots[i] = new Vector2(x, y);
        }

        List<SafeLine> safeLines = new();
        int[] safePivotConnectionCounts = new int[safePivotCount];

        // Generate the safe lines between safe pivots
        for (var i = 0; i < safePivotCount; i++)
        {
            const int maxPivotConnections = 2;
            List<int> candidatePivotIndices = new();
            for (var pivotIndex = 0; pivotIndex < safePivotCount; pivotIndex++)
            {
                if (safePivotConnectionCounts[pivotIndex] < maxPivotConnections)
                {
                    candidatePivotIndices.Add(pivotIndex);
                }
            }

            for (var j = safePivotConnectionCounts[i]; j < maxPivotConnections; j++)
            {
                if (candidatePivotIndices.Count > 0)
                {
                    var selectedPivotIndex = candidatePivotIndices[0];
                    var bestDistanceSqr = float.MaxValue;
                    foreach (var candidatePivotIndex in candidatePivotIndices)
                    {
                        var newDistanceSqr = safePivots[candidatePivotIndex].DistanceSquaredTo(safePivots[i]);
                        if (newDistanceSqr < bestDistanceSqr)
                        {
                            bestDistanceSqr = newDistanceSqr;
                            selectedPivotIndex = candidatePivotIndex;
                        }
                    }
                    candidatePivotIndices.Remove(selectedPivotIndex);
                    safePivotConnectionCounts[selectedPivotIndex]++;

                    var pivot0 = safePivots[i];
                    var pivot1 = safePivots[selectedPivotIndex];
                    var line = new SafeLine(pivot0, pivot1);
                    safeLines.Add(line);
                }
            }

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
