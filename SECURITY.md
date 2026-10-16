# Security policy

## Reporting a vulnerability

Please report security issues privately through GitHub:
**[Report a vulnerability](https://github.com/muhib-karim/global360-qa-automation/security/advisories/new)**
(the repository's Security tab, "Report a vulnerability"). Do not open a public issue.

Include what you found, where (file, workflow or commit), and how to reproduce it. You will get an
acknowledgement through the advisory, and a fix or a decision will be shared there before any public disclosure.

## Scope

This repository contains test automation only. It targets the public Snipe-IT demo at
`https://demo.snipeitapp.com`, and the only credentials it contains are that demo's published admin account
(`admin` / `password`, shown on the demo's login page). A guard in `Config/TestSettings.cs` refuses any other
target.

In scope: secrets in the repository or its history, unsafe GitHub Actions configuration (permissions, untrusted
input, unpinned actions), and dependencies with known vulnerabilities.

Vulnerabilities in Snipe-IT itself should go to the Snipe-IT project, not here.

## Safeguards in place

- gitleaks scans the full git history on every pull request.
- CodeQL analyses the C# code on pull requests, on `main` and weekly.
- Dependabot proposes NuGet, GitHub Actions and Docker updates weekly.
- Workflows default to read-only permissions, pin actions by commit SHA and do not persist checkout credentials.
