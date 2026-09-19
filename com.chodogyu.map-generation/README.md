# ChoDogyu Procedural Map Generation

규칙과 Seed를 기반으로 재현 가능한 Room + Corridor 구조의 맵 데이터를 생성하고, 생성 결과를 검증하거나 Unity Scene에 시각화할 수 있도록 구성한 범용 Procedural Map Generation Framework입니다.

맵 생성 로직과 Scene 표현을 분리하여 특정 게임이나 Prefab 구성에 종속되지 않고 사용할 수 있도록 구성했습니다.

## 주요 기능

* Room + Corridor 기반 Procedural Map Generation
* 동일 설정과 Seed를 통한 결정적 맵 재현
* Room 크기, 개수, 간격 및 Map 크기 설정
* 기본 연결 및 Extra Connection 생성
* 논리적 `MapData` 생성
* Room / Corridor / Cell 정보 제공
* 생성 결과 Validation
* Room 및 Floor 연결 상태 검증
* Prefab 기반 Scene Visualization
* XY / XZ Projection 지원
* Editor Map Generator 제공
* Editor Grid Preview
* 생성 결과 Scene Apply / Clear
* Basic Usage Sample 제공
* Runtime / Editor 자동화 테스트 제공

## 패키지 정보

* Package Name: `com.chodogyu.map-generation`
* Runtime Assembly: `CDG.MapGeneration`
* Editor Assembly: `CDG.MapGeneration.Editor`
* Namespace: `CDG.MapGeneration`
* Unity: `6000.3` 이상
* Dependency: `com.chodogyu.core`

## 설치

먼저 ChoDogyu Core 패키지가 설치되어 있어야 합니다.

### ChoDogyu Core v1.0.0

```text
https://github.com/ChoDoGyu/ChoDogyuCore.git?path=/com.chodogyu.core#v1.0.0
```

### ChoDogyu Procedural Map Generation v1.0.0

```text
https://github.com/ChoDoGyu/ChoDogyuMapGeneration.git?path=/com.chodogyu.map-generation#v1.0.0
```

Unity Package Manager의 **Install package from git URL...**을 사용하여 설치합니다.

## 기본 생성

`RoomCorridorMapGenerator`를 통해 맵을 생성할 수 있습니다.

```csharp
using CDG.Core.Results;
using CDG.MapGeneration;

Result<MapData> result =
    RoomCorridorMapGenerator.Generate(
        MapGenerationSettings.Default,
        12345);

if (result.IsSuccess)
{
    MapData mapData = result.Value;
}
```

잘못된 설정이나 Room 배치 실패는 예외 대신 실패 `Result<MapData>`로 반환됩니다.

## Generation Settings

`MapGenerationSettings`를 통해 다음 생성 조건을 지정할 수 있습니다.

* Width
* Height
* Room Count
* Min Room Size
* Max Room Size
* Room Padding
* Edge Padding
* Max Placement Attempts
* Extra Connections

기본 설정은 다음과 같이 사용할 수 있습니다.

```csharp
MapGenerationSettings settings =
    MapGenerationSettings.Default;
```

## Seed 재현성

동일한 Generation Settings와 동일한 Seed를 사용하면 동일한 맵 구조가 생성됩니다.

```csharp
Result<MapData> first =
    RoomCorridorMapGenerator.Generate(
        MapGenerationSettings.Default,
        12345);

Result<MapData> second =
    RoomCorridorMapGenerator.Generate(
        MapGenerationSettings.Default,
        12345);
```

새로운 Seed가 필요한 경우 다음 API를 사용할 수 있습니다.

```csharp
int seed = MapSeedUtility.CreateRandomSeed();
```

## MapData

`MapData`는 생성된 맵의 논리적인 결과를 보관합니다.

주요 정보는 다음과 같습니다.

* Width
* Height
* Seed
* Rooms
* Corridors
* Grid Cell 상태

Cell은 다음 세 상태로 구분됩니다.

* `Empty`
* `Floor`
* `Wall`

```csharp
MapCellType cell = mapData.GetCell(x, y);
```

`MapData`는 Scene GameObject나 Prefab 참조를 포함하지 않습니다.

## Validation

생성된 맵은 `MapValidator`를 통해 검증할 수 있습니다.

```csharp
MapValidationReport report =
    MapValidator.Validate(mapData);
```

다음 정보를 확인할 수 있습니다.

```csharp
report.IsValid
report.ErrorCount
report.WarningCount
report.Issues
```

Room Bounds, Corridor 연결, Corridor Cell 연속성, Room 연결 상태 및 Floor 연결 상태 등을 검사합니다.

## Prefab Visualization

`PrefabMapVisualizer`를 사용하면 `MapData`를 실제 Unity Scene에 Prefab으로 표현할 수 있습니다.

```csharp
Result result = visualizer.Visualize(mapData);
```

Visualizer는 맵을 생성하지 않고 전달받은 `MapData`를 표현하는 역할만 담당합니다.

### PrefabMapTheme

Floor와 Wall 표현에 사용할 Prefab은 `PrefabMapTheme`에서 지정합니다.

생성 메뉴:

```text
Create
└── CDG
    └── Map Generation
        └── Prefab Map Theme
```

다음 Prefab이 필요합니다.

* Floor Prefab
* Wall Prefab

Visualizer는 XY 또는 XZ 평면에 Grid를 배치할 수 있으며 `CellSize`로 Cell 간 World 간격을 설정할 수 있습니다.

생성한 Scene 표현은 다음과 같이 제거합니다.

```csharp
visualizer.Clear();
```

## Editor Tool

다음 메뉴에서 Map Generator를 열 수 있습니다.

```text
Tools
└── CDG
    └── Map Generation
        └── Map Generator
```

Editor Window에서는 다음 기능을 제공합니다.

* Generation Settings 편집
* Seed 입력
* Random Seed 생성
* Generate
* Regenerate
* Validate
* Clear
* Grid Preview
* 생성 결과 Summary
* PrefabMapVisualizer 지정
* Apply to Scene
* Clear Scene

Editor Tool에서도 Runtime과 동일한 Generation API를 사용합니다.

## Basic Usage Sample

Package Manager에서 `Basic Usage` Sample을 Import할 수 있습니다.

Sample에서는 다음 흐름을 확인할 수 있습니다.

* 기본 Seed를 이용한 맵 생성
* 동일 Seed 재생성
* 새로운 Random Seed 생성
* Map Validation
* Prefab 기반 Scene Visualization
* Visualization Clear

Sample Scene:

```text
Scenes/BasicUsage.unity
```

Floor와 Wall은 기능 확인을 위한 단순 Prefab으로 구성되어 있으며 실제 프로젝트에서는 원하는 Prefab으로 교체할 수 있습니다.

## 테스트

최종 자동화 테스트 결과:

```text
PlayMode: 103 Passed
Editor:    5 Passed
Total:   108 Passed
```

주요 검증 범위:

* Map Data Model
* Generation Settings
* Deterministic Random
* Room Placement
* Room Connection
* Extra Connections
* Corridor Generation
* Cell Building
* Room + Corridor 전체 Generation
* Seed Reproducibility
* Map Validation
* Prefab Visualization
* Editor Scene Apply / Clear

새 빈 Unity 프로젝트에서 Core와 본 패키지만 설치한 상태의 독립 설치 및 Basic Usage Sample 실행도 별도로 검증합니다.

## 설계 방향

이 패키지는 다음 책임을 분리합니다.

* **Generation** — 설정과 Seed를 기반으로 논리적 맵 생성
* **Map Data** — 생성 결과 데이터 보관
* **Validation** — 생성 결과의 구조적 유효성 검사
* **Visualization** — `MapData`를 Unity Scene에 표현
* **Editor** — Runtime Generation 기능을 Editor Workflow로 제공

특정 게임의 전투, 스폰, 퀘스트, 아이템 배치 등의 게임 규칙은 포함하지 않으며 Procedural Map Generation 자체의 책임에 집중합니다.
