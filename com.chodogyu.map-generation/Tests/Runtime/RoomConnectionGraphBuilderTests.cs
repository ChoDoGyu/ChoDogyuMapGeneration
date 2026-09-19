using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace CDG.MapGeneration.Tests.Runtime
{
    public sealed class RoomConnectionGraphBuilderTests
    {
        [Test]
        public void CreateSortedCandidates_모든Room쌍의연결후보를생성한다()
        {
            IReadOnlyList<MapRoom> rooms = CreateRooms();

            IReadOnlyList<RoomConnection> candidates = RoomConnectionGraphBuilder.CreateSortedCandidates(rooms);

            Assert.That(candidates.Count, Is.EqualTo(6));
        }

        [Test]
        public void CreateSortedCandidates_거리와RoomId순서로결정적으로정렬한다()
        {
            MapRoom[] rooms =
            {
                new MapRoom(0, new RectInt(0, 0, 2, 2)),
                new MapRoom(1, new RectInt(4, 0, 2, 2)),
                new MapRoom(2, new RectInt(0, 4, 2, 2))
            };

            IReadOnlyList<RoomConnection> candidates = RoomConnectionGraphBuilder.CreateSortedCandidates(rooms);

            Assert.That(candidates[0].FromRoomId, Is.EqualTo(0));
            Assert.That(candidates[0].ToRoomId, Is.EqualTo(1));
            Assert.That(candidates[1].FromRoomId, Is.EqualTo(0));
            Assert.That(candidates[1].ToRoomId, Is.EqualTo(2));
            Assert.That(candidates[2].FromRoomId, Is.EqualTo(1));
            Assert.That(candidates[2].ToRoomId, Is.EqualTo(2));
        }

        [Test]
        public void BuildMinimumSpanningTree_RoomCount보다하나적은연결을생성한다()
        {
            IReadOnlyList<MapRoom> rooms = CreateRooms();

            IReadOnlyList<RoomConnection> connections = RoomConnectionGraphBuilder.BuildMinimumSpanningTree(rooms);

            Assert.That(connections.Count, Is.EqualTo(rooms.Count - 1));
        }

        [Test]
        public void BuildMinimumSpanningTree_모든Room을하나의집합으로연결한다()
        {
            IReadOnlyList<MapRoom> rooms = CreateRooms();
            IReadOnlyList<RoomConnection> connections = RoomConnectionGraphBuilder.BuildMinimumSpanningTree(rooms);
            Dictionary<int, int> indices = new Dictionary<int, int>();

            for (int i = 0; i < rooms.Count; i++)
            {
                indices.Add(rooms[i].Id, i);
            }

            DisjointSet disjointSet = new DisjointSet(rooms.Count);

            for (int i = 0; i < connections.Count; i++)
            {
                disjointSet.Union(indices[connections[i].FromRoomId], indices[connections[i].ToRoomId]);
            }

            int root = disjointSet.Find(0);

            for (int i = 1; i < rooms.Count; i++)
            {
                Assert.That(disjointSet.Find(i), Is.EqualTo(root));
            }
        }

        [Test]
        public void BuildMinimumSpanningTree_Room이하나이면연결을생성하지않는다()
        {
            MapRoom[] rooms =
            {
                new MapRoom(0, new RectInt(5, 5, 5, 5))
            };

            IReadOnlyList<RoomConnection> connections = RoomConnectionGraphBuilder.BuildMinimumSpanningTree(rooms);

            Assert.That(connections, Is.Empty);
        }

        [Test]
        public void BuildMinimumSpanningTree_동일한Room목록은동일한연결을생성한다()
        {
            IReadOnlyList<MapRoom> rooms = CreateRooms();
            IReadOnlyList<RoomConnection> first = RoomConnectionGraphBuilder.BuildMinimumSpanningTree(rooms);
            IReadOnlyList<RoomConnection> second = RoomConnectionGraphBuilder.BuildMinimumSpanningTree(rooms);

            Assert.That(first.Count, Is.EqualTo(second.Count));

            for (int i = 0; i < first.Count; i++)
            {
                Assert.That(first[i].FromRoomId, Is.EqualTo(second[i].FromRoomId));
                Assert.That(first[i].ToRoomId, Is.EqualTo(second[i].ToRoomId));
                Assert.That(first[i].DistanceSquared, Is.EqualTo(second[i].DistanceSquared));
            }
        }

        [Test]
        public void BuildMinimumSpanningTree_중복RoomId가있으면예외가발생한다()
        {
            MapRoom[] rooms =
            {
                new MapRoom(0, new RectInt(0, 0, 5, 5)),
                new MapRoom(0, new RectInt(10, 10, 5, 5))
            };

            Assert.Throws<ArgumentException>(() => RoomConnectionGraphBuilder.BuildMinimumSpanningTree(rooms));
        }

        private static IReadOnlyList<MapRoom> CreateRooms()
        {
            return new[]
            {
                new MapRoom(0, new RectInt(2, 2, 5, 5)),
                new MapRoom(1, new RectInt(20, 3, 5, 5)),
                new MapRoom(2, new RectInt(4, 20, 5, 5)),
                new MapRoom(3, new RectInt(22, 22, 5, 5))
            };
        }
    }
}