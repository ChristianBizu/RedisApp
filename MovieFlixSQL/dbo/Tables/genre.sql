CREATE TABLE [dbo].[genre] (
    [genre] NVARCHAR (24) NOT NULL,
    [short] INT           NOT NULL,
    CONSTRAINT [PK_genre] PRIMARY KEY CLUSTERED ([genre] ASC)
);

