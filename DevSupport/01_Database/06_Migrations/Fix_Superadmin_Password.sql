SET QUOTED_IDENTIFIER ON
GO

UPDATE Utilizatori 
SET PasswordHash = N'$2a$11$U12D7ZTP5PJsXY.4u0Ngvu/soI990Wn6JtfksE.j5HltmRK.N.A0.' 
WHERE Username = 'superadmin'
GO

SELECT Username, LEN(PasswordHash) AS HashLength, PasswordHash 
FROM Utilizatori 
WHERE Username = 'superadmin'
GO
