# 샘플 생성

`Sample` 탭은 선택한 로컬 또는 임베디드 패키지에 UPM 표준 샘플 폴더를 생성하고 `package.json`에 등록합니다.

## 실행 순서

1. Unity 메뉴에서 `Tools/UPM Package Generator`를 선택합니다.
2. 왼쪽 메뉴에서 `Sample` 탭을 선택합니다.
3. 샘플을 추가할 대상 패키지를 선택합니다.
4. `Display Name`과 `Description`을 입력합니다.
5. `Generate Package Sample` 버튼을 누릅니다.

## 입력 항목

| 항목 | 설명 |
| --- | --- |
| `Package` | 샘플을 추가할 로컬 또는 임베디드 패키지입니다. |
| `Display Name` | 샘플 표시 이름입니다. 샘플 폴더명 생성에도 사용됩니다. |
| `Description` | `package.json`의 `samples` 배열에 기록할 샘플 설명입니다. |

모든 항목을 입력해야 샘플을 생성할 수 있습니다.

## 샘플 이름 규칙

샘플 폴더명은 `Display Name`의 공백을 `_`로 바꾼 값입니다. `Display Name`에는 문자, 숫자, 공백, `_`만 사용할 수 있습니다.

예를 들어 `Display Name`이 `Basic Sample`이면 샘플 경로는 `Samples~/Basic_Sample`이 됩니다.

## 생성 결과

샘플 생성 버튼을 누르면 선택 패키지에 다음 구성이 추가됩니다.

```text
Samples~/
  Basic_Sample/
    Basic_Sample.asmdef
    Scripts/
      SampleScript.cs
```

패키지의 `package.json`에는 다음과 같은 `samples` 항목이 추가됩니다.

```json
{
  "displayName": "Basic Sample",
  "description": "Sample description",
  "path": "Samples~/Basic_Sample"
}
```

## 중복 처리

이미 같은 표시 이름이나 같은 경로의 샘플이 `package.json`에 등록되어 있으면 생성이 중단됩니다. 같은 이름의 샘플 폴더가 이미 있어도 생성할 수 없습니다.

## 주의사항

- 샘플 생성 대상 패키지는 현재 프로젝트에서 로컬 또는 임베디드 패키지로 인식되어야 합니다.
- 샘플은 Unity Package Manager의 Samples 영역에서 가져올 수 있도록 `Samples~` 폴더 아래에 생성됩니다.
- 생성된 샘플 스크립트는 기본 `MonoBehaviour` 예시 코드만 포함합니다.
