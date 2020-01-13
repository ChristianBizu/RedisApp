CREATE TABLE [dbo].[visualizations] (
    [idUser]   NVARCHAR (24) NOT NULL,
    [idMovie]  INT           NOT NULL,
    [dateView] DATETIME      NOT NULL,
    CONSTRAINT [PK_DatosIva] PRIMARY KEY CLUSTERED ([idUser] ASC, [idMovie] ASC, [dateView] ASC)
);

