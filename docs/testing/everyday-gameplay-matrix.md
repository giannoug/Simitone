# Everyday gameplay regression matrix

All cases are **NOT RUN**. No Windows runner or licensed game assets have been supplied. This checklist is not passing-test evidence.

| Case | Required check | Status |
|---|---|---|
| B01 | Initial residential load | NOT RUN |
| B02 | Change object and save | NOT RUN |
| B03 | Restart and compare household/cash/relationships/inventory/objects | NOT RUN |
| B04 | Repeat save/reload without duplication | NOT RUN |
| B05 | Load an older copied Simitone save | NOT RUN |
| B06 | Return/quit Yes/No/Cancel | NOT RUN |
| B07 | Denied save retains live lot; retry after recovery | NOT RUN |
| B08 | Block unsupported away-save actions | NOT RUN |
| H01 | Reference eviction finances and lot state | NOT RUN |
| H02 | Reference move-in charge and state | NOT RUN |
| H03 | Adult/child/pet household preservation | NOT RUN |
| H04 | Duplicate request does not duplicate credit | NOT RUN |
| H05 | Cancel leaves state unchanged | NOT RUN |
| H06 | Reject stale source and occupied destination | NOT RUN |
| H07 | Cleanup failure prevents later household mutation | NOT RUN |
| T01 | Community destinations only, no residential editor controls | NOT RUN |
| T02 | Cancel returns to usable original lot | NOT RUN |
| T03 | One response for double selection/cancellation | NOT RUN |
| T04 | Return to correct non-default neighborhood | NOT RUN |
| T05 | Single/group round trip preserves acquired state after restart | NOT RUN |
| T06 | Repeated trips and failed restore retain correct home snapshot | NOT RUN |
| O01 | Cancel/Escape changes no persisted or live settings | NOT RUN |
| O02 | Apply/restart exact settings; free will still persists | NOT RUN |
| O03 | Denied config write reports error and supports retry | NOT RUN |
| O04 | Old-window resize preserves pending display choice | NOT RUN |
| O05 | One modal, usable 800x600/150% DPI, no click-through | NOT RUN |
| O06 | No-sound and existing 3D remain usable | NOT RUN |

For every executed case, attach the precise revisions, edition, environment, starting values, expected result, actual result and evidence. See the approved subsystem plans for steps. Do not mark a manual case passed from static source inspection.

## Automated checkpoint (2026-10-24)

The package-free checks pass for config persistence/failure preservation, save eligibility, stale household operations, visit filtering/one-shot cancellation/origin actor, and options draft/rollback/restart display behavior. These checks bypass rendering and assets and **do not mark any manual case above as passed**. The Windows locked-file case remains pending on Linux. See [revision-pinned results](release-1-results.md) for exact coverage and unfinished work.
