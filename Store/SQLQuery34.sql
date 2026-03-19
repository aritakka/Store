ALTER TABLE ProductSuppliers DROP CONSTRAINT IF EXISTS PK_ProductSuppliers; -- если есть PK
ALTER TABLE ProductSuppliers ADD NewId INT IDENTITY(1,1);

UPDATE PS SET NewId = Id FROM ProductSuppliers PS; -- если нужно сохранить существующие данные (опционально)

ALTER TABLE ProductSuppliers DROP COLUMN Id;
EXEC sp_rename 'ProductSuppliers.NewId', 'Id', 'COLUMN';

ALTER TABLE ProductSuppliers ADD CONSTRAINT PK_ProductSuppliers PRIMARY KEY (Id);
SET IDENTITY_INSERT ProductSuppliers ON;

INSERT INTO ProductSuppliers (Id, ProductId, SupplierId) VALUES
(1,1,1),
(2,2,2),
(3,3,3);

SET IDENTITY_INSERT ProductSuppliers OFF;