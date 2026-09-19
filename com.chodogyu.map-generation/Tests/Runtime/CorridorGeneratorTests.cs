using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace CDG.MapGeneration.Tests.Runtime
{
    public sealed class CorridorGeneratorTests
    {
        [Test]
        public void Generate_Connection마다하나의Corridor를생성한다()
        {
            IReadOnlyList<MapRoom> rooms = CreateRooms();
            IReadOnlyList<RoomConnection> connections = RoomConnectionGraphBuilder.BuildConnections(rooms, 1);

            IReadOnlyList<MapCorridor> corridors = CorridorGenerator.Generate(rooms, connections, new DeterministicRandom(12345));

            Assert.That(corridors.Count, Is.EqualTo(connections.Count));
        }

        [Test]
        public void Generate_Corridor는연결된두Room중심에서시작하고끝난다()
        {
            IReadOnlyList<MapRoom> rooms = CreateRooms();
            RoomConnection[] connections = { new RoomConnection(0, 1, 0) };

            IReadOnlyList<MapCorridor> corridors = CorridorGenerator.Generate(rooms, connections, new DeterministicRandom(12345));

            Assert.That(corridors[0].Cells[0], Is.EqualTo(rooms[0].Center));
            Assert.That(corridors[0].Cells[corridors[0].Cells.Count - 1], Is.EqualTo(rooms[1].Center));
        }

        [Test]
        public void Generate_CorridorCell은상하좌우로만연속된다()
        {
            IReadOnlyList<MapRoom> rooms = CreateRooms();
            RoomConnection[] connections = { new RoomConnection(0, 3, 0) };

            IReadOnlyList<MapCorridor> corridors = CorridorGenerator.Generate(rooms, connections, new DeterministicRandom(12345));
            IReadOnlyList<Vector2Int> cells = corridors[0].Cells;

            for (int i = 1; i < cells.Count; i++)
            {
                int distance = Math.Abs(cells[i].x - cells[i - 1].x) + Math.Abs(cells[i].y - cells[i - 1].y);
                Assert.That(distance, Is.EqualTo(1));
            }
        }

        [Test]
        public void Generate_Corridor길이는두중심의ManhattanDistance와일치한다()
        {
            IReadOnlyList<MapRoom> rooms = CreateRooms();
            RoomConnection[] connections = { new RoomConnection(0, 3, 0) };

            IReadOnlyList<MapCorridor> corridors = CorridorGenerator.Generate(rooms, connections, new DeterministicRandom(12345));
            Vector2Int start = rooms[0].Center;
            Vector2Int end = rooms[3].Center;
            int expectedCellCount = Math.Abs(end.x - start.x) + Math.Abs(end.y - start.y) + 1;

            Assert.That(corridors[0].Cells.Count, Is.EqualTo(expectedCellCount));
        }

        [Test]
        public void Generate_동일한Seed는동일한Corridor를생성한다()
        {
            IReadOnlyList<MapRoom> rooms = CreateRooms();
            IReadOnlyList<RoomConnection> connections = RoomConnectionGraphBuilder.BuildConnections(rooms, 2);
            IReadOnlyList<MapCorridor> first = CorridorGenerator.Generate(rooms, connections, new DeterministicRandom(777));
            IReadOnlyList<MapCorridor> second = CorridorGenerator.Generate(rooms, connections, new DeterministicRandom(777));

            Assert.That(first.Count, Is.EqualTo(second.Count));

            for (int i = 0; i < first.Count; i++)
            {
                Assert.That(first[i].FromRoomId, Is.EqualTo(second[i].FromRoomId));
                Assert.That(first[i].ToRoomId, Is.EqualTo(second[i].ToRoomId));
                Assert.That(first[i].Cells, Is.EqualTo(second[i].Cells));
            }
        }

        [Test]
        public void Generate_존재하지않는Room을참조하면예외가발생한다()
        {
            IReadOnlyList<MapRoom> rooms = CreateRooms();
            RoomConnection[] connections = { new RoomConnection(0, 99, 0) };

            Assert.Throws<ArgumentException>(() => CorridorGenerator.Generate(rooms, connections, new DeterministicRandom(12345)));
        }

        [Test]
        public void Generate_Random이null이면예외가발생한다()
        {
            Assert.Throws<ArgumentNullException>(() => CorridorGenerator.Generate(CreateRooms(), Array.Empty<RoomConnection>(), null));
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