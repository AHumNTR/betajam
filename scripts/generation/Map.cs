using System;
using System.Collections.Generic;
using Godot;

public class Map
{
    public List<SingleObject> SingleObjects;
    public List<LongObject> LongObjects;
    public List<Objective> Objectives;
    public List<SafeLine> SafeLines;    // TODO: Keep until testing
    public List<RestrictedTriangle> RestrictedTriangles;

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

        var triangulationResult = Triangulator.Triangulate(safePivots);
        List<SafeLine> safeLines = triangulationResult.Lines;
        List<RestrictedTriangle> restrictTriangles = triangulationResult.Triangles;

        for (var i = 0; i < safeLines.Count / 2; i++)
        {
            safeLines.RemoveAt(random.Next() % safeLines.Count);
        }

        map.SafeLines = safeLines;

        // TODO: (Maybe) Generate random rivers, add bridges where they meet safe lines

        // Add Single Objects ???
        const float objectLineDistance = 2f;
        const int objectDimension = 30;
        List<SingleObject> singleObjects = new();
        for (var i = 0; i < objectDimension; i++)
        {
            for (var j = 0; j < objectDimension; j++)
            {
                var x = (random.NextSingle() * 2 - 1 + i * 2 - objectDimension) * MapSize / objectDimension;
                var y = (random.NextSingle() * 2 - 1 + j * 2 - objectDimension) * MapSize / objectDimension;
                var pos = new Vector2(x, y);
                var overlappedOnce = false;
                foreach (var safeLine in safeLines)
                {
                    if (safeLine.Overlaps(pos, objectLineDistance))
                    {
                        overlappedOnce = true;
                    }
                }

                if (!overlappedOnce)
                {
                    singleObjects.Add(new SingleObject(pos, random.Next() % 5));
                }
            }
        }

        // Add Single Objects Along the Safe Lines
        List<Vector2> candidateObjectPositions = new();
        foreach (var safeLine in safeLines)
        {
            var invLength = 1 / safeLine.End.DistanceTo(safeLine.Start);

            for (var t = 0f; t < 1f; t += invLength * 2f)
            {
                var offset = 1f * ((random.Next() % 2) * 2 - 1);
                var diff = safeLine.End - safeLine.Start;
                var spawnPosition = safeLine.Start + diff * t;
                spawnPosition += diff.Orthogonal().Normalized() * offset;
                candidateObjectPositions.Add(spawnPosition);
            }
        }

        const int significantObjectCount = 15;
        for (var i = 0; i < significantObjectCount; i++)
        {
            const int signficantObjectKindAmount = 10;
            for (var j = 0; j < signficantObjectKindAmount; j++)
            {
                var randomIndex = random.Next() % candidateObjectPositions.Count;
                var selectedPosition = candidateObjectPositions[randomIndex];
                candidateObjectPositions.RemoveAt(randomIndex);

                singleObjects.Add(new SingleObject(selectedPosition, 3));
            }
        }

        map.SingleObjects = singleObjects;

        // Add long objects to restrict moving out of safe lines
        List<LongObject> longObjects = new();
        foreach (var triangle in restrictTriangles)
        {
            var center = (triangle.P1 + triangle.P2 + triangle.P3) / 3f;
            var newP1 = triangle.P1 + (triangle.P1 - center).Normalized();
            var newP2 = triangle.P2 + (triangle.P2 - center).Normalized();
            var newP3 = triangle.P3 + (triangle.P3 - center).Normalized();

            longObjects.Add(new LongObject(newP1, newP2, random.Next() % 3));
            longObjects.Add(new LongObject(newP1, newP3, random.Next() % 3));
            longObjects.Add(new LongObject(newP2, newP3, random.Next() % 3));
        }

        map.RestrictedTriangles = restrictTriangles;

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

        public bool Overlaps(Vector2 point, float thickness)
        {
            var diff = End - Start;
            float projectionValue = (point - Start).Project(diff).Dot(diff);
            var diffLenSqr = diff.LengthSquared();
            if (projectionValue < 0 || projectionValue > diffLenSqr) return false;
            var numerator = Mathf.Pow(diff.Y * point.X - diff.X * point.Y + End.X * Start.Y - Start.X * End.Y, 2);
            return (thickness * thickness > numerator / diffLenSqr);
        }
    }

    public class SingleObject
    {
        public Vector2 Position;
        public int ObjectType;

        public SingleObject(Vector2 position, int type)
        {
            Position = position;
            ObjectType = type;
        }
    }

    public class LongObject
    {
        public Vector2 StartPosition;
        public Vector2 EndPosition;
        public int ObjectType;

        public LongObject(Vector2 startPosition, Vector2 endPosition, int type)
        {
            StartPosition = startPosition;
            EndPosition = endPosition;
            ObjectType = type;
        }
    }

    public class Objective
    {
        public Vector2 Position;
        public int ObjectType;

        public Objective(Vector2 position)
        {
            Position = position;
        }
    }

    public class RestrictedTriangle
    {
        public Vector2 P1;
        public Vector2 P2;
        public Vector2 P3;

        public RestrictedTriangle(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            P1 = p1;
            P2 = p2;
            P3 = p3;
        }
    }

    private class Triangulator
    {
        public static (List<SafeLine> Lines, List<RestrictedTriangle> Triangles) Triangulate(List<Vector2> safePivots)
        {
            List<SafeLine> lines = new List<SafeLine>();
            List<RestrictedTriangle> triangles = new List<RestrictedTriangle>();
            int n = safePivots.Count;

            if (n < 3) return (lines, triangles);

            /* HashSet<(int, int)> uniqueEdges = new HashSet<(int, int)>();

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
            } */

            // Triangle calculation
            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    for (int k = j + 1; k < n; k++)
                    {
                        Vector2 p1 = safePivots[i];
                        Vector2 p2 = safePivots[j];
                        Vector2 p3 = safePivots[k];

                        GetCircumcircle(p1, p2, p3, out Vector2 center, out float radiusSq);

                        bool isDelaunay = true;
                        for (int m = 0; m < n; m++)
                        {
                            if (m == i || m == j || m == k) continue;

                            // Check if any other point is inside the circumcircle
                            if (center.DistanceSquaredTo(safePivots[m]) < radiusSq - 0.001f)
                            {
                                isDelaunay = false;
                                break;
                            }
                        }

                        if (isDelaunay)
                        {
                            triangles.Add(new RestrictedTriangle(p1, p2, p3));
                        }
                    }
                }
            }

            // Lines calculation
            HashSet<(Vector2, Vector2)> uniqueEdges = new HashSet<(Vector2, Vector2)>();

            foreach (var tri in triangles)
            {
                AddUniqueEdge(uniqueEdges, lines, tri.P1, tri.P2);
                AddUniqueEdge(uniqueEdges, lines, tri.P2, tri.P3);
                AddUniqueEdge(uniqueEdges, lines, tri.P3, tri.P1);
            }

            return (lines, triangles);
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

        private static void AddUniqueEdge(HashSet<(Vector2, Vector2)> set, List<SafeLine> list, Vector2 a, Vector2 b)
        {
            // Order points to ensure (a, b) is the same as (b, a)
            var edge = a.X < b.X || (a.X == b.X && a.Y < b.Y) ? (a, b) : (b, a);
            if (set.Add(edge))
            {
                list.Add(new SafeLine(a, b));
            }
        }
    }

}
