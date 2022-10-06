CREATE VIEW ProductsWithFeature
AS
SELECT P.Id, P.Name ProductName, C.Name CategoryName, PF.Width ProductWidth, PF.Height ProductHeight FROM Products P
JOIN Categories C ON C.Id = P.CategoryId
JOIN ProductFeatures PF ON P.Id = PF.Id;
