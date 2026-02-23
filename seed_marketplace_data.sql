-- =====================================================
-- SQL Seed Script for Testify Marketplace
-- Run this in SSMS against your local Testify DB
-- =====================================================
-- This script:
--   1. Inserts TemplateCategories
--   2. Inserts TemplateTags
--   3. Inserts TestSuiteTemplates (linked to a real User + Category)
--   4. Inserts TestSuiteTemplateTags (junction table)
-- =====================================================

-- ===================== STEP 0: Find your User ID =====================
-- IMPORTANT: Replace the @UserId value below with a REAL UserId from your AspNetUsers table.
-- Run this query first to find one:
--   SELECT TOP 1 Id, UserName FROM AspNetUsers;

DECLARE @UserId NVARCHAR(450);
SET @UserId = (SELECT TOP 1 Id FROM AspNetUsers);

-- Safety check: if no user exists, abort
IF @UserId IS NULL
BEGIN
    PRINT 'ERROR: No users found in AspNetUsers. Please register an account first.';
    RETURN;
END

PRINT 'Using UserId: ' + @UserId;

-- ===================== STEP 1: Insert Categories =====================
SET IDENTITY_INSERT TemplateCategories ON;

-- Only insert if not already existing
IF NOT EXISTS (SELECT 1 FROM TemplateCategories WHERE Id = 1)
    INSERT INTO TemplateCategories (Id, Name, Description, ParentCategoryId) 
    VALUES (1, 'E-Commerce', 'Test suites for e-commerce platforms', NULL);

IF NOT EXISTS (SELECT 1 FROM TemplateCategories WHERE Id = 2)
    INSERT INTO TemplateCategories (Id, Name, Description, ParentCategoryId) 
    VALUES (2, 'FinTech', 'Test suites for financial technology', NULL);

IF NOT EXISTS (SELECT 1 FROM TemplateCategories WHERE Id = 3)
    INSERT INTO TemplateCategories (Id, Name, Description, ParentCategoryId) 
    VALUES (3, 'Healthcare', 'Test suites for healthcare applications', NULL);

IF NOT EXISTS (SELECT 1 FROM TemplateCategories WHERE Id = 4)
    INSERT INTO TemplateCategories (Id, Name, Description, ParentCategoryId) 
    VALUES (4, 'SaaS', 'Test suites for SaaS applications', NULL);

SET IDENTITY_INSERT TemplateCategories OFF;

-- ===================== STEP 2: Insert Tags =====================
SET IDENTITY_INSERT TemplateTags ON;

IF NOT EXISTS (SELECT 1 FROM TemplateTags WHERE Id = 1)
    INSERT INTO TemplateTags (Id, TagName) VALUES (1, 'regression');
IF NOT EXISTS (SELECT 1 FROM TemplateTags WHERE Id = 2)
    INSERT INTO TemplateTags (Id, TagName) VALUES (2, 'smoke');
IF NOT EXISTS (SELECT 1 FROM TemplateTags WHERE Id = 3)
    INSERT INTO TemplateTags (Id, TagName) VALUES (3, 'api');
IF NOT EXISTS (SELECT 1 FROM TemplateTags WHERE Id = 4)
    INSERT INTO TemplateTags (Id, TagName) VALUES (4, 'ui');
IF NOT EXISTS (SELECT 1 FROM TemplateTags WHERE Id = 5)
    INSERT INTO TemplateTags (Id, TagName) VALUES (5, 'security');
IF NOT EXISTS (SELECT 1 FROM TemplateTags WHERE Id = 6)
    INSERT INTO TemplateTags (Id, TagName) VALUES (6, 'payment');
IF NOT EXISTS (SELECT 1 FROM TemplateTags WHERE Id = 7)
    INSERT INTO TemplateTags (Id, TagName) VALUES (7, 'performance');
IF NOT EXISTS (SELECT 1 FROM TemplateTags WHERE Id = 8)
    INSERT INTO TemplateTags (Id, TagName) VALUES (8, 'compliance');

SET IDENTITY_INSERT TemplateTags OFF;

-- ===================== STEP 3: Insert Templates =====================
-- KEY FIELDS: IsPublic = 1 (REQUIRED for API to return them!)
--             IsDeleted = 0 (not soft-deleted)
--             UserId = a real user from AspNetUsers

SET IDENTITY_INSERT TestSuiteTemplates ON;

IF NOT EXISTS (SELECT 1 FROM TestSuiteTemplates WHERE Id = 1)
    INSERT INTO TestSuiteTemplates 
    (Id, Name, Description, CategoryId, UserId, IsPublic, IsDeleted, ViewCount, CloneCount, TotalStarred, FolderId, ShareCode, CreatedAt, CreatedBy) 
    VALUES 
    (1, 'E-Commerce Checkout Flow', 'Complete test suite covering cart, checkout, payment processing, and order confirmation for e-commerce platforms.', 
     1, @UserId, 1, 0, 245, 89, 1200, NULL, NULL, GETDATE(), @UserId);

IF NOT EXISTS (SELECT 1 FROM TestSuiteTemplates WHERE Id = 2)
    INSERT INTO TestSuiteTemplates 
    (Id, Name, Description, CategoryId, UserId, IsPublic, IsDeleted, ViewCount, CloneCount, TotalStarred, FolderId, ShareCode, CreatedAt, CreatedBy) 
    VALUES 
    (2, 'Payment Gateway Integration', 'Regression test suite for Stripe, PayPal, and VNPay payment gateway integrations.', 
     2, @UserId, 1, 0, 180, 67, 950, NULL, NULL, GETDATE(), @UserId);

IF NOT EXISTS (SELECT 1 FROM TestSuiteTemplates WHERE Id = 3)
    INSERT INTO TestSuiteTemplates 
    (Id, Name, Description, CategoryId, UserId, IsPublic, IsDeleted, ViewCount, CloneCount, TotalStarred, FolderId, ShareCode, CreatedAt, CreatedBy) 
    VALUES 
    (3, 'User Authentication & Authorization', 'Security-focused test suite for login, registration, JWT tokens, role-based access, and 2FA.', 
     4, @UserId, 1, 0, 320, 145, 2100, NULL, NULL, GETDATE(), @UserId);

IF NOT EXISTS (SELECT 1 FROM TestSuiteTemplates WHERE Id = 4)
    INSERT INTO TestSuiteTemplates 
    (Id, Name, Description, CategoryId, UserId, IsPublic, IsDeleted, ViewCount, CloneCount, TotalStarred, FolderId, ShareCode, CreatedAt, CreatedBy) 
    VALUES 
    (4, 'HIPAA Compliance Testing', 'Healthcare compliance test suite covering data privacy, audit logs, and PHI handling.', 
     3, @UserId, 1, 0, 95, 34, 500, NULL, NULL, GETDATE(), @UserId);

IF NOT EXISTS (SELECT 1 FROM TestSuiteTemplates WHERE Id = 5)
    INSERT INTO TestSuiteTemplates 
    (Id, Name, Description, CategoryId, UserId, IsPublic, IsDeleted, ViewCount, CloneCount, TotalStarred, FolderId, ShareCode, CreatedAt, CreatedBy) 
    VALUES 
    (5, 'REST API Smoke Tests', 'Quick smoke test suite for verifying API endpoints, status codes, and response formats.', 
     4, @UserId, 1, 0, 410, 210, 3200, NULL, NULL, GETDATE(), @UserId);

IF NOT EXISTS (SELECT 1 FROM TestSuiteTemplates WHERE Id = 6)
    INSERT INTO TestSuiteTemplates 
    (Id, Name, Description, CategoryId, UserId, IsPublic, IsDeleted, ViewCount, CloneCount, TotalStarred, FolderId, ShareCode, CreatedAt, CreatedBy) 
    VALUES 
    (6, 'Product Catalog Search & Filter', 'E-Commerce product search, filtering, sorting, and pagination test coverage.', 
     1, @UserId, 1, 0, 150, 52, 780, NULL, NULL, GETDATE(), @UserId);

SET IDENTITY_INSERT TestSuiteTemplates OFF;

-- ===================== STEP 4: Link Tags to Templates =====================
-- Template 1: E-Commerce Checkout → regression, ui, payment
IF NOT EXISTS (SELECT 1 FROM TestSuiteTemplateTags WHERE TemplateId = 1 AND TagId = 1)
    INSERT INTO TestSuiteTemplateTags (TemplateId, TagId) VALUES (1, 1);
IF NOT EXISTS (SELECT 1 FROM TestSuiteTemplateTags WHERE TemplateId = 1 AND TagId = 4)
    INSERT INTO TestSuiteTemplateTags (TemplateId, TagId) VALUES (1, 4);
IF NOT EXISTS (SELECT 1 FROM TestSuiteTemplateTags WHERE TemplateId = 1 AND TagId = 6)
    INSERT INTO TestSuiteTemplateTags (TemplateId, TagId) VALUES (1, 6);

-- Template 2: Payment Gateway → api, security, payment
IF NOT EXISTS (SELECT 1 FROM TestSuiteTemplateTags WHERE TemplateId = 2 AND TagId = 3)
    INSERT INTO TestSuiteTemplateTags (TemplateId, TagId) VALUES (2, 3);
IF NOT EXISTS (SELECT 1 FROM TestSuiteTemplateTags WHERE TemplateId = 2 AND TagId = 5)
    INSERT INTO TestSuiteTemplateTags (TemplateId, TagId) VALUES (2, 5);
IF NOT EXISTS (SELECT 1 FROM TestSuiteTemplateTags WHERE TemplateId = 2 AND TagId = 6)
    INSERT INTO TestSuiteTemplateTags (TemplateId, TagId) VALUES (2, 6);

-- Template 3: Auth → security, api, regression
IF NOT EXISTS (SELECT 1 FROM TestSuiteTemplateTags WHERE TemplateId = 3 AND TagId = 5)
    INSERT INTO TestSuiteTemplateTags (TemplateId, TagId) VALUES (3, 5);
IF NOT EXISTS (SELECT 1 FROM TestSuiteTemplateTags WHERE TemplateId = 3 AND TagId = 3)
    INSERT INTO TestSuiteTemplateTags (TemplateId, TagId) VALUES (3, 3);
IF NOT EXISTS (SELECT 1 FROM TestSuiteTemplateTags WHERE TemplateId = 3 AND TagId = 1)
    INSERT INTO TestSuiteTemplateTags (TemplateId, TagId) VALUES (3, 1);

-- Template 4: HIPAA → compliance, security
IF NOT EXISTS (SELECT 1 FROM TestSuiteTemplateTags WHERE TemplateId = 4 AND TagId = 8)
    INSERT INTO TestSuiteTemplateTags (TemplateId, TagId) VALUES (4, 8);
IF NOT EXISTS (SELECT 1 FROM TestSuiteTemplateTags WHERE TemplateId = 4 AND TagId = 5)
    INSERT INTO TestSuiteTemplateTags (TemplateId, TagId) VALUES (4, 5);

-- Template 5: REST API → smoke, api, performance
IF NOT EXISTS (SELECT 1 FROM TestSuiteTemplateTags WHERE TemplateId = 5 AND TagId = 2)
    INSERT INTO TestSuiteTemplateTags (TemplateId, TagId) VALUES (5, 2);
IF NOT EXISTS (SELECT 1 FROM TestSuiteTemplateTags WHERE TemplateId = 5 AND TagId = 3)
    INSERT INTO TestSuiteTemplateTags (TemplateId, TagId) VALUES (5, 3);
IF NOT EXISTS (SELECT 1 FROM TestSuiteTemplateTags WHERE TemplateId = 5 AND TagId = 7)
    INSERT INTO TestSuiteTemplateTags (TemplateId, TagId) VALUES (5, 7);

-- Template 6: Product Catalog → regression, ui
IF NOT EXISTS (SELECT 1 FROM TestSuiteTemplateTags WHERE TemplateId = 6 AND TagId = 1)
    INSERT INTO TestSuiteTemplateTags (TemplateId, TagId) VALUES (6, 1);
IF NOT EXISTS (SELECT 1 FROM TestSuiteTemplateTags WHERE TemplateId = 6 AND TagId = 4)
    INSERT INTO TestSuiteTemplateTags (TemplateId, TagId) VALUES (6, 4);

-- ===================== VERIFICATION =====================
-- Run these queries to verify data was inserted correctly:

SELECT 'Templates:' AS [Check], COUNT(*) AS [Count] FROM TestSuiteTemplates WHERE IsPublic = 1 AND IsDeleted = 0;
SELECT 'Categories:' AS [Check], COUNT(*) AS [Count] FROM TemplateCategories;
SELECT 'Tags:' AS [Check], COUNT(*) AS [Count] FROM TemplateTags;
SELECT 'TagLinks:' AS [Check], COUNT(*) AS [Count] FROM TestSuiteTemplateTags;

-- Preview what the API will return:
SELECT 
    t.Id, t.Name, t.Description, t.IsPublic, t.IsDeleted,
    c.Name AS CategoryName,
    u.UserName AS AuthorName,
    t.TotalStarred AS Stars, t.CloneCount AS Clones, t.ViewCount AS [Views]
FROM TestSuiteTemplates t
LEFT JOIN TemplateCategories c ON t.CategoryId = c.Id
LEFT JOIN AspNetUsers u ON t.UserId = u.Id
WHERE t.IsPublic = 1 AND t.IsDeleted = 0;

PRINT '✅ Seed data inserted successfully!';
