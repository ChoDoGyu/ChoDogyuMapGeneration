using CDG.Core.Results;
using UnityEditor;
using UnityEngine;

namespace CDG.MapGeneration.Editor
{
    /// <summary>
    /// Room + Corridor 맵의 생성 설정과 Seed를 입력하고 생성 결과를 확인할 수 있는 Editor Window입니다.
    /// Preview와 Scene 적용 기능은 별도 기능으로 확장됩니다.
    /// </summary>
    public sealed class MapGenerationEditorWindow : EditorWindow
    {
        [SerializeField] private bool initialized;
        [SerializeField] private int width;
        [SerializeField] private int height;
        [SerializeField] private int roomCount;
        [SerializeField] private Vector2Int minRoomSize;
        [SerializeField] private Vector2Int maxRoomSize;
        [SerializeField] private int roomPadding;
        [SerializeField] private int edgePadding;
        [SerializeField] private int maxPlacementAttemptsPerRoom;
        [SerializeField] private int extraConnectionCount;
        [SerializeField] private int seed;

        private MapData currentMapData;
        private MapValidationReport currentValidation;
        private string statusMessage;
        private MessageType statusType = MessageType.Info;
        private Vector2 scrollPosition;

        [MenuItem("Tools/CDG/Map Generation/Map Generator")]
        private static void OpenWindow()
        {
            MapGenerationEditorWindow window = GetWindow<MapGenerationEditorWindow>("Map Generator");
            window.minSize = new Vector2(420f, 560f);
            window.Show();
        }

        private void OnEnable()
        {
            if (initialized)
            {
                return;
            }

            ApplyDefaultSettings();
            seed = MapSeedUtility.CreateRandomSeed();
            initialized = true;
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawHeader();
            EditorGUILayout.Space(8f);

            DrawGenerationSettings();
            EditorGUILayout.Space(8f);

            DrawSeedSettings();
            EditorGUILayout.Space(8f);

            DrawActions();
            EditorGUILayout.Space(8f);

            DrawStatus();
            DrawSummary();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("Procedural Map Generator", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "Room + Corridor 구조의 논리적 MapData를 설정과 Seed를 기반으로 생성합니다.",
                EditorStyles.wordWrappedLabel);
        }

        private void DrawGenerationSettings()
        {
            EditorGUILayout.LabelField("Generation Settings", EditorStyles.boldLabel);

            width = EditorGUILayout.IntField("Width", width);
            height = EditorGUILayout.IntField("Height", height);
            roomCount = EditorGUILayout.IntField("Room Count", roomCount);
            minRoomSize = EditorGUILayout.Vector2IntField("Min Room Size", minRoomSize);
            maxRoomSize = EditorGUILayout.Vector2IntField("Max Room Size", maxRoomSize);
            roomPadding = EditorGUILayout.IntField("Room Padding", roomPadding);
            edgePadding = EditorGUILayout.IntField("Edge Padding", edgePadding);
            maxPlacementAttemptsPerRoom = EditorGUILayout.IntField(
                "Max Placement Attempts", maxPlacementAttemptsPerRoom);
            extraConnectionCount = EditorGUILayout.IntField("Extra Connections", extraConnectionCount);

            if (GUILayout.Button("Reset Defaults"))
            {
                ApplyDefaultSettings();
                SetStatus("기본 Generation Settings를 적용했습니다.", MessageType.Info);
            }
        }

        private void DrawSeedSettings()
        {
            EditorGUILayout.LabelField("Seed", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            seed = EditorGUILayout.IntField(seed);

            if (GUILayout.Button("Randomize", GUILayout.Width(100f)))
            {
                seed = MapSeedUtility.CreateRandomSeed();
                SetStatus($"새 Seed를 생성했습니다: {seed}", MessageType.Info);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawActions()
        {
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Generate", GUILayout.Height(28f)))
            {
                GenerateMap();
            }

            EditorGUI.BeginDisabledGroup(currentMapData == null);

            if (GUILayout.Button("Regenerate", GUILayout.Height(28f)))
            {
                GenerateMap();
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(currentMapData == null);

            if (GUILayout.Button("Validate"))
            {
                ValidateCurrentMap();
            }

            if (GUILayout.Button("Clear"))
            {
                ClearCurrentMap();
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawStatus()
        {
            if (string.IsNullOrEmpty(statusMessage))
            {
                return;
            }

            EditorGUILayout.HelpBox(statusMessage, statusType);
            EditorGUILayout.Space(4f);
        }

        private void DrawSummary()
        {
            if (currentMapData == null)
            {
                return;
            }

            EditorGUILayout.LabelField("Generated Map Summary", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Size", $"{currentMapData.Width} x {currentMapData.Height}");
            EditorGUILayout.LabelField("Seed", currentMapData.Seed.ToString());
            EditorGUILayout.LabelField("Rooms", currentMapData.Rooms.Count.ToString());
            EditorGUILayout.LabelField("Corridors", currentMapData.Corridors.Count.ToString());

            CountCells(currentMapData, out int floorCount, out int wallCount, out int emptyCount);

            EditorGUILayout.LabelField("Floor Cells", floorCount.ToString());
            EditorGUILayout.LabelField("Wall Cells", wallCount.ToString());
            EditorGUILayout.LabelField("Empty Cells", emptyCount.ToString());

            if (currentValidation != null)
            {
                EditorGUILayout.LabelField("Validation", currentValidation.IsValid ? "Valid" : "Invalid");
                EditorGUILayout.LabelField("Errors", currentValidation.ErrorCount.ToString());
                EditorGUILayout.LabelField("Warnings", currentValidation.WarningCount.ToString());
            }
        }

        private void GenerateMap()
        {
            MapGenerationSettings settings = CreateSettings();
            Result<MapData> result = RoomCorridorMapGenerator.Generate(settings, seed);

            if (result.IsFailure)
            {
                currentMapData = null;
                currentValidation = null;
                SetStatus($"{result.Error.Code}\n{result.Error.Message}", MessageType.Error);
                return;
            }

            currentMapData = result.Value;
            currentValidation = MapValidator.Validate(currentMapData);

            if (!currentValidation.IsValid)
            {
                SetStatus(
                    $"맵은 생성되었지만 Validation에 실패했습니다. Error Count: {currentValidation.ErrorCount}",
                    MessageType.Error);
                return;
            }

            SetStatus(
                $"맵 생성 완료 - Seed: {currentMapData.Seed}, Rooms: {currentMapData.Rooms.Count}, " +
                $"Corridors: {currentMapData.Corridors.Count}",
                MessageType.Info);
        }

        private void ValidateCurrentMap()
        {
            if (currentMapData == null)
            {
                return;
            }

            currentValidation = MapValidator.Validate(currentMapData);

            if (currentValidation.IsValid)
            {
                SetStatus("Map Validation 통과 - 발견된 Error가 없습니다.", MessageType.Info);
                return;
            }

            SetStatus($"Map Validation 실패 - Error Count: {currentValidation.ErrorCount}", MessageType.Error);
        }

        private void ClearCurrentMap()
        {
            currentMapData = null;
            currentValidation = null;
            SetStatus("현재 생성된 MapData를 제거했습니다.", MessageType.Info);
        }

        private MapGenerationSettings CreateSettings()
        {
            return new MapGenerationSettings(width, height, roomCount, minRoomSize, maxRoomSize, roomPadding,
                edgePadding, maxPlacementAttemptsPerRoom, extraConnectionCount);
        }

        private void ApplyDefaultSettings()
        {
            MapGenerationSettings defaults = MapGenerationSettings.Default;

            width = defaults.Width;
            height = defaults.Height;
            roomCount = defaults.RoomCount;
            minRoomSize = defaults.MinRoomSize;
            maxRoomSize = defaults.MaxRoomSize;
            roomPadding = defaults.RoomPadding;
            edgePadding = defaults.EdgePadding;
            maxPlacementAttemptsPerRoom = defaults.MaxPlacementAttemptsPerRoom;
            extraConnectionCount = defaults.ExtraConnectionCount;
        }

        private void SetStatus(string message, MessageType type)
        {
            statusMessage = message;
            statusType = type;
            Repaint();
        }

        private static void CountCells(MapData mapData, out int floorCount, out int wallCount, out int emptyCount)
        {
            floorCount = 0;
            wallCount = 0;
            emptyCount = 0;

            for (int y = 0; y < mapData.Height; y++)
            {
                for (int x = 0; x < mapData.Width; x++)
                {
                    switch (mapData.GetCell(x, y))
                    {
                        case MapCellType.Floor:
                            floorCount++;
                            break;

                        case MapCellType.Wall:
                            wallCount++;
                            break;

                        default:
                            emptyCount++;
                            break;
                    }
                }
            }
        }
    }
}