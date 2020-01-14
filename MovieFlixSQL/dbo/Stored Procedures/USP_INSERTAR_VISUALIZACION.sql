
CREATE PROCEDURE [dbo].[USP_INSERTAR_VISUALIZACION]
	@idUser nvarchar(24),
	@idMovie int
AS
BEGIN

	INSERT INTO [dbo].[views]
	SELECT @idUser, @idMovie, GETDATE()

END