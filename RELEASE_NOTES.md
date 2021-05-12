#### 1.2.0- 2021-05-12
Tested with Parkitect 1.7t
* Add: Enable tool without GUI
   * Default Key: `Comma`
   * Changing between GUI & no GUI: with their shortcuts (defaults: `Period` & `Comma`)
   * Selection Tool can be changed with their shortcuts (defaults: Keypad 7-9, 3 (clear))
   * Note: There's a tiny `HS` in the upper right corner when Hide Scenery without GUI is enabled. Otherwise when no scenery to hide is selected & no tool active: looks just the same as Mod not active.

#### 1.1.0- 2021-05-09
Tested with Parkitect 1.7t
* Add tiny gui state

#### 1.0.4- 2020-12-12
Tested with Parkitect 1.7d
* Don't merge Harmony, but instead keep as extra dll. See #5

#### 1.0.3- 2020-12-09
Tested with Parkitect 1.7a
* Fix #3: Use AbstractMod for Multiplayer Update
* In Multiplayer: mod can be used, isn't required by all players
* Use AssemblyName instead of custom Identifier (are same)
* Update Version to `1.0.3`
* Update Harmony
* Update ILRepack

#### 1.0.1.1 - 2020-11-13
Tested with Parkitect 1.6c
* Recompile to work with Update 1.6

(No new version number: no code change was needed, just a recompile -- and I forgot to adjust the version number...)

#### 1.0.1 - 2020-08-01
Tested with Parkitect *1.5j*
* Fix: Cannot find `Input`
       -> was moved into `UnityEngine.InputLegacyModule`

#### 1.0.0 - 2020-06-21
Tested with Parkitect *1.5i*
* Fix: Objects don't turn transparent

#### 0.1.0 - 2019-01-13
Tested with Parkitect *1.2a*
* Initial release
