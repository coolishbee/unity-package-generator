# GitHub Pages 배포

이 문서는 `Documentation/docfx/_site`에 생성된 정적 사이트를 GitHub Pages에 배포하는 기본 흐름을 설명합니다.

## 사전 준비

- 저장소에 `Documentation` 폴더가 포함되어 있어야 합니다.
- 로컬 또는 CI 환경에서 .NET local tool을 복원할 수 있어야 합니다.
- GitHub 저장소의 Pages 기능을 사용할 수 있어야 합니다.

## 로컬 빌드 확인

배포 전에 로컬에서 문서 사이트가 정상 생성되는지 확인합니다.

```bash
cd Documentation
./GenerateDocWebsite.sh --serve
```

브라우저에서 로컬 미리보기가 정상적으로 보이면 배포 준비가 된 상태입니다.

## 배포용 빌드

GitHub Pages의 공개 주소를 첫 번째 인자로 전달하면 `docfx/index.md`의 `redirect_url`이 해당 경로에 맞게 갱신됩니다.

```bash
cd Documentation
./GenerateDocWebsite.sh "https://<owner>.github.io/<repository>"
```

빌드 결과는 다음 경로에 생성됩니다.

```text
Documentation/docfx/_site
```

## GitHub Pages 설정

GitHub Pages는 정적 파일을 호스팅하므로 `Documentation/docfx/_site`의 내용을 Pages 배포 대상으로 사용합니다.

대표적인 방식은 다음 중 하나입니다.

1. GitHub Actions에서 `Documentation/docfx/_site`를 Pages artifact로 업로드합니다.
2. 별도 배포 브랜치를 사용하는 경우 `_site` 내용을 해당 브랜치 루트로 복사해 배포합니다.

프로젝트마다 배포 방식은 다를 수 있지만, 최종 호스팅 대상은 항상 `Documentation/docfx/_site`의 내용입니다.

## 확인 사항

- 배포 URL을 인자로 넘긴 경우 첫 페이지 redirect가 올바른지 확인합니다.
- `docfx/_site`, `docfx/api`는 생성 산출물이므로 일반적으로 Git에 커밋하지 않습니다.
- Pages 설정에서 공개 경로가 저장소명 하위 경로인지 루트 도메인인지 확인합니다.
