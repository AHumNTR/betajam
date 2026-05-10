using System;
using System.Collections.Generic;
using Godot;
using Microsoft.Win32.SafeHandles;

public class Map
{
    public List<Vector2> GrassPositions;
    public List<SingleObject> SingleObjects;
    public List<LongObject> LongObjects;
    public List<Objective> Objectives;
    public List<SafeLine> SafeLines;    // TODO: Keep until testing
    public List<RestrictedTriangle> RestrictedTriangles;
    public Vector2 CircleOfMushroomsPosition;

    public const float MAP_SIZE = 250f;

    public static Map CreateMap(int seed)
    {
        var random = new Random(seed);

        var map = new Map();

        const int safePivotDimension = 15;
        List<Vector2> safePivots = new(safePivotDimension * safePivotDimension);

        // Generate safe pivots and safe lines to ensure objectives are reachable
        for (var i = 0; i < safePivotDimension; i++)
        {
            for (var j = 0; j < safePivotDimension; j++)
            {
                var x = (random.NextSingle() + i) * MAP_SIZE / safePivotDimension * .95f;   // fixed multiplier is for giving
                var y = (random.NextSingle() + j) * MAP_SIZE / safePivotDimension * .95f;   // space to other objects
                safePivots.Add(new Vector2(x, y));
            }
        }

        // Add mirrored pivots on the edges to ensure
        for (var i = 0; i < 4; i++)
        {
            var x = random.NextSingle() * MAP_SIZE;
            safePivots.Add(new Vector2(x, MAP_SIZE));
            safePivots.Add(new Vector2(x, 0));
        }
        for (var j = 0; j < 4; j++)
        {
            var y = random.NextSingle() * MAP_SIZE;
            safePivots.Add(new Vector2(0, y));
            safePivots.Add(new Vector2(MAP_SIZE, y));
        }

        var triangulationResult = Triangulator.Triangulate(safePivots);
        List<SafeLine> safeLines = triangulationResult.Lines;
        List<RestrictedTriangle> restrictTriangles = triangulationResult.Triangles;

        for (var i = 0; i < safeLines.Count / 2; i++)
        {
            safeLines.RemoveAt(random.Next() % safeLines.Count);
        }

        // Remove safe lines on the edge
        List<int> lineIndicesToDelete = new();
        for (var i = 0; i < safeLines.Count; i++)
        {
            var line = safeLines[i];
            if (
                (Mathf.IsZeroApprox(line.Start.X) && Mathf.IsZeroApprox(line.End.X)) ||
                (Mathf.IsZeroApprox(line.Start.Y) && Mathf.IsZeroApprox(line.End.Y)) ||
                (Mathf.IsEqualApprox(line.Start.X, MAP_SIZE) && Mathf.IsEqualApprox(line.End.X, MAP_SIZE)) ||
                (Mathf.IsEqualApprox(line.Start.Y, MAP_SIZE) && Mathf.IsEqualApprox(line.End.Y, MAP_SIZE))
                )
            {
                lineIndicesToDelete.Add(i);
            }
        }
        for (var i = lineIndicesToDelete.Count - 1; i >= 0; i--)
        {
            safeLines.RemoveAt(lineIndicesToDelete[i]);
        }

        map.SafeLines = safeLines;

        // Add Grass
        const float objectLineDistance = 2f;
        const int grassFrequency = 20;
        List<Vector2> grassPositions = new();
        for (var i = 0; i < grassFrequency; i++)
        {
            for (var j = 0; j < grassFrequency; j++)
            {
                var x = (random.NextSingle() + i) * MAP_SIZE / grassFrequency;
                var y = (random.NextSingle() + j) * MAP_SIZE / grassFrequency;
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
                    grassPositions.Add(new Vector2(x, y));
                }
            }
        }
        map.GrassPositions = grassPositions;

        // Add Single Objects
        const int objectFrequency = 60;
        List<SingleObject> singleObjects = new();
        for (var i = 0; i < objectFrequency; i++)
        {
            for (var j = 0; j < objectFrequency; j++)
            {
                var x = (random.NextSingle() + i) * MAP_SIZE / objectFrequency;
                var y = (random.NextSingle() + j) * MAP_SIZE / objectFrequency;
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
                    var selectedObjectType = _denseObjects[random.Next() % _denseObjects.Length];
                    singleObjects.Add(new SingleObject(pos, selectedObjectType.ObjectID));
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

        for (var i = 0; i < _sparseObjects.Length; i++)
        {
            var sparseObjectType = _sparseObjects[i];
            for (var j = 0; j < sparseObjectType.count; j++)
            {
                var randomIndex = random.Next() % candidateObjectPositions.Count;
                var selectedPosition = candidateObjectPositions[randomIndex];
                candidateObjectPositions.RemoveAt(randomIndex);

                singleObjects.Add(new SingleObject(selectedPosition, sparseObjectType.ObjectID));
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

            var selectedObjectType0 = _longObjects[random.Next() % _longObjects.Length];
            var selectedObjectType1 = _longObjects[random.Next() % _longObjects.Length];
            var selectedObjectType2 = _longObjects[random.Next() % _longObjects.Length];
            longObjects.Add(new LongObject(newP1, newP2, selectedObjectType0.ObjectID));
            longObjects.Add(new LongObject(newP1, newP3, selectedObjectType1.ObjectID));
            longObjects.Add(new LongObject(newP2, newP3, selectedObjectType2.ObjectID));
        }

        map.RestrictedTriangles = restrictTriangles;

        // Add Objectives
        List<Objective> objectives = new();
        foreach (var line in safeLines)
        {
            var randomPosition = line.Start.Lerp(line.End, random.NextSingle());
            var isOverlapped = false;
            foreach (var otherObjective in objectives)
            {
                if (otherObjective.Position.DistanceSquaredTo(randomPosition) < 9f)
                {
                    isOverlapped = true;
                }
            }

            if (!isOverlapped)
            {
                objectives.Add(new Objective(randomPosition,random.Next()%2));
            }
        }

        var maxHarmlessObjectiveCount = Mathf.Min(100, objectives.Count);
        var addedHarmlessObjectiveCount = 0;
        while (addedHarmlessObjectiveCount < maxHarmlessObjectiveCount)
        {
            var randomObjective = objectives[random.Next() % objectives.Count];
            if (!randomObjective.Harmless)
            {
                randomObjective.Harmless = true;
                addedHarmlessObjectiveCount++;
            }
        }
        map.Objectives = objectives;

        map.CircleOfMushroomsPosition = new Vector2(random.NextSingle() * 100f, random.NextSingle() * 100f);

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

    private record DenseObjectDefinition(int ObjectID);
    private record SparseObjectDefinition(int ObjectID, int count);
    private record LongObjectDefinition(int ObjectID);

    private static readonly DenseObjectDefinition[] _denseObjects = [
        new(0), // Tree1
        new(1), // TreePine
        new(2), // Rock
        ];
    private static readonly SparseObjectDefinition[] _sparseObjects = [
        new(3, 12), // Tree2
        new(4, 12), // Tree3
        new(5, 12), // TreeRed
        new(6, 12), // TreeYellow
        //new(7, 25), // Mushroom
        //new(8, 25), // Mushroom2
        new(9,  20), // Crystal1
        new(10, 20), // Crystal2
        new(11, 20), // Crystal3
        new(12, 12), // Well
        ];
    private static readonly LongObjectDefinition[] _longObjects = [
        new(0),
        ];

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
        public bool Harmless;

        public Objective(Vector2 position,int objectType)
        {
            Position = position;
            Harmless = false;
            ObjectType=objectType;
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
