CREATE TABLE [dbo].[Movies] (
    [MovieId]   INT            NOT NULL,
    [MovieName] NVARCHAR (255) NOT NULL,
    [GenreName]   NVARCHAR (32)  NOT NULL,
    CONSTRAINT [PK_Movies] PRIMARY KEY CLUSTERED ([MovieId] ASC)
);

