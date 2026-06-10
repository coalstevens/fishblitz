---
description: Stage all, suggest a commit message, push to fishblitz
---

1. Run `git add .`
2. Read `git diff --cached --stat` and `git diff --cached` to review staged changes
3. Generate a concise commit message in the project's style (imperative mood, no period) covering the key changes
4. Ask the user: "Commit with: **`<suggested message>`**? (yes/no)"
5. If **yes** → run `git commit -m "<message>" && git push fishblitz`
6. If **no** → ask the user for their preferred message, then commit and push with that
