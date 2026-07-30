# ency-extension-template

Template for an [ENCY](https://encycam.com) extension that **publishes itself to the
[ENCY Extension Store](https://dmc.encycam.com/store) on every version tag**. Write code,
push a tag — the extension appears in the store. No files copied or uploaded by hand.

## Quick start

1. **Use this template** (GitHub button) — name the repository after your extension, e.g.
   `PocketMill`. The first push renames the placeholder inside `src/` to match, on its own, so
   `git clone` gets you a project already called by your name.
2. Let this repository publish under that name — once, and **nothing goes into GitHub**: open
   [the store](https://dmc.encycam.com/store) → **My published** → *Connect a GitHub repository*.
   Signing in to the store is the proof it is you; no token is created, so there is no secret to
   store, rotate or leak.

   Every publish after that — the first one included — authenticates with the workflow's own GitHub
   OIDC token, which GitHub issues per run and which expires on its own.
3. Write your code in `src/` (start at `Extension.cs`), fill `src/readme.md` (it becomes the
   store card README) and `description`/`author` in `src/package.info.json`.
4. Publish — no commands needed: open **Actions → publish-to-ency-store → Run workflow** and press
   the button with the fields empty. It works out the next version, tags the commit, builds, packs
   and publishes; the card link is in the job summary.

   From a terminal instead: `git tag v0.1.0 && git push --tags`.

   Skipped step 2? The run stops and tells you so, with a ready link to the connect form — press
   Connect there and re-run the job.

## Rather stay in the console?

Optional, and only worth it if you also want your editor's assistant to do this for you. Needs the
.NET 8 SDK:

```bash
dotnet tool install -g EncySoftware.ExtensionStoreMcp
ency-extension-mcp setup
ency-extension-mcp claim MyCoolExtension owner/MyCoolExtension   # same as step 2, from a terminal
```

`setup` signs you in to the store (your licsys account; only a refresh token is kept on your
machine) and, in Cursor or Claude Code, registers the MCP server. Add `--no-login` to skip the
sign-in. With that in place you can skip the steps above and simply ask: *"create an ENCY extension
called MyCoolExtension"*, then *"publish it as 0.1.0"*.

## Local build & try-out

```powershell
dotnet build src -c Release -t:PackReady   # flat, pack-ready output in src\bin\Release
```

(`-t:PackReady` = normal build + sweeps the SDK doc-xmls out of the output; see the csproj.)

To try the built extension in a local ENCY before publishing: install it via the store card
after a publish (new extensions are reachable by direct link right away), or — if you have the
ENCY pack CLI — pack `src\bin\Release` yourself and use file install. CI needs no packing tool:
the store backend packs the uploaded build output itself.

## Layout

| Path | Purpose |
|---|---|
| `src/Extension.cs` | your extension logic (`IExtensionUtility.Run`) |
| `src/ExtensionFactory.cs` | entry point ENCY looks for (`CAMAPI.ExtensionFactory`) — keep the class/namespace |
| `src/<YourName>.settings.json` | declares the extensions of this dll for ENCY (ids must match the factory) |
| `src/package.info.json` | store metadata: packageId, version, `tags` (keep the `ency-extension` marker!), sdkVersion |
| `src/readme.md` | store card README |
| `src/screenshots/` | PNG/JPG pictures of your extension — they become the card's screenshots, and the first one becomes its cover |
| `.github/workflows/publish.yml` | tag → build → pack → publish |

## Rules worth knowing

- The `ency-extension` tag in `package.info.json` is the **store marker** — remove it and the
  catalog will not index your package (the publish step fails fast on this).
- The tag `vX.Y.Z` overrides the version in `package.info.json` at publish time.
- `sdkVersion` controls the "minimal ENCY version" hint on the card — bump it when you bump
  the `EncySoftware.CAMAPI.Sdk.Net` package.
- The publisher (token owner) becomes the extension owner in the store.
