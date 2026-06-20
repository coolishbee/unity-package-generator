# 패키지 생성

`Package Generation` 탭은 새 UPM 패키지 폴더와 기본 파일을 생성합니다.

## 실행 순서

1. Unity 메뉴에서 `Tools/UPM Package Generator`를 선택합니다.
2. 왼쪽 메뉴에서 `Package Generation` 탭을 선택합니다.
3. 생성 위치와 패키지 정보를 입력합니다.
4. 필요한 어셈블리와 테스트 옵션을 선택합니다.
5. `Generate Package` 버튼을 누릅니다.
6. 확인 창에서 생성 경로를 확인한 뒤 진행합니다.

## 위치 설정

`Location`은 패키지를 생성할 기준 폴더입니다. 현재 Unity 프로젝트 루트 기준 상대 경로를 사용할 수 있습니다.

| 버튼 | 동작 |
| --- | --- |
| `Reset` | 기본 위치인 `../../Packages`로 되돌립니다. |
| `Local` | 현재 Unity 프로젝트의 `Packages` 폴더를 사용합니다. |
| `Browse` | 폴더 선택 창에서 생성 위치를 선택합니다. |

기본 위치 `../../Packages`는 Unity 프로젝트 밖의 공용 `Packages` 폴더에 패키지를 생성할 때 사용합니다. 로컬 위치 `Packages`는 현재 Unity 프로젝트에 임베디드 패키지를 만들 때 사용합니다.

## 패키지 정보

| 항목 | 설명 |
| --- | --- |
| `Category` | 패키지 이름에 사용할 분류입니다. |
| `Display Name` | 패키지 표시 이름이며 생성 폴더명에도 사용됩니다. |
| `Package Name` | UPM 패키지 이름입니다. 기본 prefix는 `com.author.`입니다. |
| `Description` | `package.json`의 설명입니다. |
| `Author` | `package.json`의 작성자 이름입니다. |
| `Unity Version` | `package.json`의 `unity` 값입니다. 기본값은 `6000.2`입니다. |

`Category` 또는 `Display Name`을 변경하면 `Package Name`이 자동으로 갱신됩니다. 예를 들어 `Category`가 `Tools`, `Display Name`이 `My Package`이면 패키지 이름은 `com.author.tools.my-package` 형식으로 만들어집니다.

## 생성 옵션

| 옵션 | 생성 내용 |
| --- | --- |
| `Include Runtime Assembly` | `Runtime` 폴더와 런타임 어셈블리 정의 파일을 생성합니다. |
| `Include Editor Assembly` | `Editor` 폴더와 에디터 전용 어셈블리 정의 파일을 생성합니다. |
| `Include Tests` | `Tests` 폴더와 테스트 의존성을 생성합니다. |
| `Include Runtime Tests` | `Tests/Runtime` 테스트 어셈블리 정의 파일을 생성합니다. |
| `Include Editor Tests` | `Tests/Editor` 테스트 어셈블리 정의 파일을 생성합니다. |

`Include Tests`를 켜면 `package.json`에 `com.unity.test-framework` 의존성이 추가됩니다. 테스트를 포함하는 경우 `Include Runtime Tests` 또는 `Include Editor Tests` 중 하나 이상을 선택해야 합니다.

`Include Runtime Assembly`와 `Include Editor Assembly`를 모두 끌 수는 없습니다. 패키지는 최소 하나의 Runtime 또는 Editor 어셈블리를 포함해야 합니다.

## 생성 결과

선택한 옵션에 따라 다음 구조가 생성됩니다.

```text
<Location>/<Display_Name>/
  package.json
  Runtime/
    Coolishbee.<AssemblyName>.Runtime.asmdef
  Editor/
    Coolishbee.<AssemblyName>.Editor.asmdef
  Tests/
    Runtime/
      Coolishbee.<AssemblyName>.Tests.Runtime.asmdef
    Editor/
      Coolishbee.<AssemblyName>.Tests.Editor.asmdef
```

선택하지 않은 옵션의 폴더와 어셈블리 정의 파일은 생성되지 않습니다.

## 외부 위치 생성

`Location`이 `Packages`가 아닌 경우 생성된 패키지는 외부 위치 패키지로 처리됩니다. 이때 현재 Unity 프로젝트의 `Packages/manifest.json`에 `file:` 의존성이 자동으로 추가됩니다.

테스트를 포함한 경우 `manifest.json`의 `testables` 배열에도 생성 패키지 이름이 추가됩니다. 이 설정을 통해 Unity Test Runner가 패키지 테스트를 발견할 수 있습니다.

## 입력 제한

- 패키지 생성 경로에 같은 이름의 폴더가 이미 있으면 생성할 수 없습니다.
- `Package Name`은 영문 소문자, 숫자, `-`, `_`, `.`만 사용할 수 있습니다.
- `Package Name`의 첫 글자와 마지막 글자는 영문 소문자 또는 숫자여야 합니다.
- `Display Name`, `Description`, `Author`는 비워 둘 수 없습니다.
