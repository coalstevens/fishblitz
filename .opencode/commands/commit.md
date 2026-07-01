---
description: Stage all, suggest a commit message, push to fishblitz
---

1. Run `git add .`
2. Read `git diff --cached --stat` and `git diff --cached` to review staged changes
3. Generate 2-3 commit message options in the project's style (imperative mood, no period), numbered:
   - Option 1: `feat: ...`
   - Option 2: `fix: ...`
   - Option 3: `chore: ...`
4. Ask the user: "Pick an option (1/2/3), type a custom message, or ask for a revision (e.g., 'focus on the X part')"
5. If they pick a number → use that option
6. If they type a custom message → use it
7. If they request a revision → revise and ask again (loop back to step 4)
8. Once finalized, run `git commit -m "<message>" && git push fishblitz`
