#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
scan_roots=(src tests samples probes tools benchmarks playground)
existing_roots=()
for root in "${scan_roots[@]}"; do
    if [[ -d "$repo_root/$root" ]]; then
        existing_roots+=("$root")
    fi
done

if ((${#existing_roots[@]} == 0)); then
    printf 'system-math: no first-party source roots found\n'
    exit 0
fi

# No provider files are allowlisted. Generated output is excluded only through
# the obj/bin path exclusions below.
if rg -n --glob '*.cs' --glob '!**/bin/**' --glob '!**/obj/**' \
    'System\.Math(F)?\.|(^|[^[:alnum:]_])Math(F)?\.|(^|[^[:alnum:]_])DeltaMaths\.|(^|[^[:alnum:]_])maths\.|(^|[^[:alnum:]_])Maths\.(abs|asin|atan2|cos|sin|radians|fma|sign|max|min|clamp|floor|ceil|sqrt|acos|tan|round)\b|using Maths = Delta\.Maths\.maths|Delta\.Maths\.maths|using Delta\.Maths;|global::Delta\.Maths|Delta\.Maths\.' \
    "${existing_roots[@]}"; then
    printf 'system-math: legacy math spelling found; use using Delta; with Maths.* calls\n' >&2
    exit 1
fi

printf 'system-math: no direct System.Math/MathF calls\n'
