# GitLab Pages 배포

이 문서는 `Documentation/docfx/_site`에 생성된 정적 사이트를 GitLab Pages에 배포하는 기본 흐름을 설명합니다.

## 사전 준비

- 저장소에 `Documentation` 폴더가 포함되어 있어야 합니다.
- GitLab CI 환경에서 .NET local tool을 복원할 수 있어야 합니다.
- GitLab 프로젝트에서 Pages 기능을 사용할 수 있어야 합니다.

## 로컬 빌드 확인

배포 전에 로컬에서 문서 사이트가 정상 생성되는지 확인합니다.

```bash
cd Documentation
./GenerateDocWebsite.sh --serve
```

브라우저에서 로컬 미리보기가 정상적으로 보이면 배포 준비가 된 상태입니다.

## 배포용 빌드

GitLab Pages의 공개 주소를 첫 번째 인자로 전달하면 `docfx/index.md`의 `redirect_url`이 해당 경로에 맞게 갱신됩니다.

```bash
cd Documentation
./GenerateDocWebsite.sh "https://<namespace>.gitlab.io/<project>"
```

빌드 결과는 다음 경로에 생성됩니다.

```text
Documentation/docfx/_site
```

## GitLab Pages 설정

GitLab Pages는 기본적으로 `public` 디렉토리를 Pages artifact로 사용합니다. CI에서 문서를 빌드한 뒤 `Documentation/docfx/_site`의 내용을 `public`으로 복사해 배포합니다.

기본 흐름은 다음과 같습니다.

1. CI에서 `Documentation` 경로로 이동합니다.
2. `./GenerateDocWebsite.sh "https://<namespace>.gitlab.io/<project>"`를 실행합니다.
3. 생성된 `docfx/_site` 내용을 CI 작업의 `public` 폴더로 복사합니다.
4. `public` 폴더를 Pages artifact로 업로드합니다.

## 확인 사항

- 배포 URL을 인자로 넘긴 경우 첫 페이지 redirect가 올바른지 확인합니다.
- `docfx/_site`, `docfx/api`는 생성 산출물이므로 일반적으로 Git에 커밋하지 않습니다.
- GitLab Pages 주소가 그룹, 하위 그룹, 프로젝트 경로를 포함하는지 확인합니다.
- 프로젝트가 비공개인 경우 Pages 접근 권한 설정을 확인합니다.
