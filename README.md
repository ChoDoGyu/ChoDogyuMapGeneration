# ChoDogyu Procedural Map Generation

Unity 프로젝트에서 재사용할 수 있도록 제작한 Room + Corridor 기반 Procedural Map Generation Framework입니다.

설정과 Seed를 기반으로 재현 가능한 논리적 맵 데이터를 생성하며, 생성 결과에 대한 Validation과 Prefab 기반 Scene Visualization, Editor Tool을 제공합니다.

## Repository Structure

```text
ChoDogyuMapGeneration/
├── MapGenerationDevelopment/
└── com.chodogyu.map-generation/
```

### MapGenerationDevelopment

Framework를 개발하고 기능 및 테스트를 검증하기 위한 Unity 프로젝트입니다.

### com.chodogyu.map-generation

실제 Unity Package Manager를 통해 배포하고 사용할 UPM 패키지입니다.

## 주요 기능

* Room + Corridor Procedural Generation
* Seed 기반 결정적 맵 재현
* Room 배치 및 연결
* Extra Connection
* Corridor 생성
* 논리적 Grid Cell 구성
* `MapData` 기반 생성 결과 제공
* 생성 결과 Validation
* Room / Floor 연결 상태 검증
* Prefab 기반 Scene Visualization
* XY / XZ Projection
* Editor Map Generator
* Grid Preview
* Scene Apply / Clear
* Basic Usage Sample
* Runtime / Editor 자동화 테스트

## Package

```text
com.chodogyu.map-generation
```

Runtime Assembly:

```text
CDG.MapGeneration
```

Editor Assembly:

```text
CDG.MapGeneration.Editor
```

Unity:

```text
6000.3 이상
```

Dependency:

```text
com.chodogyu.core
```

## Installation

먼저 ChoDogyu Core v1.0.0을 설치합니다.

```text
https://github.com/ChoDoGyu/ChoDogyuCore.git?path=/com.chodogyu.core#v1.0.0
```

이후 Procedural Map Generation v1.0.0을 설치합니다.

```text
https://github.com/ChoDoGyu/ChoDogyuMapGeneration.git?path=/com.chodogyu.map-generation#v1.0.0
```

Unity Package Manager의 **Install package from git URL...**을 사용합니다.

## 기본 흐름

Framework의 기본 처리 흐름은 다음과 같습니다.

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

맵을 생성하는 로직과 Unity Scene 표현을 분리하여 생성 알고리즘이 특정 Prefab이나 게임 프로젝트 구조에 종속되지 않도록 구성했습니다.

## Editor Tool

다음 메뉴에서 Editor Tool을 사용할 수 있습니다.

```text
Tools
└── CDG
    └── Map Generation
        └── Map Generator
```

Generation Settings와 Seed를 입력하여 맵을 생성하고 Grid Preview, Validation 결과 및 생성 Summary를 확인할 수 있습니다.

`PrefabMapVisualizer`를 지정하면 생성된 결과를 실제 Scene에 적용하거나 제거할 수도 있습니다.

## Sample

Package Manager에서 `Basic Usage` Sample을 Import할 수 있습니다.

Sample에서는 다음 기능을 확인할 수 있습니다.

* 맵 생성
* 동일 Seed 재생성
* Random Seed 생성
* Validation
* Prefab Scene Visualization
* Visualization Clear

## Verification

최종 자동화 테스트:

```text
PlayMode: 103 Passed
Editor:    5 Passed
Total:   108 Passed
```

추가로 새 빈 Unity 프로젝트에서 다음 흐름을 검증했습니다.

```text
ChoDogyu Core 설치
→ Procedural Map Generation 설치
→ Basic Usage Sample Import
→ Sample Scene 실행
→ 맵 생성 및 Visualization 확인
```

## Design

이 Framework는 특정 게임 콘텐츠를 완성된 형태로 제공하는 것이 아니라 Procedural Map Generation을 위한 재사용 가능한 기반 계층을 제공하는 것을 목표로 합니다.

전투, 적 스폰, 아이템 배치, 퀘스트, 게임 진행 규칙은 패키지의 책임에서 제외하고 맵 생성 데이터와 검증 및 표현에 집중합니다.

## Version

Current Release:

```text
v1.0.0
```
