CREATE TABLE IF NOT EXISTS SchemaVersion (Version INTEGER PRIMARY KEY CHECK(Version=1));
INSERT OR IGNORE INTO SchemaVersion VALUES(1);
CREATE TABLE IF NOT EXISTS User (
 Id TEXT PRIMARY KEY, Enabled INTEGER NOT NULL CHECK(Enabled IN(0,1)),
 RolesMask INTEGER NOT NULL CHECK(RolesMask BETWEEN 0 AND 3),
 AuthorizationVersion INTEGER NOT NULL CHECK(AuthorizationVersion>0)
);
CREATE TABLE IF NOT EXISTS AccessToken (
 TokenHash BLOB PRIMARY KEY CHECK(length(TokenHash)=32),
 UserId TEXT NOT NULL REFERENCES User(Id) ON DELETE RESTRICT,
 Enabled INTEGER NOT NULL CHECK(Enabled IN(0,1))
);
CREATE TABLE IF NOT EXISTS ChangeRequest (
 Id TEXT PRIMARY KEY, Title TEXT NOT NULL, Description TEXT NULL, Reason TEXT NULL,
 RequesterId TEXT NOT NULL REFERENCES User(Id) ON DELETE RESTRICT,
 Status TEXT NOT NULL CHECK(Status IN('Draft','PendingReview','ChangesRequested','Approved')),
 Version INTEGER NOT NULL CHECK(Version>0),
 ReviewBaselineTitle TEXT NULL, ReviewBaselineDescription TEXT NULL, ReviewBaselineReason TEXT NULL,
 CHECK(Status!='ChangesRequested' OR ReviewBaselineTitle IS NOT NULL)
);
CREATE INDEX IF NOT EXISTS IX_RequesterId_Id ON ChangeRequest(RequesterId,Id);
CREATE INDEX IF NOT EXISTS IX_Status_Id ON ChangeRequest(Status,Id);
CREATE TABLE IF NOT EXISTS OperationResult (
 ActorId TEXT NOT NULL REFERENCES User(Id) ON DELETE RESTRICT,
 OperationKind TEXT NOT NULL CHECK(OperationKind IN('Create','ReplaceContent','Submit','Approve','RequestChanges','Delete')),
 IdempotencyKey TEXT NOT NULL COLLATE BINARY CHECK(length(IdempotencyKey) BETWEEN 1 AND 128),
 Fingerprint BLOB NOT NULL CHECK(length(Fingerprint)=32),
 TargetId TEXT NULL, HttpStatus INTEGER NOT NULL,
 ResultCode TEXT NOT NULL, ResponseJson TEXT NOT NULL, Location TEXT NULL,
 PRIMARY KEY(ActorId,OperationKind,IdempotencyKey)
);
