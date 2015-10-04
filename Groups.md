# Group Data #

Agent
  * Currently Active Group [could be kept in Generics or made a part of the User Info](this.md)

Group
  * GroupID
  * Name
  * ExtraData (json/osdmap format)
    * Charter (string)
    * InsigniaID (uuid)
    * FounderID (uuid)
    * MembershipFee (int)
    * OpenEnrollment (string)
    * ShowInList (bool)
    * AllowPublish (bool)
    * MaturePublish (bool)
    * OwnerRoleID (uuid) [Can't decide if this should be in extra data]
    * EveryoneRoleID (uuid) [Can't decide if this should be in extra data]

Group Role
  * GroupID
  * GroupRoleID
  * Name
  * ExtraData (json/osdmap format)
    * Description (string) [Can't decide if this should be in extra data]
    * Title (string) [Can't decide if this should be in extra data]

Permissions
  * PermissionID
  * Name
  * ExtraData (json/osdmap format)
    * LLGroupPermissionMask (long)

Permission
  * GroupRoleID
  * PermissionID

Group Role Member
  * GroupRoleID
  * MemberID

Group Member
  * GroupID
  * MemberID
  * SelectedRoleID [Can't decide if this should be in extra data]
  * ExtraData (json/osdmap)
    * Contribution (int)
    * ListInProfile (bool)
    * AcceptNotices (bool)

Group Notice
  * GroupID
  * NoticeID
  * ExtraData
    * Timestamp (int) [timestamp](unix.md)
    * FromName
    * Subject
    * Message
    * BinaryBucket

Group Invite
  * InviteID
  * GroupID
  * GroupRoleID
  * MemberID
  * TMStamp (int) [timestamp](unix.md)

# Group API #

  * Get/Add Group (GroupID)
  * Get/Add Group Member ([GroupID](GroupID.md), [MemberID](MemberID.md))
  * Get/Add Group Role ([GroupID](GroupID.md),[RoleID](RoleID.md))
  * Get/Add Group Permission (PermissionID)
  * Get/Add Group Role Permission (RoleID)
  * Get/Add Group Role Member (RoleID)



# AddGroup #

## Request Format ##

| **Parameter** | **Description** | **Type** | **Required** |
|:--------------|:----------------|:---------|:-------------|
|`RequestMethod`|AddGroup         |String    |Yes           |
|`GroupID`      |UUID of the group to create or update|UUID      |**Optional**|
|`Name`         |Name of the group|String    |Yes           |
|`ExtraData`    |Free form JSON data associated with this group|JSON      |Optional      |

  * `GroupID` is only required for updating an existing item

```
RequestMethod=AddGroup
&Name=Test+Group
```

## Response Format ##

| **Parameter** | **Description** | **Type** |
|:--------------|:----------------|:---------|
|`Success`      |True if an GroupID was returned, False if a Message was returned|Boolean   |
|`GroupID`      |UUID of the created or updated group|UUID      |
|`Message`      |Error message    |String    |

Success:

```
{
    "Success":true,
    "GroupID":"2cf49939-b9f0-45af-ad21-b3441de85f52"
}
```

Failure:

```
{
    "Success":false,
    "Message":"Invalid GroupID"
}
```