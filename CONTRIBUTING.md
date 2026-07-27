# Contributing to FusionOS

Thank you for your interest in contributing to **FusionOS**! We welcome contributions from developers of all skill levels to help build the world's leading open-source enterprise ERP platform.

---

## Code of Conduct

By participating in this project, you agree to abide by our [Code of Conduct](CODE_OF_CONDUCT.md). Please report any unacceptable behavior to `conduct@fusionos.org`.

---

## Development Workflow

### 1. Fork & Clone
1. Fork the repository on GitHub: [https://github.com/worksaransh/FusionOS](https://github.com/worksaransh/FusionOS)
2. Clone your fork locally:
   ```bash
   git clone https://github.com/YOUR_USERNAME/FusionOS.git
   cd FusionOS
   ```

### 2. Branch Strategy
Always create a feature branch off `main`:
```bash
git checkout -b feature/your-feature-name
# or
git checkout -b fix/your-bug-fix
```

### 3. Local Setup
Follow the instructions in the [Installation Guide](README.md#installation-guide) to spin up the local environment with Docker Compose or standalone services:
```bash
# Frontend setup
cd frontend
npm install
npm run dev

# Backend setup (.NET 8)
cd ../backend
dotnet restore
dotnet build
```

### 4. Commit Guidelines
We enforce [Conventional Commits](https://www.conventionalcommits.org/):
- `feat`: A new feature
- `fix`: A bug fix
- `docs`: Documentation only changes
- `style`: Formatting, missing semi-colons, etc.
- `refactor`: A code change that neither fixes a bug nor adds a feature
- `test`: Adding missing tests or correcting existing tests
- `chore`: Infrastructure, dependencies, or build scripts

*Example:*
```bash
git commit -m "feat(inventory): add batch serial number tracking API endpoint"
```

---

## Pull Request Process

1. **Keep PRs Focused**: Address one problem or feature per Pull Request.
2. **Include Tests**: Add unit or integration tests for new business logic.
3. **Verify Build**: Ensure local builds and linters pass:
   ```bash
   cd frontend && npm run build && npm test
   ```
4. **Submit PR**: Push your branch to GitHub and open a Pull Request targeting `main`.
5. **Review**: Maintainers will review your PR and provide feedback.

---

## Architecture Standards

Before making architectural modifications, please consult:
- `docs/blueprint/00_MASTER_CONTEXT.md`
- `docs/blueprint/01_ARCHITECTURE.md`
- `docs/blueprint/02_CODING_STANDARDS.md`

Thank you for contributing to FusionOS!
