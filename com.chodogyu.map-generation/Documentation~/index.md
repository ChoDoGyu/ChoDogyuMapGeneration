# ChoDogyu Procedural Map Generation

`ChoDogyu Procedural Map Generation`은 규칙과 Seed를 기반으로 재현 가능한 Room + Corridor 구조의 맵 데이터를 생성하고, 생성 결과를 검증하거나 Unity Scene에 시각화할 수 있도록 구성한 Unity용 Procedural Map Generation Framework입니다.

맵 생성 로직, 생성 결과 데이터, 검증, Scene Visualization을 서로 분리하여 특정 게임 프로젝트에 종속되지 않고 재사용할 수 있도록 설계했습니다.

---

## Package Information

* Package: `com.chodogyu.map-generation`
* Version: `1.0.0`
* Runtime Assembly: `CDG.MapGeneration`
* Editor Assembly: `CDG.MapGeneration.Editor`
* Unity: `6000.3` 이상
* Required Package: `com.chodogyu.core`

---

## Requirements

이 패키지는 `CDG.Core.Results`의 `Result` 및 `Result<T>`를 사용합니다.

따라서 Procedural Map Generation 패키지를 설치하기 전에 Core 패키지가 필요합니다.

Core:

```text
https://github.com/ChoDoGyu/ChoDogyuCore.git?path=/com.chodogyu.core#v1.0.0
```

---

## Installation

Unity Package Manager의 **Add package from git URL...**을 사용합니다.

먼저 Core를 설치합니다.

```text
https://github.com/ChoDoGyu/ChoDogyuCore.git?path=/com.chodogyu.core#v1.0.0
```

그다음 Procedural Map Generation 패키지를 설치합니다.

```text
https://github.com/ChoDoGyu/ChoDogyuMapGeneration.git?path=/com.chodogyu.map-generation#v1.0.0
```

설치 후 Package Manager에서 `ChoDogyu Procedural Map Generation`을 확인할 수 있습니다.

---

## Architecture

패키지는 다음 흐름을 기준으로 책임을 분리합니다.

```text
Generation Settings + Seed
        ↓
Room / Connection / Corridor Generation
        ↓
MapData
        ↓
Validation
        ↓
Visualization
        ↓
Unity Scene
```

핵심 원칙은 **맵 생성 결과와 Unity Scene 표현을 분리하는 것**입니다.

`RoomCorridorMapGenerator`는 Scene GameObject를 생성하지 않고 논리적인 `MapData`만 반환합니다.

생성된 데이터를 실제 Unity Scene에 표현하는 작업은 `PrefabMapVisualizer`가 담당합니다.

이를 통해 생성 로직을 Scene 구성이나 특정 Prefab에 종속시키지 않습니다.

---

## Generation Settings

`MapGenerationSettings`는 Room + Corridor 맵을 생성하기 위한 설정을 보관합니다.

주요 설정은 다음과 같습니다.

* `Width`

  * Grid 가로 Cell 개수
* `Height`

  * Grid 세로 Cell 개수
* `RoomCount`

  * 생성할 Room 개수
* `MinRoomSize`

  * Room 최소 크기
* `MaxRoomSize`

  * Room 최대 크기
* `RoomPadding`

  * 서로 다른 Room 사이의 최소 간격
* `EdgePadding`

  * Room과 Map 외곽 사이의 최소 간격
* `MaxPlacementAttemptsPerRoom`

  * Room 하나를 배치할 때 허용하는 최대 시도 횟수
* `ExtraConnectionCount`

  * 기본 연결 이후 추가할 Room 연결 개수

기본 설정은 다음과 같이 사용할 수 있습니다.

```csharp
MapGenerationSettings settings = MapGenerationSettings.Default;
```

기본값은 `64 × 64` Grid에서 Room 12개를 생성하도록 구성되어 있습니다.

---

## Map Generation

Room + Corridor 맵 생성의 진입점은 `RoomCorridorMapGenerator`입니다.

```csharp
Result<MapData> result =
    RoomCorridorMapGenerator.Generate(MapGenerationSettings.Default, 12345);
```

생성 과정에서는 다음 작업이 순차적으로 수행됩니다.

1. Generation Settings 검증
2. Seed 기반 Random 생성
3. Room 배치
4. Room 연결 관계 구성
5. Corridor 생성
6. Grid Cell 구성
7. `MapData` 생성

설정이 잘못되었거나 필요한 Room을 정상적으로 배치할 수 없는 경우 예외 대신 실패 `Result<MapData>`가 반환됩니다.

---

## Seed Reproducibility

맵 생성은 Seed를 기준으로 재현할 수 있도록 구성되어 있습니다.

동일한 Generation Settings와 동일한 Seed를 사용하면 동일한 맵 구조가 생성됩니다.

```csharp
Result<MapData> first =
    RoomCorridorMapGenerator.Generate(MapGenerationSettings.Default, 12345);

Result<MapData> second =
    RoomCorridorMapGenerator.Generate(MapGenerationSettings.Default, 12345);
```

새로운 Seed가 필요한 경우 `MapSeedUtility`를 사용할 수 있습니다.

```csharp
int seed = MapSeedUtility.CreateRandomSeed();
```

Seed 재현성은 자동화 테스트를 통해 별도로 검증합니다.

---

## Map Data

`MapData`는 한 번의 Procedural Generation 결과를 표현하는 논리적 데이터입니다.

주요 정보는 다음과 같습니다.

* `Width`
* `Height`
* `Seed`
* `Rooms`
* `Corridors`
* Grid Cell 상태

Cell 상태는 `MapCellType`으로 표현됩니다.

주요 Cell 종류는 다음과 같습니다.

```text
Empty
Floor
Wall
```

특정 좌표의 Cell은 다음과 같이 확인할 수 있습니다.

```csharp
MapCellType cell = mapData.GetCell(x, y);
```

좌표가 Map Bounds 내부인지 확인할 수도 있습니다.

```csharp
bool inBounds = mapData.IsInBounds(position);
```

Room과 Corridor 목록은 외부에서 직접 수정할 수 없는 읽기 전용 형태로 제공됩니다.

`MapData` 자체에는 Prefab이나 Scene GameObject 정보가 포함되지 않습니다.

---

## Validation

생성된 맵의 논리적 유효성은 `MapValidator`가 담당합니다.

```csharp
MapValidationReport report = MapValidator.Validate(mapData);
```

검증 결과는 `MapValidationReport`를 통해 확인합니다.

주요 정보:

```csharp
report.IsValid
report.ErrorCount
report.WarningCount
report.Issues
```

Validation에서는 단순히 맵 생성 성공 여부만 확인하지 않고 생성된 데이터 내부의 구조적 문제도 검사합니다.

대표적인 검증 대상은 다음과 같습니다.

* Room 정보 유효성
* Room Bounds
* Corridor가 존재하는 Room을 참조하는지
* Corridor 시작점과 끝점
* Corridor Cell 범위
* Corridor Cell 연속성
* Room 연결 상태
* Floor 영역 연결 상태

Error가 존재하지 않는 경우 `IsValid`가 `true`가 됩니다.

---

## Visualization

논리적인 `MapData`를 Unity Scene에 표현하는 작업은 `PrefabMapVisualizer`가 담당합니다.

```csharp
Result result = visualizer.Visualize(mapData);
```

Visualizer는 맵을 다시 생성하지 않습니다.

전달받은 `MapData`의 Cell 상태를 읽고 Floor와 Wall을 Prefab으로 표현하는 역할만 담당합니다.

### PrefabMapTheme

`PrefabMapTheme`은 Visualization에서 사용할 Prefab 구성을 정의하는 ScriptableObject입니다.

다음 메뉴에서 생성할 수 있습니다.

```text
Create
└── CDG
    └── Map Generation
        └── Prefab Map Theme
```

Theme에는 다음 Prefab을 지정합니다.

* Floor Prefab
* Wall Prefab

두 Prefab이 모두 지정되어 있어야 정상적인 Theme으로 판단합니다.

### Projection Plane

`PrefabMapVisualizer`는 Grid를 다음 두 World Plane 중 하나에 배치할 수 있습니다.

* `XZ`
* `XY`

### Cell Size

`CellSize`를 통해 Grid Cell 사이의 실제 World 간격을 지정할 수 있습니다.

### Clear

현재 Visualizer가 생성한 Scene 표현은 다음과 같이 제거할 수 있습니다.

```csharp
visualizer.Clear();
```

Visualizer가 생성하는 오브젝트는 Visualizer 아래의 별도 Generated Root에 배치됩니다.

---

## Runtime Usage

기본적인 Runtime 사용 흐름은 다음과 같습니다.

```csharp
using CDG.Core.Results;
using UnityEngine;

public sealed class MapExample : MonoBehaviour
{
    [SerializeField] private PrefabMapVisualizer visualizer;

    private void Start()
    {
        int seed = 12345;

        Result<MapData> generation =
            RoomCorridorMapGenerator.Generate(
                MapGenerationSettings.Default,
                seed);

        if (generation.IsFailure)
        {
            Debug.LogError(
                $"{generation.Error.Code} / {generation.Error.Message}");
            return;
        }

        MapValidationReport validation =
            MapValidator.Validate(generation.Value);

        if (!validation.IsValid)
        {
            Debug.LogError(
                $"Map Validation 실패: {validation.ErrorCount}");
            return;
        }

        Result visualization =
            visualizer.Visualize(generation.Value);

        if (visualization.IsFailure)
        {
            Debug.LogError(
                $"{visualization.Error.Code} / {visualization.Error.Message}");
        }
    }
}
```

Runtime에서도 Generation → Validation → Visualization의 각 단계를 필요에 따라 독립적으로 사용할 수 있습니다.

---

## Editor Tool

패키지는 Room + Corridor 맵을 Unity Editor에서 직접 생성하고 확인할 수 있는 Editor Window를 제공합니다.

메뉴:

```text
Tools
└── CDG
    └── Map Generation
        └── Map Generator
```

Editor Window에서는 다음 기능을 사용할 수 있습니다.

### Generation Settings

다음 값을 직접 수정할 수 있습니다.

* Width
* Height
* Room Count
* Min Room Size
* Max Room Size
* Room Padding
* Edge Padding
* Max Placement Attempts
* Extra Connections

`Reset Defaults` 버튼을 사용하면 기본 설정으로 되돌릴 수 있습니다.

### Seed

Seed를 직접 입력하거나 `Randomize` 버튼으로 새로운 Seed를 만들 수 있습니다.

### Generate

현재 Generation Settings와 Seed를 사용하여 새로운 `MapData`를 생성합니다.

### Regenerate

현재 설정과 Seed를 다시 사용하여 맵을 생성합니다.

동일한 설정과 Seed를 유지하면 동일한 구조가 재현됩니다.

### Validate

현재 생성된 `MapData`에 대해 Validation을 다시 수행합니다.

### Clear

Editor Window가 보관하고 있는 현재 `MapData`를 제거합니다.

### Map Preview

생성된 `MapData`를 Editor Window 내부에서 Grid 형태로 미리 확인할 수 있습니다.

Preview는 논리적인 Cell 정보를 표시하며 Scene GameObject 생성과는 분리되어 있습니다.

### Generated Map Summary

생성 이후 다음 정보를 확인할 수 있습니다.

* Map Size
* Seed
* Room Count
* Corridor Count
* Floor Cell Count
* Wall Cell Count
* Empty Cell Count
* Validation 결과
* Error Count
* Warning Count

### Apply to Scene

Scene에 존재하는 `PrefabMapVisualizer`를 지정한 뒤 `Apply to Scene`을 실행하면 현재 생성된 `MapData`를 Scene에 적용할 수 있습니다.

### Clear Scene

지정된 Visualizer가 생성한 Scene Visualization을 제거합니다.

Editor에서도 Generation과 Scene Visualization이 별도 단계로 유지됩니다.

---

## Basic Usage Sample

Package Manager에서 다음 Sample을 Import할 수 있습니다.

```text
Basic Usage
```

Sample 위치:

```text
Samples~/BasicUsage
```

Import 후 다음 Scene을 실행합니다.

```text
Scenes/BasicUsage.unity
```

Sample에서는 다음 기본 흐름을 확인할 수 있습니다.

1. 기본 Seed로 맵 생성
2. Map Validation
3. Prefab 기반 Scene Visualization
4. 동일 Seed를 이용한 재생성
5. 새로운 Random Seed를 이용한 다른 맵 생성
6. 생성된 Visualization 제거

화면 왼쪽 위에는 다음 버튼이 제공됩니다.

### Generate Same Seed

현재 Seed로 다시 생성합니다.

동일한 설정과 Seed를 사용하므로 동일한 맵 구조를 확인할 수 있습니다.

### Generate Random Seed

`MapSeedUtility`를 사용해 새로운 Seed를 만든 뒤 새로운 맵을 생성합니다.

### Clear

현재 Scene Visualization을 제거합니다.

Sample의 Floor와 Wall은 기능 확인을 위한 단순 Prefab이며 실제 프로젝트에서는 원하는 Prefab으로 교체할 수 있습니다.

---

## Tests

패키지에는 Runtime과 Editor 영역에 대한 자동화 테스트가 포함되어 있습니다.

최종 검증 기준:

```text
PlayMode: 103 Passed
Editor:    5 Passed
Total:   108 Passed
```

주요 테스트 범위는 다음과 같습니다.

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

자동화 테스트 외에도 별도의 빈 Unity 프로젝트에서 Package 설치와 `Basic Usage` Sample Import 및 실행을 검증합니다.

---

## Responsibilities

패키지는 각 구성 요소의 책임을 다음과 같이 구분합니다.

### Generation

Generation Settings와 Seed를 기반으로 논리적 맵 구조를 생성합니다.

Scene GameObject나 Prefab 표현을 담당하지 않습니다.

### Map Data

생성된 Room, Corridor, Cell 및 Seed 정보를 보관합니다.

특정 게임 규칙이나 Scene 표현을 포함하지 않습니다.

### Validation

완성된 `MapData`가 정의된 구조적 조건을 만족하는지 검사합니다.

Generation과 Visualization에서 독립적으로 사용할 수 있습니다.

### Visualization

완성된 `MapData`를 Prefab 기반 Unity Scene으로 표현합니다.

맵 생성 규칙에는 관여하지 않습니다.

### Editor

Runtime Generation API를 그대로 사용하여 생성, Preview, Validation 및 Scene 적용 작업을 Editor에서 사용할 수 있도록 제공합니다.

Editor 전용 생성 알고리즘을 별도로 구현하지 않습니다.

---

## Design Goals

이 패키지는 다음 원칙을 기준으로 구성했습니다.

* 동일 Seed의 결정적 생성 결과
* Generation과 Visualization의 분리
* 논리적인 Map Data 중심 구조
* Runtime과 Editor의 동일 Generation Core 사용
* 생성 결과에 대한 별도 Validation
* 특정 게임 프로젝트에 종속되지 않는 범용 구조
* Prefab 교체를 통한 Scene 표현 확장
* UPM을 통한 독립 설치 및 제거
* Sample을 통한 기본 사용 흐름 제공

`v1.0.0`은 Room + Corridor 방식의 재현 가능한 맵 생성과 검증, Prefab 기반 Scene Visualization, Editor Tool을 하나의 독립 Unity Package로 제공하는 것을 범위로 합니다.
