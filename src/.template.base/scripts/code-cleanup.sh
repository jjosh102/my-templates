#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd -- "$SCRIPT_DIR/.." && pwd)"

SOLUTION="$REPO_ROOT/MyToolkit.sln"
FULL_CLEANUP=0
VERIFY_ONLY=0

print_usage() {
  cat <<EOF
Usage: $(basename "$0") [OPTIONS]

Options:
  -s, --solution <path>  Path to the solution file
                          default: $SOLUTION
  -f, --full             Run full cleanup with JetBrains ReSharper Global Tools
  -v, --verify           Verify formatting without changing files
  -h, --help             Show this help message
EOF
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    -s|--solution)
      SOLUTION="$2"
      shift 2
      ;;
    -f|--full)
      FULL_CLEANUP=1
      shift
      ;;
    -v|--verify|--verify-only|-verify-only)
      VERIFY_ONLY=1
      shift
      ;;
    -h|--help)
      print_usage
      exit 0
      ;;
    *)
      echo "Error: Unknown option: $1" >&2
      print_usage >&2
      exit 1
      ;;
  esac
done

if [[ ! -f "$SOLUTION" ]]; then
  echo "Error: Solution file not found: $SOLUTION" >&2
  exit 1
fi

BLUE='\033[1;34m'
BOLD='\033[1m'
GREEN='\033[1;32m'
RED='\033[1;31m'
GRAY='\033[0;90m'
NC='\033[0m'

step() {
  echo -e "\n${BLUE}==>${NC} ${BOLD}$1${NC}"
}

ok() {
  echo -e "    ${GREEN}OK${NC} $1"
}

fail() {
  echo -e "    ${RED}FAIL${NC} $1" >&2
}

run() {
  echo -e "    ${GRAY}$*${NC}"
  "$@"
}

ensure_jetbrains_tool() {
  step "Checking JetBrains ReSharper Global Tools"

  cd "$REPO_ROOT"

  if [[ ! -f "$REPO_ROOT/.config/dotnet-tools.json" && ! -f "$REPO_ROOT/dotnet-tools.json" ]]; then
    echo "    Creating local tool manifest..."
    run dotnet new tool-manifest
    ok "Created local tool manifest"
  else
    ok "Local tool manifest found"
  fi

  if ! dotnet tool list --local 2>/dev/null | grep -q "jetbrains\.resharper\.globaltools"; then
    echo "    Installing JetBrains.ReSharper.GlobalTools locally..."
    run dotnet tool install JetBrains.ReSharper.GlobalTools --local
    ok "Installed JetBrains.ReSharper.GlobalTools"
  else
    ok "JetBrains.ReSharper.GlobalTools already installed"
  fi

  echo "    Restoring local .NET tools..."
  run dotnet tool restore
}

run_dotnet_format() {
  step "Formatting whitespace, indentation, and line endings"
  run dotnet format whitespace "$SOLUTION" \
    --no-restore \
    --verbosity minimal

  step "Removing unused using directives and applying selected safe .NET style fixes"
  run dotnet format style "$SOLUTION" \
    --no-restore \
    --diagnostics IDE0005 \
    --severity info \
    --verbosity minimal
}

refresh_git_index() {
  if ! command -v git >/dev/null 2>&1; then
    return
  fi

  if ! git -C "$REPO_ROOT" rev-parse --is-inside-work-tree >/dev/null 2>&1; then
    return
  fi

  step "Refreshing Git index stat cache"

  if git -C "$REPO_ROOT" update-index --refresh -- >/dev/null 2>&1; then
    ok "Git index refreshed"
  else
    ok "Git index refreshed; real file changes may remain"
  fi
}

SECONDS=0

step "Starting .NET Code Cleanup"
echo "    Solution: $SOLUTION"

if (( VERIFY_ONLY )); then
  echo "    Mode:     Verify only"
elif (( FULL_CLEANUP )); then
  echo "    Mode:     Full cleanup with JetBrains, then dotnet format"
else
  echo "    Mode:     Standard cleanup"
fi

step "Restoring NuGet packages"
run dotnet restore "$SOLUTION"

if (( VERIFY_ONLY )); then
  step "Checking formatting without changing files"
  run dotnet format "$SOLUTION" \
    --no-restore \
    --verify-no-changes \
    --exclude-diagnostics IDE1006 \
    --verbosity minimal
else
  if (( FULL_CLEANUP )); then
    ensure_jetbrains_tool

    step "Running JetBrains CleanupCode"
    cd "$REPO_ROOT"
    run dotnet tool run jb -- cleanupcode "$SOLUTION" --verbosity=WARN
  fi

  run_dotnet_format
  refresh_git_index
fi

DURATION_FORMATTED=$(printf "%02d:%02d" $((SECONDS / 60)) $((SECONDS % 60)))

echo -e "\n${GREEN}==>${NC} ${BOLD}Cleanup Complete${NC} (Took $DURATION_FORMATTED)"
