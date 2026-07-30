# Working on this ENCY extension

<!-- Generated from guides/_index.json in EncySoftware/ency-extension-mcp.
     Edit the guides there and run tools/sync-rules.ps1 - changes made here are overwritten. -->

This repo is one extension for **ENCY 3**. GitHub Actions builds it and the ENCY Extension Store
packs and publishes it when you push a version tag.

**Write for ENCY 3, not ENCY 2.** The SDK is pinned in `src/EncyExtension.csproj` as
`EncySoftware.CAMAPI.Sdk.Net` 3.0.1-rc.22 - a release candidate, because that is the only 3.x
published so far. Do not "fix" that by moving to a 2.x version: 2.x is the previous generation of the
product, and an extension built against it is an extension for the old ENCY. If the pinned rc is a
problem, say so instead of switching lines quietly. (An assistant that read the old instructions
spent an hour writing for ENCY 2 - hence this paragraph.)

**Decide which entry point you need BEFORE writing extension code, then read its guide.** Each guide
gives the exact interface, the `*.settings.json` key, a compiling skeleton and the traps. The guides
are plain markdown under `.cursor/rules/` - open them directly. If the `ency-extension-store` MCP
server is connected, `get_extension_guide` serves the same text (`type=list` lists every kind).

| What the extension should do | Kind | Guide |
|---|---|---|
| Add a button to the ENCY utilities menu or toolbar; runs on click with the full application context | `utility` | `.cursor/rules/type-utility.mdc` |
| Run code once when ENCY starts and once when it shuts down; no UI of its own | `global` | `.cursor/rules/type-global.mdc` |
| Intercept or wrap how another utility is executed - custom file selector, pre/post processing | `utility_runner` | `.cursor/rules/type-utility-runner.mdc` |
| Add items to the right-click menu of a technology operation in the operations tree | `operation_popup` | `.cursor/rules/type-operation-popup.mdc` |
| Add items to the right-click menu of a node in the 3D geometry model tree | `geom_model_node_popup` | `.cursor/rules/type-geom-node-popup.mdc` |
| Implement your own toolpath calculation algorithm for one or more operation types | `operation_solver` | `.cursor/rules/type-operation-solver.mdc` |
| Transform the calculated CLD toolpath data before it reaches the next pipeline stage | `cldata_converter` | `.cursor/rules/type-cldata-converter.mdc` |
| Integrate ENCY with a PLM system: browse items, download projects, upload results | `plm` | `.cursor/rules/type-plm.mdc` |

Two guides apply to every change:

- `.cursor/rules/ency-extension.mdc` - repo anatomy: the `CAMAPI.ExtensionFactory` contract,
  matching ids between `*.settings.json` and the factory, `package.info.json`, how to build for
  packing.
- `.cursor/rules/ency-cookbook.mdc` - COM lifetime (`ComWrapper`), errors through
  `TResultStatus`, asking the user for parameters, windows and STA rules.

## Publishing

```bash
git tag v1.2.3 && git push --tags
```

Actions builds the project, the store packs the ENCY-format package and publishes it. A brand new
extension waits for a store moderator (its direct card link works immediately); new versions of an
approved extension go live at once.