# 소개

## 개요

`UPM Package Generator`는 Unity 에디터에서 UPM 패키지를 빠르게 만들기 위한 에디터 도구입니다. 새 패키지의 기본 구조를 생성하고, 패키지 문서 프로젝트와 UPM 샘플 등록까지 하나의 창에서 처리할 수 있습니다.

도구는 Unity 상단 메뉴의 `Tools/UPM Package Generator`에서 실행합니다.

## 패키지 정보

| 항목 | 값 |
| --- | --- |
| 패키지 이름 | `com.coolishbee.package-generator` |
| 표시 이름 | `UPM Package Generator` |
| Unity 버전 | `6000.2` 이상 |
| 기준 에디터 버전 | `6000.2.15f1` |

## 주요 기능

1. UPM 패키지 기본 구조 생성
2. Runtime, Editor, Tests 어셈블리 정의 파일 생성
3. 외부 위치 패키지의 `Packages/manifest.json` 등록
4. `Documentation/docfx` 문서 프로젝트 생성
5. UPM 표준 `Samples~` 샘플 생성 및 `package.json` 등록

## 사용해야 하는 이유

UPM 패키지를 직접 만들 때는 `package.json`, 어셈블리 정의 파일, 테스트 설정, 샘플 등록, 문서 구조를 각각 맞춰야 합니다. 이 도구를 사용하면 반복 작업을 줄이고, 패키지마다 일관된 구조를 유지할 수 있습니다.
