## Access Levels ##

Access level can be any integer from 0 through 255 and is generally left up to applications to determine what individual access levels mean. Here are some important values used by SimianGridFrontend and OpenSim:

  * **0**: Anonymous or unverified user, such as an incoming HyperGrid connection
  * **1**: Verified account created through a normal user creation process
  * **200+**: Administrator-level account that has "God mode" access in OpenSim and administrator access in SimianGridFrontend
  * **255**: Usually reserved for a single account that is the grid administrator. This account will be returned in queries for a UserID of 00000000-0000-0000-0000-000000000000