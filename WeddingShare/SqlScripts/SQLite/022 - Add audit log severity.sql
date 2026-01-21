--
-- Add `severity` to `audit_logs` table
--
ALTER TABLE `audit_logs` ADD `severity` INTEGER NOT NULL DEFAULT 2;