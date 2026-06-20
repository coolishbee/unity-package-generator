#!/bin/bash

set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$script_dir"

dotnet tool restore

update_redirect_url() {
  local url="$1"
  local url_without_scheme
  local url_host
  local url_path
  local parsed_path
  local redirect_url
  local index_path="docfx/index.md"
  local temp_path
  local updated="false"

  case "$url" in
    http://*)
      url_without_scheme="${url#http://}"
      ;;
    https://*)
      url_without_scheme="${url#https://}"
      ;;
    *)
      echo "Invalid redirect URL: $url. Expected http:// or https:// URL" >&2
      exit 1
      ;;
  esac

  url_without_scheme="${url_without_scheme%%\?*}"
  url_without_scheme="${url_without_scheme%%#*}"

  if [ -z "$url_without_scheme" ] || [ "$url_without_scheme" = "$url" ]; then
    echo "Invalid redirect URL: $url" >&2
    exit 1
  fi

  case "$url_without_scheme" in
    */*)
      url_host="${url_without_scheme%%/*}"
      url_path="/${url_without_scheme#*/}"
      ;;
    *)
      url_host="$url_without_scheme"
      url_path=""
      ;;
  esac

  if [ -z "$url_host" ]; then
    echo "Invalid redirect URL: $url" >&2
    exit 1
  fi

  parsed_path="${url_path%%\?*}"
  parsed_path="${parsed_path%%#*}"
  parsed_path="${parsed_path%/}"

  if [ ! -f "$index_path" ]; then
    echo "$index_path not found" >&2
    exit 1
  fi

  redirect_url="$parsed_path/manual/introduction.html"
  temp_path="$(mktemp)"

  while IFS= read -r line || [ -n "$line" ]; do
    if [[ "$line" == redirect_url:* ]]; then
      printf 'redirect_url: %s\n' "$redirect_url" >> "$temp_path"
      updated="true"
    else
      printf '%s\n' "$line" >> "$temp_path"
    fi
  done < "$index_path"

  if [ "$updated" != "true" ]; then
    rm -f "$temp_path"
    echo "redirect_url field not found in $index_path" >&2
    exit 1
  fi

  mv "$temp_path" "$index_path"

  echo "Updated redirect_url: $redirect_url"
}

error_exit() {
  echo "*** ERROR ***"
  echo "An error occurred. Website was not generated."
  exit 1
}

remove_empty_root_toc_entry() {
  local toc_path="docfx/manual/toc.yml"
  local temp_path

  if [ ! -f "$toc_path" ]; then
    echo "$toc_path not found" >&2
    exit 1
  fi

  temp_path="$(mktemp)"

  while IFS= read -r line || [ -n "$line" ]; do
    if [ "$line" = "- name: Manual" ]; then
      continue
    fi

    printf '%s\n' "$line" >> "$temp_path"
  done < "$toc_path"

  mv "$temp_path" "$toc_path"
}

if [ "$#" -gt 0 ]; then
  case "$1" in
    http://*|https://*)
      echo "Docfx Redirect Url Update"
      update_redirect_url "$1"
      shift
      ;;
  esac
fi

echo "Clean up previous generated contents"
rm -rf docfx/_site
rm -rf docfx/api

# **** Check the docs folder. On errors, quit processing ****
echo "Checking references and attachments"
if ! dotnet tool run doclinkchecker -- -d ./docfx; then
  error_exit
fi

# **** Generate the table of contents ****
echo "Generating table of contents for Manual"
if ! dotnet tool run docfxtocgenerator -- -d ./docfx/manual -sr --indexing Never --folderRef None; then
  error_exit
fi
remove_empty_root_toc_entry

echo "Generate DocFX website"
dotnet tool run docfx -- ./docfx/docfx.json "$@"
