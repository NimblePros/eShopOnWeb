#!/usr/bin/env bash
# Usage: check-test-deletions.sh <base-sha> <head-sha>
# Fails if the range deletes a test file without a "DELETE_TESTS: <reason>" commit message.
set -euo pipefail

base="${1:?base sha required}"
head="${2:?head sha required}"

# Test files in this repo are all C# files under tests/ (no *Tests.cs naming convention).
is_test_file() { [[ "$1" == tests/*.cs ]]; }

# Capture git output in variables: an assignment propagates git's failure to set -e, a process substitution would not.
changes=$(git -c core.quotepath=off diff --name-status -M --diff-filter=DR "$base...$head")
messages=$(git log --format=%B "$base..$head")

deleted=()
while IFS=$'\t' read -r status old new; do
  if [[ "$status" == D ]] && is_test_file "$old"; then
    deleted+=("$old")
  # A rename out of tests/ removes a test file from the suite.
  elif [[ "$status" == R* ]] && is_test_file "$old" && ! is_test_file "$new"; then
    deleted+=("$old")
  fi
done <<< "$changes"

if [[ ${#deleted[@]} -eq 0 ]]; then
  echo "No test files deleted."
  exit 0
fi

if grep -Eq 'DELETE_TESTS:[[:space:]]*[^[:space:]]' <<< "$messages"; then
  echo "Test deletions allowed by a DELETE_TESTS commit message:"
  printf '  %s\n' "${deleted[@]}"
  exit 0
fi

echo "::error::Test files were deleted without a 'DELETE_TESTS: <reason>' commit message."
printf '  %s\n' "${deleted[@]}"
exit 1
