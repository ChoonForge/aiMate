-- Migration: Add ApiKey table for Developer tier API access
-- Date: 2025-01-18
-- Description: Adds ApiKey entity to support REST API authentication

CREATE TABLE IF NOT EXISTS "ApiKeys" (
    "Id" uuid PRIMARY KEY,
    "UserId" uuid NOT NULL,
    "HashedKey" text NOT NULL,
    "Name" text NOT NULL,
    "Description" text NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastUsedAt" timestamp with time zone NULL,
    "IsRevoked" boolean NOT NULL DEFAULT false,
    "RevokedAt" timestamp with time zone NULL,
    "RevokedReason" text NULL,
    "RequestsPerMinute" integer NOT NULL DEFAULT 60,
    "RequestsPerDay" integer NOT NULL DEFAULT 10000,
    CONSTRAINT "FK_ApiKeys_Users_UserId" FOREIGN KEY ("UserId")
        REFERENCES "Users" ("Id") ON DELETE CASCADE
);

-- Indexes for performance
CREATE INDEX IF NOT EXISTS "IX_ApiKeys_UserId" ON "ApiKeys" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_ApiKeys_CreatedAt" ON "ApiKeys" ("CreatedAt");
CREATE INDEX IF NOT EXISTS "IX_ApiKeys_UserId_IsRevoked" ON "ApiKeys" ("UserId", "IsRevoked");

-- Comments for documentation
COMMENT ON TABLE "ApiKeys" IS 'API keys for Developer tier programmatic access';
COMMENT ON COLUMN "ApiKeys"."HashedKey" IS 'BCrypt hashed API key (never store plaintext)';
COMMENT ON COLUMN "ApiKeys"."LastUsedAt" IS 'Updated on each successful API request';
COMMENT ON COLUMN "ApiKeys"."RequestsPerMinute" IS 'Rate limit for API calls per minute';
COMMENT ON COLUMN "ApiKeys"."RequestsPerDay" IS 'Rate limit for API calls per day';
