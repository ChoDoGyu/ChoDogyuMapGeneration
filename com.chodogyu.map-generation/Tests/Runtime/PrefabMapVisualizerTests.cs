using System;
using CDG.Core.Results;
using NUnit.Framework;
using UnityEngine;

namespace CDG.MapGeneration.Tests.Runtime
{
    public sealed class PrefabMapVisualizerTests
    {
        private const string GeneratedRootName = "__CDG_GeneratedMap";

        private GameObject host;
        private GameObject floorPrefab;
        private GameObject wallPrefab;
        private PrefabMapTheme theme;
        private PrefabMapVisualizer visualizer;

        [SetUp]
        public void SetUp()
        {
            host = new GameObject("VisualizerHost");
            floorPrefab = new GameObject("FloorPrefab");
            wallPrefab = new GameObject("WallPrefab");

            theme = ScriptableObject.CreateInstance<PrefabMapTheme>();
            theme.FloorPrefab = floorPrefab;
            theme.WallPrefab = wallPrefab;

            visualizer = host.AddComponent<PrefabMapVisualizer>();
            visualizer.Theme = theme;
            visualizer.CellSize = 1f;
            visualizer.ProjectionPlane = MapProjectionPlane.XZ;
        }

        [TearDown]
        public void TearDown()
        {
            if (host != null)
            {
                UnityEngine.Object.DestroyImmediate(host);
            }

            if (floorPrefab != null)
            {
                UnityEngine.Object.DestroyImmediate(floorPrefab);
            }

            if (wallPrefab != null)
            {
                UnityEngine.Object.DestroyImmediate(wallPrefab);
            }

            if (theme != null)
            {
                UnityEngine.Object.DestroyImmediate(theme);
            }
        }

        [Test]
        public void Visualize_유효한Map이면Floor와Wall만생성한다()
        {
            Result result = visualizer.Visualize(CreateValidMapData());

            Assert.That(result.IsSuccess, Is.True);

            Transform root = host.transform.Find(GeneratedRootName);

            Assert.That(root, Is.Not.Null);
            Assert.That(root.childCount, Is.EqualTo(2));
            Assert.That(FindGeneratedChild(root, "FloorPrefab"), Is.Not.Null);
            Assert.That(FindGeneratedChild(root, "WallPrefab"), Is.Not.Null);
        }

        [Test]
        public void Visualize_다시호출하면기존생성결과를교체한다()
        {
            Result first = visualizer.Visualize(CreateValidMapData());
            Result second = visualizer.Visualize(CreateValidMapData());

            Assert.That(first.IsSuccess, Is.True);
            Assert.That(second.IsSuccess, Is.True);
            Assert.That(host.transform.childCount, Is.EqualTo(1));

            Transform root = host.transform.Find(GeneratedRootName);

            Assert.That(root, Is.Not.Null);
            Assert.That(root.childCount, Is.EqualTo(2));
        }

        [Test]
        public void Visualize_XY투영이면Grid좌표를XY평면에배치한다()
        {
            visualizer.ProjectionPlane = MapProjectionPlane.XY;
            visualizer.CellSize = 2f;

            Result result = visualizer.Visualize(CreateValidMapData());

            Assert.That(result.IsSuccess, Is.True);

            Transform root = host.transform.Find(GeneratedRootName);
            Transform floor = FindGeneratedChild(root, "FloorPrefab");

            Assert.That(floor.localPosition, Is.EqualTo(new Vector3(2f, 2f, 0f)));
        }

        [Test]
        public void Visualize_XZ투영이면Grid좌표를XZ평면에배치한다()
        {
            visualizer.ProjectionPlane = MapProjectionPlane.XZ;
            visualizer.CellSize = 2f;

            Result result = visualizer.Visualize(CreateValidMapData());

            Assert.That(result.IsSuccess, Is.True);

            Transform root = host.transform.Find(GeneratedRootName);
            Transform floor = FindGeneratedChild(root, "FloorPrefab");

            Assert.That(floor.localPosition, Is.EqualTo(new Vector3(2f, 0f, 2f)));
        }

        [Test]
        public void Clear_생성된MapRoot를제거한다()
        {
            Result result = visualizer.Visualize(CreateValidMapData());

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(host.transform.Find(GeneratedRootName), Is.Not.Null);

            visualizer.Clear();

            Assert.That(host.transform.Find(GeneratedRootName), Is.Null);
        }

        [Test]
        public void Visualize_MapData가null이면InvalidData로실패한다()
        {
            Result result = visualizer.Visualize(null);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(MapGenerationErrorCodes.VisualizerInvalidData));
        }

        [Test]
        public void Visualize_Validation에실패한Map이면InvalidData로실패한다()
        {
            Result result = visualizer.Visualize(CreateInvalidMapData());

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(MapGenerationErrorCodes.VisualizerInvalidData));
            Assert.That(host.transform.Find(GeneratedRootName), Is.Null);
        }

        [Test]
        public void Visualize_Theme이없으면ThemeMissing으로실패한다()
        {
            visualizer.Theme = null;

            Result result = visualizer.Visualize(CreateValidMapData());

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(MapGenerationErrorCodes.VisualizerThemeMissing));
        }

        [Test]
        public void Visualize_ThemePrefab이부족하면InvalidTheme으로실패한다()
        {
            theme.WallPrefab = null;

            Result result = visualizer.Visualize(CreateValidMapData());

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(MapGenerationErrorCodes.VisualizerInvalidTheme));
        }

        [Test]
        public void Visualize_CellSize가0이하이면InvalidTheme으로실패한다()
        {
            visualizer.CellSize = 0f;

            Result result = visualizer.Visualize(CreateValidMapData());

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(MapGenerationErrorCodes.VisualizerInvalidTheme));
        }

        private static MapData CreateValidMapData()
        {
            MapCellType[] cells =
            {
                MapCellType.Wall, MapCellType.Empty,
                MapCellType.Empty, MapCellType.Floor
            };

            return new MapData(2, 2, 12345, cells, Array.Empty<MapRoom>(), Array.Empty<MapCorridor>());
        }

        private static MapData CreateInvalidMapData()
        {
            MapCellType[] cells =
            {
                MapCellType.Floor, MapCellType.Empty,
                MapCellType.Empty, MapCellType.Floor
            };

            return new MapData(2, 2, 12345, cells, Array.Empty<MapRoom>(), Array.Empty<MapCorridor>());
        }

        private static Transform FindGeneratedChild(Transform root, string sourceName)
        {
            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);

                if (child.name.StartsWith(sourceName))
                {
                    return child;
                }
            }

            return null;
        }
    }
}