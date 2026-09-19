using System;
using CDG.Core.Results;
using CDG.MapGeneration.Editor;
using NUnit.Framework;
using UnityEngine;

namespace CDG.MapGeneration.Tests.Editor
{
    public sealed class MapSceneApplyUtilityTests
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
        public void Apply_유효한Visualizer와Map이면Scene에생성한다()
        {
            Result result = MapSceneApplyUtility.Apply(visualizer, CreateValidMapData());

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(host.transform.Find(GeneratedRootName), Is.Not.Null);
        }

        [Test]
        public void Apply_Visualizer실패를그대로반환한다()
        {
            visualizer.Theme = null;

            Result result = MapSceneApplyUtility.Apply(visualizer, CreateValidMapData());

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(MapGenerationErrorCodes.VisualizerThemeMissing));
        }

        [Test]
        public void Clear_생성된SceneVisualization을제거한다()
        {
            Result result = MapSceneApplyUtility.Apply(visualizer, CreateValidMapData());

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(host.transform.Find(GeneratedRootName), Is.Not.Null);

            MapSceneApplyUtility.Clear(visualizer);

            Assert.That(host.transform.Find(GeneratedRootName), Is.Null);
        }

        [Test]
        public void Apply_Visualizer가null이면예외가발생한다()
        {
            Assert.Throws<ArgumentNullException>(() => MapSceneApplyUtility.Apply(null, CreateValidMapData()));
        }

        [Test]
        public void Clear_Visualizer가null이면예외가발생한다()
        {
            Assert.Throws<ArgumentNullException>(() => MapSceneApplyUtility.Clear(null));
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
    }
}