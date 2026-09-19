using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace CDG.MapGeneration.Tests.Runtime
{
    public sealed class ExtraConnectionTests
    {
        [Test]
        public void BuildConnections_ExtraConnection이0이면MST만반환한다()
        {
            IReadOnlyList<MapRoom> rooms = CreateRooms();
            IReadOnlyList<RoomConnection> connections = RoomConnectionGraphBuilder.BuildConnections(rooms, 0);

            Assert.That(connections.Count, Is.EqualTo(rooms.Count - 1));
        }

        [Test]
        public void BuildConnections_요청한추가연결개수를추가한다()
        {
            IReadOnlyList<MapRoom> rooms = CreateRooms();
            IReadOnlyList<RoomConnection> connections = RoomConnectionGraphBuilder.BuildConnections(rooms, 2);

            Assert.That(connections.Count, Is.EqualTo(rooms.Count - 1 + 2));
        }

        [Test]
        public void BuildConnections_동일한Room쌍을중복연결하지않는다()
        {
            IReadOnlyList<MapRoom> rooms = CreateRooms();
            IReadOnlyList<RoomConnection> connections = RoomConnectionGraphBuilder.BuildConnections(rooms, 3);
            HashSet<long> pairs = new HashSet<long>();

            for (int i = 0; i < connections.Count; i++)
            {
                long key = CreateConnectionKey(connections[i].FromRoomId, connections[i].ToRoomId);
                Assert.That(pairs.Add(key), Is.True);
            }
        }

        [Test]
        public void BuildConnections_가능한최대추가연결을요청하면모든Room쌍을연결한다()
        {
            IReadOnlyList<MapRoom> rooms = CreateRooms();
            IReadOnlyList<RoomConnection> connections = RoomConnectionGraphBuilder.BuildConnections(rooms, 3);

            Assert.That(connections.Count, Is.EqualTo(6));
        }

        [Test]
        public void BuildConnections_동일한입력은동일한연결순서를생성한다()
        {
            IReadOnlyList<MapRoom> rooms = CreateRooms();
            IReadOnlyList<RoomConnection> first = RoomConnectionGraphBuilder.BuildConnections(rooms, 2);
            IReadOnlyList<RoomConnection> second = RoomConnectionGraphBuilder.BuildConnections(rooms, 2);

            Assert.That(first.Count, Is.EqualTo(second.Count));

            for (int i = 0; i < first.Count; i++)
            {
                Assert.That(first[i].FromRoomId, Is.EqualTo(second[i].FromRoomId));
                Assert.That(first[i].ToRoomId, Is.EqualTo(second[i].ToRoomId));
                Assert.That(first[i].DistanceSquared, Is.EqualTo(second[i].DistanceSquared));
            }
        }

        [Test]
        public void BuildConnections_ExtraConnectionCount가음수이면예외가발생한다()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => RoomConnectionGraphBuilder.BuildConnections(CreateRooms(), -1));
        }

        [Test]
        public void BuildConnections_가능한추가연결수를초과하면예외가발생한다()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => RoomConnectionGraphBuilder.BuildConnections(CreateRooms(), 4));
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

        private static long CreateConnectionKey(int fromRoomId, int toRoomId)
        {
            return ((long)fromRoomId << 32) | (uint)toRoomId;
        }
    }
}