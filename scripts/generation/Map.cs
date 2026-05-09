using System;
using System.Collections.Generic;
using Godot;

public class Map
{
    //public List<SingleObject> SingleObjects = new();
    public List<Objective> Objectives;
    public List<SafeLine> SafeLines;    // TODO: Keep until testing

    private const float MapSize = 100f;

    /*public void AddSingleObject(SingleObject obj)
    {
        SingleObjects.Add(obj);
    }*/

    public static Map CreateMap(int seed, float newMapSize = 100f)
    {
        float MapSize = newMapSize; // TODO: REMOVE THIS WHEN DECIDED

        var random = new Random(seed);

        var map = new Map();

        const int safePivotDimension = 10;
        List<Vector2> safePivots = new(safePivotDimension * safePivotDimension);

        // Generate safe pivots and safe lines to ensure objectives are reachable
        for (var i = 0; i < safePivotDimension; i++)
        {
            for (var j = 0; j < safePivotDimension; j++)
            {
                var x = (random.NextSingle() * 2 - 1 + i * 2 - safePivotDimension) * MapSize / safePivotDimension;
                var y = (random.NextSingle() * 2 - 1 + j * 2 - safePivotDimension) * MapSize / safePivotDimension;
                safePivots.Add(new Vector2(x, y));
            }
        }

        List<SafeLine> safeLines = Triangulator.Triangulate(safePivots);

        for (var i = 0; i < safeLines.Count / 2; i++)
        {
            safeLines.RemoveAt(random.Next() % safeLines.Count);
        }

        map.SafeLines = safeLines;

        // TODO: Generate sparse rare objects at some radius away from safe line
        // TODO: Generate more dense objects at some further radius away from safe line
        // TODO: (Maybe) Generate random rivers, add bridges where they meet safe lines

        // Add Objectives
        const int objectiveCount = 5;
        List<Objective> objectives = new(objectiveCount);
        for (var i = 0; i < objectiveCount; i++)
        {
            var selectedSafeLine = safeLines[random.Next() % safeLines.Count];
            var selectedPosition = selectedSafeLine.Start + (selectedSafeLine.End - selectedSafeLine.Start) * random.NextSingle();
            objectives.Add(new Objective(selectedPosition));
        }
        map.Objectives = objectives;
        
        return map;
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

    public class Objective
    {
        public Vector2 Position;

        public Objective(Vector2 position)
        {
            Position = position;
        }
    }

    private class Triangulator
    {
        public static List<SafeLine> Triangulate(List<Vector2> safePivots)
        {
            List<SafeLine> safeLines = new List<SafeLine>();
            int n = safePivots.Count;

            if (n < 3) return safeLines;

            HashSet<(int, int)> uniqueEdges = new HashSet<(int, int)>();

            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    for (int k = j + 1; k < n; k++)
                    {
                        Vector2 p1 = safePivots[i];
                        Vector2 p2 = safePivots[j];
                        Vector2 p3 = safePivots[k];

                        Vector2 center;
                        float radiusSq;
                        GetCircumcircle(p1, p2, p3, out center, out radiusSq);

                        bool isDelaunay = true;
                        for (int m = 0; m < n; m++)
                        {
                            if (m == i || m == j || m == k) continue;

                            if (center.DistanceSquaredTo(safePivots[m]) < radiusSq - 0.001f)
                            {
                                isDelaunay = false;
                                break;
                            }
                        }

                        if (isDelaunay)
                        {
                            AddEdge(uniqueEdges, safeLines, i, j, p1, p2);
                            AddEdge(uniqueEdges, safeLines, j, k, p2, p3);
                            AddEdge(uniqueEdges, safeLines, k, i, p3, p1);
                        }
                    }
                }
            }

            return safeLines;
        }

        private static void GetCircumcircle(Vector2 p1, Vector2 p2, Vector2 p3, out Vector2 center, out float radiusSq)
        {
            float x1 = p1.X, y1 = p1.Y;
            float x2 = p2.X, y2 = p2.Y;
            float x3 = p3.X, y3 = p3.Y;

            float D = 2 * (x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2));

            float centerX = ((x1 * x1 + y1 * y1) * (y2 - y3) + (x2 * x2 + y2 * y2) * (y3 - y1) + (x3 * x3 + y3 * y3) * (y1 - y2)) / D;
            float centerY = ((x1 * x1 + y1 * y1) * (x3 - x2) + (x2 * x2 + y2 * y2) * (x1 - x3) + (x3 * x3 + y3 * y3) * (x2 - x1)) / D;

            center = new Vector2(centerX, centerY);
            radiusSq = p1.DistanceSquaredTo(center);
        }

        private static void AddEdge(HashSet<(int, int)> edges, List<SafeLine> lines, int i1, int i2, Vector2 v1, Vector2 v2)
        {
            int min = Math.Min(i1, i2);
            int max = Math.Max(i1, i2);
            if (edges.Add((min, max)))
            {
                lines.Add(new SafeLine(v1, v2));
            }
        }
    }

}
