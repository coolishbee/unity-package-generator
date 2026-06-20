# 문서 생성

`Documentation` 탭은 선택한 로컬 또는 임베디드 패키지에 `Documentation/docfx` 문서 프로젝트를 생성합니다.

## 실행 순서

1. Unity 메뉴에서 `Tools/UPM Package Generator`를 선택합니다.
2. 왼쪽 메뉴에서 `Documentation` 탭을 선택합니다.
3. 문서를 생성할 대상 패키지를 선택합니다.
4. 필요한 문서 옵션을 선택합니다.
5. `Generate Documents` 버튼을 누릅니다.

## 대상 패키지

대상 목록에는 현재 Unity 프로젝트에서 인식되는 로컬 또는 임베디드 패키지만 표시됩니다. 레지스트리에서 받은 패키지는 문서 생성 대상으로 표시되지 않습니다.

패키지를 선택하면 표시 이름과 설명이 함께 표시됩니다. 이 정보는 생성되는 DocFX 설정의 앱 이름과 제목에 반영됩니다.

## 문서 옵션

| 옵션 | 설명 |
| --- | --- |
| `Include Changelog` | 변경 이력 문서 포함 여부를 나타내는 옵션입니다. |

## 생성 결과

문서 생성 버튼을 누르면 선택 패키지의 프로젝트 루트 아래에 `Documentation` 폴더가 생성됩니다.

```text
Documentation/
  GenerateDocWebsite.sh
  docfx/
    docfx.json
    index.md
    manual/
    changelog/
    images/
```

생성 과정에서 `Documentation/.gitignore`에는 DocFX 산출물인 `docfx/_site/`와 `docfx/api/`가 추가됩니다.

## 문서 빌드

문서 프로젝트가 생성된 뒤에는 터미널에서 다음 명령으로 사이트를 생성할 수 있습니다.

```bash
cd Documentation
./GenerateDocWebsite.sh
```

빌드 결과는 `Documentation/docfx/_site`에 생성됩니다.

로컬에서 바로 확인하려면 `--serve` 옵션을 사용합니다.

```bash
cd Documentation
./GenerateDocWebsite.sh --serve
```

## 주의사항

- 문서 생성 대상 패키지는 현재 프로젝트에서 로컬 또는 임베디드 패키지로 인식되어야 합니다.
- 패키지를 방금 생성했다면 Unity Package Manager가 패키지를 인식할 때까지 기다린 뒤 문서 생성을 진행합니다.
- 문서 빌드에는 .NET local tool 복원이 필요하므로 네트워크 또는 로컬 캐시 상태에 따라 시간이 걸릴 수 있습니다.
