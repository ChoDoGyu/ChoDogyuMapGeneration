using System;
using System.Collections.Generic;
using UnityEngine;

namespace CDG.MapGeneration
{
    /// <summary>
    /// Room 중심 거리로 연결 후보를 만들고 Kruskal 알고리즘으로 모든 Room을 연결하는 최소 신장 트리를 생성합니다.
    /// 동일한 거리에서는 Room ID를 기준으로 정렬하여 항상 동일한 연결 결과를 유지합니다.
    /// </summary>
    internal static class RoomConnectionGraphBuilder
    {
        internal static IReadOnlyList<RoomConnection> CreateSortedCandidates(IReadOnlyList<MapRoom> rooms)
        {
            ValidateRooms(rooms);

            List<RoomConnection> candidates = new List<RoomConnection>();

            for (int i = 0; i < rooms.Count; i++)
            {
                for (int j = i + 1; j < rooms.Count; j++)
                {
                    MapRoom first = rooms[i];
                    MapRoom second = rooms[j];
                    long distanceSquared = CalculateDistanceSquared(first.Center, second.Center);

                    candidates.Add(new RoomConnection(first.Id, second.Id, distanceSquared));
                }
            }

            candidates.Sort(CompareConnections);
            return candidates.AsReadOnly();
        }

        internal static IReadOnlyList<RoomConnection> BuildMinimumSpanningTree(IReadOnlyList<MapRoom> rooms)
        {
            ValidateRooms(rooms);

            if (rooms.Count <= 1)
            {
                return Array.Empty<RoomConnection>();
            }

            IReadOnlyList<RoomConnection> candidates = CreateSortedCandidates(rooms);
            Dictionary<int, int> roomIndices = CreateRoomIndexLookup(rooms);
            DisjointSet disjointSet = new DisjointSet(rooms.Count);
            List<RoomConnection> connections = new List<RoomConnection>(rooms.Count - 1);

            for (int i = 0; i < candidates.Count; i++)
            {
                RoomConnection candidate = candidates[i];
                int fromIndex = roomIndices[candidate.FromRoomId];
                int toIndex = roomIndices[candidate.ToRoomId];

                if (!disjointSet.Union(fromIndex, toIndex))
                {
                    continue;
                }

                connections.Add(candidate);

                if (connections.Count == rooms.Count - 1)
                {
                    break;
                }
            }

            return connections.AsReadOnly();
        }

        private static Dictionary<int, int> CreateRoomIndexLookup(IReadOnlyList<MapRoom> rooms)
        {
            Dictionary<int, int> lookup = new Dictionary<int, int>(rooms.Count);

            for (int i = 0; i < rooms.Count; i++)
            {
                lookup.Add(rooms[i].Id, i);
            }

            return lookup;
        }

        private static long CalculateDistanceSquared(Vector2Int first, Vector2Int second)
        {
            long x = (long)first.x - second.x;
            long y = (long)first.y - second.y;
            return x * x + y * y;
        }

        private static int CompareConnections(RoomConnection first, RoomConnection second)
        {
            int distanceComparison = first.DistanceSquared.CompareTo(second.DistanceSquared);

            if (distanceComparison != 0)
            {
                return distanceComparison;
            }

            int fromComparison = first.FromRoomId.CompareTo(second.FromRoomId);

            if (fromComparison != 0)
            {
                return fromComparison;
            }

            return first.ToRoomId.CompareTo(second.ToRoomId);
        }

        private static void ValidateRooms(IReadOnlyList<MapRoom> rooms)
        {
            if (rooms == null)
            {
                throw new ArgumentNullException(nameof(rooms));
            }

            HashSet<int> roomIds = new HashSet<int>();

            for (int i = 0; i < rooms.Count; i++)
            {
                if (rooms[i] == null)
                {
                    throw new ArgumentException("Room 목록에는 null이 포함될 수 없습니다.", nameof(rooms));
                }

                if (!roomIds.Add(rooms[i].Id))
                {
                    throw new ArgumentException($"중복된 Room ID가 존재합니다. ID: {rooms[i].Id}", nameof(rooms));
                }
            }
        }
    }
}