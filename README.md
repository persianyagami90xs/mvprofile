# mvprofile
A tool to rename the user profile folder in Windows 10 (and maybe other versions).

Renaming the user profile folder in Windows is not just a simple matter of renaming the folder, or even renaming the folder and updating the registry entry that specifies the folder for the specific user. The user's folder will have been written as an absolute, expanded path in registry keys for numerous application and system configurations (install paths, file histories, etc).

This tool will load the registry hive from a given user profile folder, search for that user folder in all keys and values, and try to update all occurences to be the new folder before unloading the hive and renaming the folder.

Even running as administrator, access to some keys may be denied. It might be posible to run this tool successfully as the System user, but that has not been tested.

This tool successfully cleaned up a user profile that was partly changed by hand, but has not been thoroughly tested on multiple profiles or systems. This code is made available not as a guaranteed tool to rename a user profile, but as an example of what needs to be done in order to find and replace the user profile folder string. If using this tool, make a system backup first.

## Usage

```
mvprofile.exe \<old path> \<new path>
```
Where `old path` and `new path` must be absolute paths to the original and desired folder names.