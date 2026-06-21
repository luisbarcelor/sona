import { readdirSync, statSync } from 'node:fs'
import { join, relative } from 'node:path'

const srcRoot = new URL('../src', import.meta.url).pathname
const kebabCase = /^[a-z0-9]+(?:-[a-z0-9]+)*$/
const violations = []

function walk(directory) {
  for (const entry of readdirSync(directory)) {
    const absolutePath = join(directory, entry)
    const stats = statSync(absolutePath)

    if (stats.isDirectory()) {
      if (!kebabCase.test(entry)) {
        violations.push(relative(srcRoot, absolutePath))
      }

      walk(absolutePath)
      continue
    }

    if (entry.endsWith('.tsx') && entry !== 'main.tsx') {
      const fileName = entry.slice(0, -'.tsx'.length)

      if (!kebabCase.test(fileName)) {
        violations.push(relative(srcRoot, absolutePath))
      }
    }
  }
}

walk(srcRoot)

if (violations.length > 0) {
  console.error('Component filenames must use kebab-case:')
  for (const violation of violations) {
    console.error(`- ${violation}`)
  }
  process.exit(1)
}
