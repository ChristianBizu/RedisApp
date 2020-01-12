CREATE TABLE [dbo].[movies] (
    [idMovie]    INT            NOT NULL,
    [nameMovie]  NVARCHAR (255) NOT NULL,
    [genreMovie] NVARCHAR (255) NOT NULL,
    CONSTRAINT [PK_movies] PRIMARY KEY CLUSTERED ([idMovie] ASC)
);

