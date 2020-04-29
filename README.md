# net-regex-experiment

Greedy regex-like programming puzzle

Started out as an example for a solution to a programming puzzle, that asked for:

- only characters `a-z`
- full string matching
- `.` as a wildcard for any of the supported character
- `*` as a quantifier indicating 0 or more characters

Slowly added some more regex features, like:

- Other quantifiers
  - `+` for 1 or more
  - `?` for 0 or one
  - `{n}` for exactly n
  - `{n,}` for at least n
  - `{m,n}` for at least m and at most n
  
- Backtracking if there is no match
  - So, `a*ab` will match `aaaab`, where before it would fail before because `a*` captured all the `a`'s.

