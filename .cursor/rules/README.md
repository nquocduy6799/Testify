# Testify Project Rules

This folder contains Cursor AI rules that enforce coding standards for the Testify team project.

## Rules

### `testify-team-standards.mdc`

**Always applies** - Core standards for all team members:
- Project structure guidelines
- Service layer separation (Server vs Client)
- Feature-based organization
- Clean code principles
- What to do / not to do

## How It Works

Cursor AI will automatically reference these rules when:
- Making code changes
- Creating new files
- Refactoring code
- Answering questions about the project

## For Team Members

**Before starting work:**
1. Read `testify-team-standards.mdc` to understand the project structure
2. Ask Cursor AI questions like:
   - "Where should I create a new notification component?"
   - "How do I add a new API endpoint?"
   - "What's the correct service pattern for this feature?"

**Cursor AI will follow these rules automatically** and remind you of best practices!

## Need to Update Rules?

If project standards change, update the `.mdc` files in this folder.
Rules take effect immediately - no restart needed.
