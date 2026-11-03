# RidingCrop - test coverage

**Automated** = covered by a test that ran green; **Manual** = needs a person; **Gap** = not covered. Never mark an untested behaviour as covered.

| Feature / requirement | Unit | Integration | TestLab | Manual | Status |
|---|---|---|---|---|---|
| Loads with exact KydlCore, no errors | - | ManifestTests, ArchitectureRulesTests | selftest | - | Automated |
| Seven crops exist with recipes and stations | CropTableTests | - | all-7-crops, recipes-stations | - | Automated |
| Damage / bleed / durability ladder | CropBalanceTests | - | damage-ladder | - | Automated |
| RidingCrop skill | - | - | skills | - | Automated |
| Give, equip, craft a crop | - | - | give-and-equip, craft-a-crop | - | Automated |
| KY hub tab, feature switches | FunctionsTests | - | hub-page, feature-toggle | hub-look | Automated + Manual |
| Settings layout, patch target, suite rules | - | SettingsTests, PatchTargetsTests, SuiteRulesTests | - | - | Automated |
| Sound maths (lead-in cut, gain, limiter) | SoundMathTests | - | - | - | Automated |
| Whoosh and slap heard, slap once per swing | - | - | - | swing-feel | Gap |
| Crop model, grip and icon look | - | - | - | swing-feel | Gap |
| Bleed ticks, no stacking, kill credit | - | - | - | swing-feel | Gap |
| Multiplayer (EveryoneMustHaveMod, virtual peer) | - | - | - | 2-player test | Gap |