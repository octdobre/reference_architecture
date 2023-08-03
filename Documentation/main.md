# Bug Tracker Software: Squash

//Todo introduction & description

## Vocabulary

**Description** represents text information about the object.

**Fields** represent properties on a object.

**Values** represent individual unique values.

**Initialization** represents the default values for fields of an object at creation time.

**Actions** represent an act upon an object.

**Examples** represent example values for the properties of an object.

**Restrictions** represent actions that cannot be performed on an object under certain conditions.

**Event** represents actions that trigger the creation or modification of an object.

**Consequences** represent aditional actions that are performed by the system as a consequence of 
the user performing an action on a object.

**Notes** represent details about the usage.


## Core Domain Objects

### Bug (Bug)

Fields:
- Identifier
- Title
- Description
- Reporter
- Date & Time Reported
- Current active state 
- Active state log
- Latest historic state
- Historic state log
- Attachments
- Attachments log
- Current severity level
- Current severity level log
- Bug type

Initialization:
- Date & Time Reported is set automatically
- Current active state is set to `Open` and a log is created.
- A historic state log is created with the value 'Created' and the Latest historic state is set to it.
- Current severity level is set to `Low` if not specified and a log is created.

Actions:
- Create
- Read by Id
- Update
- Get Paginated 
    - Sorted by Title
- Delete by Id

- Reopen Bug

Consequences:
- Log item is produced for `Bug`
- Log item is produced for `Current active state`
- Log item is produced for `Current historic state`
- Log item is produced for `Current severity level`

---
### Active States (ActiveStates)

Fields:
- Identifier
- Value

Values:
- Open
- Closed

Events:
- Opening or Reopening `Bug` sets to 'Open'
- Closing `Bug` sets to 'Closed'

---
### Historic States (HistoricStates)

Fields:
- Identifier
- Value

Values:
- Created
- Modified
- Archived

Events:
- When a `Bug` is created then a log entry with the `Created` value is created.
- When a `Bug` is modified then a log entry with the `Modified` value is created.
- When a `Bug` is archived then a log entry with the `Archived` value is created.

---
### Severity Levels (SeverityLevels)

Fields:
- Identifier
- Value

Fields:
- Low
- Medium
- High

---
### Attachments  (Attachment)

Description:
- Represents aditional media that can be used to identify the bug.
- Media such as text, audio, image and video and other.

Fields:
- Identifier
- Identifier of the `Bug`
- Title
- Description
- Media Type:
    - Values:
        - text
        - image
        - audio
        - video
- Byte value or path
- Date & Time

Actions:
- Create attachment
- Update attachment values
- Delete

Restrictions:
- Title & Byte value or path must not be null

### Bug Code Type (BugCodeType)

Examples Values:
- frontend, backend, services, infra, gateway

Fields:
- Identifier
- Value (Unique) 

Actions:
- Add new `Bug Code Type`
- Update string value
- Read

Restrictions:
- A admin user can add, update, delete values.
- A normal user only reads the values.

### Bug Type (BugType)
Description:
Settable by used. A subjective view of a Bug.

Fields:
- Identifier
- Value (Unique) 

Values:
- Error -> Mistake in Code
- Flaw -> 
- Fault -> Mistake in Design

### Bug Reproduce Scenario (BugReproduceScenario)

Description:
- A list of steps that a person can perform to reproduce the bug.

Fields:
- Identifier
- Identifier of `Bug`
- Title
- Description
- Chance
    - Description:
        - Chance that the stepps will reproduce the bug
    - Example Values:
        - High
        - Medium
        - Low
- List of reproduce steps

Restrictions:
- A `Bug Reproduce Scenario` can only be created with at least one `Bug Reproduce Step`.

### Bug Reproduce Step

Fields:
- Identifier of `Bug Reproduce Scenario`
- Step Name
- Step description
- Attachment

## Bug Code File (BugCodeFile)

Description:
- Metadata about a code file for future insight.

Values:
- Identifier of `Bug`
- File name
- File path
- File programming language
- File line

Actions:
- Add
- Update

## Cross-Cutting Concerns Objects

### Log Item  (LogItem)

It is used to log specific actions performed in the system.

Fields:
- Link to a object
- Date & Time

### Audit 

Possible security audit for all actions.

Fields:
- User identifier
- User action
- User object
- Date & time

### Archive File

An archive file represents a archived object.
Objects that are deleted are always soft deleted and not
actually removed from the database.

- Link to deleted item
- Date & Time Archived

Event:
- Whenever a object is deleted.

## Bug Metric (BugMetric)

Description:
- Object used to measure and record actions in the bug tracking system in order
to provide future insight.


Values:
- Object Type Tracked
- Date & Time of recording

Notes:
- Can be used to measure things like time untill bug is closed, 
- If `Bug Code Type` is provided then bug can be measured on file/project/feature

## Support Objects

### Bug Chat (BugChat)

Values:
- Link to Bug
- List of chat messages
- Status of Chat
    - Values:
        - Open
        - Closed

Event:
- Status of chat is set to closed when a Bug is closed.
- A new chat is created when a bug is reopened and the old chat remains closed.

Restrictions:
- No more chat messages can be posted when a `Bug` is closed.

### Bug Chat Message (BugChatMessage)

Values:
- Message
- Date & Time of posting
- Writer

## Cross-Cutting Features

### AI Bug Fix Recommendation

### Cost of Fixing Estimate
