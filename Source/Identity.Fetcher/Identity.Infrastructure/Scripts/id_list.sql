﻿CREATE TYPE [dbo].[id_list] AS TABLE(
	[id] [bigint] NOT NULL,
	PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (IGNORE_DUP_KEY = OFF)
)