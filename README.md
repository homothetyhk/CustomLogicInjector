# CustomLogicInjector

CustomLogicInjector is an add-on to Randomizer 4 which allows editing randomizer logic. The source logic can be found at https://github.com/homothetyhk/RandomizerMod/tree/master/RandomizerMod/Resources/Logic and important information about logic is provided through the readme at https://github.com/homothetyhk/RandomizerMod/blob/master/LOGIC_README.md

# Examples

This mod should have been distributed along with an Examples folder. To use the examples, simply move the contents of the Examples folder up one level (in other words, the folders inside Examples should be moved so they are next to the mod dll).

# Usage

- CustomLogicInjector partitions user input into packs, which each receive a submenu with various features. These include:
  - The ability to control whether the logic edits of the pack are applied, through the "Enabled" setting.
  - The ability to toggle settings defined by the pack. Note that these are only relevant if the pack is Enabled.
- To add a Custom Logic Pack, create a directory next to CustomLogicInjector.dll, and create the "pack.json" file within that directory. This file provides basic information about the pack and is used to construct its submenu. The "pack.json" file should be a json object with the following properties:
  - Name: this is the text displayed in the menu. This should be unique (compared to any other custom logic packs in use) as it will be used to identify the toggle setting.
  - Files: this should be a json array with an element for each other file associated to the pack (more detail below). Each element should be a json object with the following properties:
    - FileName: the name of the file, including extension.
    - Priority: this determines the order in which logic files are applied, with lower priorities applied earlier. A general rule of thumb is that MacroEdits should be applied before LogicSubsts which should be applied before LogicEdits, and so the priority values of -2, -1, and 0 are respectively recommended for those formats, unless special circumstances call for other values.
    - JsonType: the format and type of the file.
  - Settings: this should be a json array with entries corresponding to each setting defined by the pack. Each entry should be a json object with the following properties:
    - MenuName: the name of the setting to display on its toggle in the menu.
    - LogicName: the name of the setting to allocate as a term for use in logic.

## JsonType

The json formats supported by CustomLogicInjector are defined in RandomizerCore.Logic.LogicManagerBuilder.JsonType. The most important formats are as follows:
- "MacroEdit"
  - Requires a json file in Dictionary<string, string> format (see: macros.json in the randomizer). Keys correspond to names of existing or new macros, and values are strings of logic.
  - If the name does not correspond to an existing macro, defines a new macro which corresponds to the logic string.
    - This operation does not support the **ORIG** token.
    - The new macro should not shadow any existing terms,
  - If the name corresponds to an existing macro, replaces the value of the macro with the new logic string.
    - This operation supports the **ORIG** token. All occurences of **ORIG** in the new logic string will be replaced by the old value.
- "LogicSubst"
  - Requires a json file in List<RawSubstDef> format. This means a json array of json objects, each with the properties "name", "old", and "replacement".
    - The "name" property determines the name of the logic to modify. This can be the name of a macro, location, waypoint, or transition.
    - The "old" property determines the token to replace. This should be a single token; in other words, **SIMPLE**, **SIMPLE>3** or **SIMPLE4** are valid tokens to replace, but **FULLCLAW | WINGS** is not a single token.
    - The "replacement" property determines what is put in place of the old token. This can be any logic sentence.
  - This operation does not support the **ORIG** token.
- "LogicEdit"
  - Requires a json file in List<RawLogicDef> format (see: locations.json in the randomizer). This means a json array of json objects, each with the properties "name" and "logic".
    - The "name" property determines the name of the logic to modify. This can be the name of a location, waypoint, or transition, but not the name of a macro.
  - If the name does not correspond to an existing logic def, defines a new logic def which corresponds to the logic string.
    - This operation does not support the **ORIG** token.
  - If the name corresponds to an existing logic def, replaces the value of the logic def with the new logic string.
    - This operation supports the **ORIG** token. All occurences of **ORIG** in the new logic string will be replaced by the old value.
    
As a general note, multiple files with any combination of the formats are accepted.
- In summary, a typical pack has:
  - A pack.json file, with the information about the pack.
  - A macros.json file, which modifies existing macros and/or defines new macros.
  - A subst.json file, which does fine-tuned substitutions on existing macros and logic.
  - A logic.json file, which does general logic edits.