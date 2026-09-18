# Release checklist

1. Confirm the release branch contains one coherent phase and has a clean working tree.
2. Update the application version, README release notes and affected user documentation.
3. Run `./scripts/Verify.ps1` and confirm every check passes with zero build warnings.
4. Test changed workflows manually with generated files. For copy changes, cover Rename, Skip, Replace, Cancel and Pause/Resume.
5. Run `./publish.ps1`, then open the produced self-contained EXE on Windows.
6. Confirm `LICENSE`, `NOTICE` and `README.md` are present beside the executable.
7. Inspect the Git diff and merge into `main` only after functional review.
8. Create the GitHub release from the reviewed commit and attach the portable ZIP.

Never use personal photos as release fixtures or commit files from `artifacts`.
