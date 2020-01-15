CREATE TABLE [dbo].[Visualizations] (
    [UserId]            NVARCHAR (32) NOT NULL,
    [MovieId]           INT           NOT NULL,
    [DateVisualization] DATETIME,
    CONSTRAINT [PK_Visualizations] PRIMARY KEY CLUSTERED ([UserId] ASC, [MovieId] ASC, [DateVisualization] ASC)
);

